using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;
    public Stage stageController; 
    public StageData currentStageData;
    private int currentRewardCount = 0;

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
        SpawnNextWave(); //게임 시작 시 첫 소환
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
       //인벤토리나 골드 시스템에 반영
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
