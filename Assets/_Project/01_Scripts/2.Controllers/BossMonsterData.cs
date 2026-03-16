using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBossData", menuName = "Monster/Boss")]
public class BossMonsterData : BaseMonsterData
{
    [Header("보스전 설정")]
    public float bossTimeLimit = 30f; //보스 도전 시간

    [Header("보스 처치 보상")]
    public Sprite unlockSkin; //해금스킨
}