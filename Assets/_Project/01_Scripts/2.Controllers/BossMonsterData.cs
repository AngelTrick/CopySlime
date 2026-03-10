using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBossData", menuName = "Monster/Boss")]
public class BossMonsterData : BaseMonsterData
{
    [Header("보스 전용 능력치")]
    public float attackPower; //공격력
    public float attackRange; //사거리

    [Header("보스 처치 보상")]
    public Sprite unlockSkin; //해금스킨
}