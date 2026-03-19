using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Temp/TempItemData")]

public class TempItemData : ScriptableObject  //임시 SO용 지우는거 생각하기 나중에 진짜 SO로 교체
{
    
    //public int currentValue;
    //public int nextValue;
    //public int upgradeCost;



    [Header("기본 정보")]
    public Sprite icon;
    public string itemName;
    public int maxLevel;
    public int currentLevel;

    [Header("스텟 설정")]
    public float baseValue; // 기본 스텟
    public float growthRate; // 레벨당 증가량

    [Header("비용 설정")]
    public int baseCost; // 기본 업그레이드 비용
    public float costGrowthRate; // 레벨당 비용 증가율 (예: 1.3f는 30% 증가)

    public float GetCurrentValue()
    {
        return baseValue + (growthRate * currentLevel);
    }

    public float GetNextValue()
    {
        return baseValue + (growthRate * (currentLevel + 1));
    }
    
    public int GetUpgradeCost() // 레벨이 높은 거에 적용
    {
        int fineIncrement = 15 + (currentLevel / 3); // 나중에 숫자를 조정할수있는 변수 생각하기
        if (currentLevel == 0)
        {
            return baseCost;
        }

        return baseCost + (16 * currentLevel) + fineIncrement;
    }

    public bool IsMaxLevel()
    {
        return currentLevel >= maxLevel;
    }



}
