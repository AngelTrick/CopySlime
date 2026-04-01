using System.Collections;
using UnityEngine;

public class StageManager : Singleton<StageManager>
{
    public Stage stageController;
    public StageData currentStageData;
    private int _currentRewardCount = 0; //킬게이지

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

    [Header("보물 상자 설정")]
    public GameObject treasureChestPrefab;

    [Header("UI 연결")]
    public UIStage uiStage;

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
    public void ChallengeBoss()
    {
        if (stageController.isBossLevel) return;

        if (currentStageData != null && currentStageData.stageBoss != null)
        {
            CancelInvoke("SpawnNextWave");

            ClearCurrentMonsters();

            stageController.EnterBossMap();

            stageController.StartNewWave(currentStageData.stageBoss);

            _currentBossLimitTime = currentStageData.stageBoss.bossTimeLimit;
            _currentBossTimer = _currentBossLimitTime;

            _isTimerRunning = true;
        }
    }
    public void OnBossClear()
    {
        _isTimerRunning = false;
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

        DataManager.Instance.AddGold(finalAmount);
        OnGoldChanged?.Invoke(totalGold); //골드 획득 사운드 등 이벤트
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
            GiveReward();
            _currentRewardCount = 0; //게이지 초기화

            if (uiStage != null) uiStage.ResetBar();
        }
    }
    private void GiveReward()
    {
        if (treasureChestPrefab != null && currentStageData.stageTreasure != null)
        {
            CancelInvoke("SpawnNextWave");

            ClearCurrentMonsters();

            int actualLevel = GetCurrentLevel();

            double growth = currentStageData.monsterGrowthRate;
            double exponentialMultiplier = System.Math.Pow(growth, actualLevel - 1);

            double finalChestGold = (double)currentStageData.baseRewardGold * (double)currentStageData.rewardMultiplier * exponentialMultiplier;

            Vector3 spawnPos = new Vector3(stageController.bossSpawnPos, stageController.spawnHeight, 0f);

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
    }
    public void OnBossChallengeFailed()
    {
        if (stageController.isBossLevel)
        {
            _isTimerRunning = false;

            ClearCurrentMonsters();

            stageController.ReturnToField(); //스테이지 상태 복구

            SpawnNextWave(); //다시 일반 소환 루프 진행

        }
    }
    public void OnWaveCompleted()
    {
        if (!stageController.isBossLevel)
        {
            if (stageController.activeMonsters.Count > 0) return;

            CancelInvoke("SpawnNextWave");
            Invoke("SpawnNextWave", 1.5f); //보스전 중이 아니라면 무조건 다음 웨이브 소환 (무한 반복)
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
