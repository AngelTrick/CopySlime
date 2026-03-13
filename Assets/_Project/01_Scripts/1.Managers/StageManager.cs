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

    [Header("보스전 타임어택 설정")]
    private float _currentBossLimitTime;
    private float _currentBossTimer;
    private bool _isTimerRunning = false;

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
        if (allStageDatas == null || allStageDatas.Length == 0)
        {
            Debug.LogError("StageManager: Stage Data가 할당되지 않았습니다!");
        }
    }
    void Start()
    {
        if (allStageDatas.Length > 0)
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
            Debug.Log("테스트: 보스 도전 실패 강제 호출");
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
        Debug.Log("보스 처치 성공!");
        _isTimerRunning = false;
        GoToNextStage();
    }
    public void GoToNextStage()
    {
        _currentStageIndex++;

        if (_currentStageIndex >= allStageDatas.Length)
        {
            _currentStageIndex = allStageDatas.Length - 1;
        }

        currentStageData = allStageDatas[_currentStageIndex];
        _currentRewardCount = 0; //게이지 초기화

        stageController.ReturnToField();
        SpawnNextWave();
    }
    public void AddGold(int amount)
    {
        totalGold += amount;
    }
    //스테이지 레벨
    public int GetCurrentLevel()
    {
        if (currentStageData != null)
        {
            return currentStageData.stageLevel;
        }
        else
        {
            return 0;
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
        //기본 보상 * 스테이지 보상 배율
        int finalReward = Mathf.RoundToInt(currentStageData.baseRewardGold * currentStageData.rewardMultiplier);

        totalGold += finalReward;
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

            Debug.Log("보스전 실패: 일반 필드로 돌아갑니다.");
        }
    }
    public void OnWaveCompleted()
    {
        if (!stageController.isBossLevel)
        {
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
