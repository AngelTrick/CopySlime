using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMonsterData : ScriptableObject
{
    [Header("공통 정보")]
    public string monsterName; //몬스터이름
    public GameObject modelPrefab; //몬스터프리펩

    [Header("공통 전투 데이터")]
    public float maxHp; //최대체력

    [Header("공통 보상")]
    public int dropGold; //보상골드
}
