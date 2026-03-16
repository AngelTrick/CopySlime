using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStageData", menuName = "ScriptableObjects/StageData")]
public class StageData : ScriptableObject
{
    [Header("스테이지 정보")]
    public string stageName; //스테이지 이름
    public int stageLevel; //스테이지 번호

    [Header("난이도 및 보상 배율")]
    public float statsMultiplier = 1.0f; //몬스터 체력/공격력 배율 (1.0 = 100%)
    public float rewardMultiplier = 1.0f; //골드 보상 비율

    [Header("반복 보상 설정")]
    public int rewardGoalCount = 20; //20마리 잡으면 보상 지급
    public int baseRewardGold = 100; //기본 골드 양

    [Header("등장 몬스터")]
    public NormalMonsterData[] fieldMonsters; //필드 무한 루프용 몬스터들
    public BossMonsterData stageBoss; //버튼 눌렀을 때 나올 보스
}
