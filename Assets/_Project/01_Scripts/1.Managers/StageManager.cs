using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    public Stage stageController; 
    public StageData currentStageData;
    private int currentRewardCount = 0;

    public int totalGold = 0; //플레이어가 현재 가진 총 골드

    [Header("스테이지 관리")]
    public StageData[] allStageDatas; //스테이지 데이터 리스트
    private int currentStageIndex = 0; //현재 몇 번째 스테이지인지 저장

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
            currentStageData = allStageDatas[currentStageIndex];
        }
        SpawnNextWave(); //게임 시작 시 첫 소환
    }
    public void GoToNextStage()
    {
        currentStageIndex++;

        if (currentStageIndex < allStageDatas.Length)
        {
            currentStageData = allStageDatas[currentStageIndex];

            SpawnNextWave();
        }
    }
    public void AddGold(int amount)
    {
        totalGold += amount;
    }
    public void AddKillCount()
    {
        currentRewardCount++;

        if (currentRewardCount >= currentStageData.rewardGoalCount)
        {
            GiveReward();
            currentRewardCount = 0; //게이지 초기화
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
