using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTreasureData", menuName = "Stage/Treasure Data")]
public class TreasureData : ScriptableObject
{
    public string treasureName;
    public GameObject chestPrefab; //상자 프리펩

    [Header("전투 및 크기 설정")]
    public double maxHp = 50f; //상자 체력
    public float chestScale = 2f; //상자 크기

    public int goldCount = 10; //터지는 동전 개수
}