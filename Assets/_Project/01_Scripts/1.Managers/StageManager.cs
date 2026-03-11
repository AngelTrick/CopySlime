using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    public Stage stageController; 
    public StageData currentStageData;
    private int _currentRewardCount = 0;

    public int totalGold = 0; //플레이어가 현재 가진 총 골드

    [Header("스테이지 관리")]
    public StageData[] allStageDatas; //스테이지 데이터 리스트
    private int _currentStageIndex = 0; //현재 몇 번째 스테이지인지 저장

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }
    void Start()
    {
        if (allStageDatas.Length > 0)
        {
            currentStageData = allStageDatas[_currentStageIndex];
        }
        SpawnNextWave(); //게임 시작 시 첫 소환
    }
    public void GoToNextStage()
    {
        _currentStageIndex++;

        if (_currentStageIndex < allStageDatas.Length)
        {
            currentStageData = allStageDatas[_currentStageIndex];

            SpawnNextWave();
        }
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

    public void OnWaveCompleted()
    {
        Invoke("SpawnNextWave", 1.5f); //보스전 중이 아니라면 무조건 다음 웨이브 소환 (무한 반복)
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
