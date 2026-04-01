using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Temp/TempItemData")]

public class TempItemData : ScriptableObject  //임시 SO용 지우는거 생각하기 나중에 진짜 SO로 교체
{

    //public int currentValue;
    //public int nextValue;
    //public int upgradeCost;
    public enum FormulaType
    {
        Linear,       // base + (lv - 1) * growthRate
        Exponential   // base + (lv * multiplier) * pow(growthRate, lv - 1)  ← 공격력용
    }


    [Header("기본 정보")]
    public Sprite icon;
    public string itemName;
    public int maxLevel;
    public int currentLevel = 1;

    [Header("타입 설정 공격력은 Exponential 선택 나머지는 Linear")]
    public FormulaType formulaType;  // ← 인스펙터에서 선택

    [Header("스텟 설정")]
    public float baseValue; // 기본 스텟
    public float growthRate; // 레벨당 증가율
    public float expMultiplier = 5f; // Exponential (공격력용) 나중에 설정하는 변수 생각하기

    [Header("비용 설정")]
    public int baseCost; // 기본 업그레이드 비용

    private float CalcValue(int level)
    {
        switch (formulaType)
        {
            case FormulaType.Exponential:
                // 공격력 : base + (lv * multiplier) * pow(growthRate, lv - 1)
                return baseValue + (level * expMultiplier) * Mathf.Pow(growthRate, level - 1);

            case FormulaType.Linear:
            default:
                // 선형(나머지) : base + (lv - 1) * growthRate
                return baseValue + ((level - 1) * growthRate);
        }
    }
    public float GetCurrentValue()
    {
        //return baseValue + (growthRate * currentLevel);
        return CalcValue(currentLevel);
    }

    public float GetNextValue()
    {
        //return baseValue + (growthRate * (currentLevel + 1));
        return CalcValue(currentLevel + 1);
    }
    public float GetValueAfterLevel(int count) // n레벨 후 밸류
    {
        //return baseValue + (growthRate * (currentLevel + count));
        return CalcValue(currentLevel + count);
    }

    public double GetUpgradeCostLevel(int level) // 특정 레벨에서의 비용 (n배수 계산용)
    {
        int fineIncrement = 15 + (level / 3);  // 나중에 숫자를 설정하는 변수 생각하기
        if (level == 0)
        {
            return baseCost;
        }
        return baseCost + (16 * level) + fineIncrement;
    }
    // n회 업그레이드 총 비용
    public double GetTotalCostLevel(int count) // 나중에 숫자(21억↑)가 커지면 long으로 대체 데이터매니저도 포함해서
    {
        double total = 0;
        for (int i = 0; i < count; i++)
        {
            total += GetUpgradeCostLevel(currentLevel + i);
        }
        return total;
    }
    public bool IsMaxLevel()
    {
        return currentLevel >= maxLevel;
    }



}
