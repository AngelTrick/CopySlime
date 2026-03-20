using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : Singleton<StageManager>
{
    public Stage stageController;
    public StageData currentStageData;
    private int _currentRewardCount = 0; //킬게이지

    public int totalGold = 0; //플레이어가 현재 가진 총 골드

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

    public System.Action<int> OnGoldChanged;

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
    }
    void Start()
    {
        if (DataManager.Instance != null && allStageDatas.Length > 0)
        {
            _currentStageIndex = (DataManager.Instance.CurrentStage - 1) % allStageDatas.Length;
            currentStageData = allStageDatas[_currentStageIndex];
        }
        else if (allStageDatas.Length > 0)
        {
            currentStageData = allStageDatas[_currentStageIndex];
        }
        SpawnNextWave(); //게임 시작 시 첫 소환
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
    public void ChallengeBoss()
    {
        if (stageController.isBossLevel) return;

        if (currentStageData != null && currentStageData.stageBoss != null)
        {
            CancelInvoke("SpawnNextWave");

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

            stageController.EnterBossMap();

            stageController.StartNewWave(currentStageData.stageBoss);

            _currentBossLimitTime = currentStageData.stageBoss.bossTimeLimit;
            _currentBossTimer = _currentBossLimitTime;

            _isTimerRunning = true;
            Debug.Log($"보스전 시작! 제한 시간: {currentStageData.stageBoss.monsterName}초");
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

        int actualLevel;
        if (DataManager.Instance != null)
        {
            actualLevel = DataManager.Instance.CurrentStage;
        }
        else
        {
            actualLevel = 1;
        }

        int zoneIndex = (actualLevel - 1) / stagesPerTheme; //스테이지 간격

        if (allStageDatas.Length > 0)
        {
            _currentStageIndex = zoneIndex % allStageDatas.Length;
            currentStageData = allStageDatas[_currentStageIndex];
        }

        _currentRewardCount = 0; //게이지 초기화
       
        stageController.ReturnToField();
        SpawnNextWave();
    }
    public void AddGold(int amount) //골드 획득 부분
    {
        totalGold += amount;
        OnGoldChanged?.Invoke(totalGold); //골드 획득 사운드 등 이벤트
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

        if (_currentRewardCount >= currentStageData.rewardGoalCount)
        {
            GiveReward();
            _currentRewardCount = 0; //게이지 초기화
        }
    }
    private void GiveReward()
    {
        if (treasureChestPrefab != null)
        {
            int actualLevel = GetCurrentLevel();

            float growth = currentStageData.monsterGrowthRate;
            float exponentialMultiplier = Mathf.Pow(growth, actualLevel - 1);

            int finalChestGold = Mathf.RoundToInt(currentStageData.baseRewardGold * currentStageData.rewardMultiplier * exponentialMultiplier);

            Vector3 spawnPos = new Vector3(stageController.bossSpawnPos, 0f, 0f);

            GameObject chestGo = PoolManager.Instance.Pop(treasureChestPrefab, spawnPos, Quaternion.identity);

            TreasureChest chestScript = chestGo.GetComponent<TreasureChest>();
            if (chestScript != null)
            {
                //계산된 최종 골드를 상자에 주입
                chestScript.Init(finalChestGold);
            }

            stageController.activeMonsters.Add(chestGo);
            CancelInvoke("SpawnNextWave");
        }
    }
    public void OnBossChallengeFailed()
    {
        if (stageController.isBossLevel)
        {
            _isTimerRunning = false;

            foreach (GameObject m in stageController.activeMonsters) //소환된 보스 몬스터 제거
            {
                if (m != null) PoolManager.Instance.Push(m);
            }
            stageController.activeMonsters.Clear();

            stageController.ReturnToField(); //스테이지 상태 복구

            SpawnNextWave(); //다시 일반 소환 루프 진행

        }
    }
    public void OnWaveCompleted()
    {
        if (!stageController.isBossLevel)
        {
            if (stageController.activeMonsters.Count > 0) return;

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
