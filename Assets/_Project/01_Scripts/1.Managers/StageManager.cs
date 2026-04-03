using System.Collections;
using UnityEngine;

public class StageManager : Singleton<StageManager>
{
    public Stage stageController;
    public StageData currentStageData;
    private int _currentRewardCount = 0; //킬게이지
    private bool _isRewardPending = false;

    public double totalGold //플레이어가 현재 가진 총 골드
    {
        get
        {
            return DataManager.Instance.Gold;
        }
    }
    public int totalSkinShards //플레이어가 현재 가진 총 조각
    {
        get
        {
            return DataManager.Instance.CopyFragments;
        }
    }

    [Header("스테이지 관리")]
    public StageData[] allStageDatas; //스테이지 데이터 리스트
    private int _currentStageIndex = 0; //현재 몇 번째 스테이지인지 저장

    [Header("테마 변경 설정")]
    public int stagesPerTheme = 20; //기본값 20

    [Header("보스전 타임어택 설정")]
    private float _currentBossLimitTime; //보스전 한계 시간
    private float _currentBossTimer; //보스전 타이머
    private bool _isTimerRunning = false;

    [Header("UI 연결")]
    public UIStage uiStage;

    [Header("UI 제어")]
    public GameObject stageUIObject;

    [Header("보스 UI 연결")]
    public BossBattleUI uiBossBattle;

    [Header("보물 상자 설정")]
    public GameObject treasureChestPrefab;
    public float treasureSpawnDelay = 1.5f; //상자 스폰 시간
    public float treasureSpawnOffset = 5.0f; //상자 스폰 위치

    [Header("웨이브 설정")]
    public float monsterSpawnDelay = 1.5f; //웨이브 시간

    public System.Action<double> OnGoldChanged;
    public System.Action<int> OnSkinShardChanged;

    private PlayerController _player;

    public float GetBossTimerProgress()
    {
        float maxTime = _currentBossLimitTime;
        if (maxTime <= 0)
        {
            maxTime = 0.1f;
        }
        float progress = _currentBossTimer / maxTime;

        if (progress > 1.0f) progress = 1.0f;
        if (progress < 0.0f) progress = 0.0f;

        return progress;
    }

    protected override void Awake()
    {
        base.Awake();
        InitializeInitialStage();
    }
    private void InitializeInitialStage()
    {
        if (allStageDatas == null || allStageDatas.Length == 0) return;

        if (DataManager.Instance != null)
        {
            int actualLevel = DataManager.Instance.CurrentStage;
            _currentStageIndex = ((actualLevel - 1) / stagesPerTheme) % allStageDatas.Length;
        }
        else
        {
            _currentStageIndex = 0;
        }

        currentStageData = allStageDatas[_currentStageIndex];

        if (stageController != null && currentStageData.backgroundPrefab != null)
        {
            stageController.ChangeStageBackground(currentStageData.backgroundPrefab);
        }
    }
    void Start()
    {
        _player = FindObjectOfType<PlayerController>();

        if (GameManager.Instance == null)
        {
            SpawnNextWave();
        }
        else
        {
            GameManager.Instance.OnStateChanged += HandleGameStateChanged;

            if (GameManager.Instance.CurrentState == GameManager.GameState.StageFarming || GameManager.Instance.CurrentState == GameManager.GameState.Boot)
            {
                if (stageController != null && stageController.activeMonsters.Count == 0)
                {
                    SpawnNextWave();
                }
            }

        }
    }
    void Update()
    {
        if (_isTimerRunning)
        {
            _currentBossTimer -= Time.deltaTime;
            if (_currentBossTimer <= 0)
            {
                OnBossChallengeFailed(); //시간 종료 시 실패 처리
            }
        }
        if (Input.GetKeyDown(KeyCode.F)) //테스트 용
        {
            OnBossChallengeFailed();
        }
    }
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleGameStateChanged;
        }
    }
    private void HandleGameStateChanged(GameManager.GameState oldState, GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.StageFarming)
        {
            if (stageController != null && stageController.activeMonsters.Count == 0 && !stageController.isBossLevel)
            {
                SpawnNextWave();
            }
        }
    }
    public void ClearCurrentMonsters()
    {
        if (stageController != null && stageController.activeMonsters != null)
        {
            foreach (GameObject m in stageController.activeMonsters)
            {
                if (m != null)
                {
                    if (PoolManager.Instance != null)
                        PoolManager.Instance.Push(m);
                    else
                        Destroy(m);
                }
            }
            stageController.activeMonsters.Clear();
        }
    }
    public void ChallengeBoss() //보스 도전
    {
        if (stageController.isBossLevel) return;

        if (currentStageData != null && currentStageData.stageBoss != null)
        {
            if (stageUIObject != null) stageUIObject.SetActive(false); //일반 스테이지 UI 숨김

            CancelInvoke("SpawnNextWave");

            ClearCurrentMonsters();

            stageController.EnterBossMap();

            stageController.StartNewWave(currentStageData.stageBoss);

            Invoke("SafeOpenUI", 0.1f);


            _currentBossLimitTime = currentStageData.stageBoss.bossTimeLimit;
            _currentBossTimer = _currentBossLimitTime;

            _isTimerRunning = true;
        }
    }
    private void SafeOpenUI()
    {
        if (uiBossBattle != null && stageController.activeMonsters.Count > 0)
        {
            Monster bossScript = stageController.activeMonsters[0].GetComponent<Monster>();
            string title = $"STAGE {GetCurrentLevel()} {currentStageData.stageName}";
            uiBossBattle.Open(bossScript, title);
        }
    }
    private void RestoreStageUI()
    {
        if (stageUIObject != null) stageUIObject.SetActive(true);
    }
    public void OnBossClear()
    {
        _isTimerRunning = false;
        if (uiBossBattle != null) uiBossBattle.Close();
        StartCoroutine(WaitAndGoToNextStage(2.0f));
    }
    private IEnumerator WaitAndGoToNextStage(float delay)
    {
        yield return new WaitForSeconds(delay);
        GoToNextStage();
    }
    public void GoToNextStage()
    {
        if (DataManager.Instance != null)
        {
            DataManager.Instance.StageCleared(); //실제 스테이지 번호 증가
        }

        int actualLevel = GetCurrentLevel();

        _currentStageIndex = ((actualLevel - 1) / stagesPerTheme) % allStageDatas.Length;
        currentStageData = allStageDatas[_currentStageIndex];

        _currentRewardCount = 0;

        if (stageController != null && currentStageData.backgroundPrefab != null)
        {
            stageController.ChangeStageBackground(currentStageData.backgroundPrefab);
        }

        RestoreStageUI();

        ClearCurrentMonsters();

        stageController.ReturnToField();
        SpawnNextWave();
    }
    public void AddGold(double amount) //골드 획득 부분
    {
        double finalAmount = amount;

        if (_player != null)
        {
            finalAmount = (double)_player.FarmGold((float)amount); //플레이어 컨트롤러 골드 더블 수정 전 사용
            //finalAmount = _player.FarmGold(amount);
        }

        if (DataManager.Instance != null)
        {
            DataManager.Instance.AddGold(finalAmount);
            OnGoldChanged?.Invoke(totalGold);
        }
    }
    public void AddSkinShard(int amount)
    {
        DataManager.Instance.Addfragments(amount);
        OnSkinShardChanged?.Invoke(totalSkinShards);
    }
    //스테이지 레벨
    public int GetCurrentLevel()
    {
        if (DataManager.Instance != null)
        {
            return DataManager.Instance.CurrentStage;
        }
        else
        {
            return 1;
        }
    }
    //보상 게이지
    public float GetKillGaugeProgress()
    {
        if (currentStageData == null || currentStageData.rewardGoalCount == 0)
        {
            return 0f;
        }
        //현재 잡은 수 / 목표 수를 계산.
        return (float)_currentRewardCount / currentStageData.rewardGoalCount;
    }
    //스테이지 이름
    public string GetStageName()
    {
        if (currentStageData != null)
        {
            return currentStageData.stageName;
        }
        else
        {
            return "Unknown";
        }
    }
    public void AddKillCount()
    {
        _currentRewardCount++;

        if (uiStage != null)
        {
            uiStage.UpdateFillBar();
        }

        if (_currentRewardCount >= currentStageData.rewardGoalCount)
        {
            _isRewardPending = true;
            _currentRewardCount = 0; //게이지 초기화

            if (uiStage != null) uiStage.ResetBar();
        }
    }
    private void GiveReward()
    {
        if (currentStageData == null || currentStageData.stageTreasure == null)
        {
            return;
        }

        CancelInvoke(nameof(SpawnNextWave));

        stageController.activeMonsters.Clear();

        int actualLevel = GetCurrentLevel();

        double growth = currentStageData.monsterGrowthRate;
        double exponentialMultiplier = System.Math.Pow(growth, actualLevel - 1);

        double finalChestGold = (double)currentStageData.baseRewardGold * (double)currentStageData.rewardMultiplier * exponentialMultiplier;

        float finalX = stageController.bossSpawnPos + treasureSpawnOffset;
        Vector3 spawnPos = new Vector3(finalX, stageController.spawnHeight, 0f);

        GameObject chestGo = PoolManager.Instance.Pop(currentStageData.stageTreasure.chestPrefab, spawnPos, Quaternion.identity);

        chestGo.transform.localScale = Vector3.one * 1.5f; //상자 크기 조절

        TreasureChest chestScript = chestGo.GetComponent<TreasureChest>();
        if (chestScript != null)
        {
            //계산된 최종 골드를 상자에 주입
            chestScript.Init(currentStageData.stageTreasure, finalChestGold);
        }

        stageController.activeMonsters.Add(chestGo);

        if (stageController != null)
        {
            stageController.StopAllCoroutines();
            stageController.StartCoroutine("MoveWorldRoutine");
        }

    }
    public void OnBossChallengeFailed()
    {
        if (stageController.isBossLevel)
        {
            _isTimerRunning = false;

            if (uiBossBattle != null) uiBossBattle.Close();

            RestoreStageUI();

            ClearCurrentMonsters();

            stageController.ReturnToField(); //스테이지 상태 복구

            Invoke(nameof(SpawnNextWave), monsterSpawnDelay); //다시 일반 소환 루프 진행

        }
    }
    public void OnWaveCompleted()
    {
        if (!stageController.isBossLevel)
        {
            if (_isRewardPending)
            {
                _isRewardPending = false;

                Invoke(nameof(GiveReward), treasureSpawnDelay); //상자 소환 시간 1.5초
            }
            else
            {
                Invoke(nameof(SpawnNextWave), monsterSpawnDelay); //일반 웨이브는 기존처럼 1.5초 뒤에 소환
            }
        }
    }
    private void SpawnNextWave()
    {
        if (currentStageData != null && stageController != null)
        {
            //여러 일반 몬스터 중 랜덤으로 하나 전달
            int rand = Random.Range(0, currentStageData.fieldMonsters.Length);
            stageController.StartNewWave(currentStageData.fieldMonsters[rand]);
        }
    }
}
