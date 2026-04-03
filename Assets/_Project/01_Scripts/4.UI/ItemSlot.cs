using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, ISlot  // 메인 SO들어오면 변수들 애기진행 조율해서 집어 넣기
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI maxLv;
    [SerializeField] private TextMeshProUGUI currentLv;
    [SerializeField] private TextMeshProUGUI increase;
    [SerializeField] private TextMeshProUGUI upgradeCost;
    [SerializeField] private Button upgradeButton;

    [SerializeField] private UIHoldButton holdButton;

    private TempItemData data;
    private int level;
    private int multiplier = 1; // 기본 배수 x1

    private int GetCurrentLevelDataManager()
    {
        if (DataManager.Instance == null) return 1;

        switch (data.itemName)
        {
            case "공격력": return DataManager.Instance.AttackLevel;
            case "치명타 데미지": return DataManager.Instance.CritDamageLevel;
            case "치명타 확률": return DataManager.Instance.CritRateLevel;
            case "운": return DataManager.Instance.LuckLevel;
            case "공격 속도": return DataManager.Instance.AttackSpeedLevel;
            default: return 1;
        }
    }

    public void SetItem(TempItemData data)
    {
        this.data = data;
        if (holdButton != null)
        {
            holdButton.Init(OnClickUpgradeButton);
        }
        Refresh();
    }
    public void SetMultiplier(int multi)
    {
        multiplier = multi;
        Refresh();
    }
    private int GetTotalUpgradeCount(int currentLevel) // 최대 레벨까지 남은 업그레이드 횟수 계산해서 반환하는 함수
    {
        //level = GetCurrentLevelDataManager();
        int remnant = data.maxLevel - currentLevel;
        return Mathf.Min(multiplier, remnant);
    }
    public void Refresh()
    {
        if (data == null) return;

        level = GetCurrentLevelDataManager();

        itemIcon.sprite = data.icon;
        maxLv.text = $"<size=100%>{data.itemName}</size> " +
                     $"<size=60%>Max Lv.{data.maxLevel}</size>";
        currentLv.text = $"Lv.{level}";

        if (data.IsMaxLevel(level))
        {
            increase.text = $"{data.GetCurrentValue(level)}";
            upgradeCost.text = "MAX";
            upgradeButton.interactable = false;
        }
        else
        {
            int count = GetTotalUpgradeCount(level);
            double totalCost = data.GetTotalCostLevel(level, count);
            float nextValue = data.GetValueAfterLevel(level, count);

            increase.text = $"{data.GetCurrentValue(level)} -> {nextValue}";
            upgradeCost.text = totalCost.ToString();
            upgradeButton.interactable = true;
        }
    }

    public void OnClickUpgradeButton()
    {
        level = GetCurrentLevelDataManager();

        if (data.IsMaxLevel(level)) // 혹시 모를 방지
        {
            Debug.Log("최대 레벨입니다.");

            holdButton.StopHold();
            return;
        }

        int count = GetTotalUpgradeCount(level); // 총 업그레이드 비용
        double totalCost = data.GetTotalCostLevel(level, count);

        if (DataManager.Instance.SpendGold(totalCost)) // 21억 이상이면 데이터매니저 포함해서 long으로 교체
        {
            DataManager.Instance.UpgradeStatLevel(data.itemName, count);
            Refresh();
        }
        else
        {
            Debug.Log("골드가 부족합니다!");
            UIManager.Instance.ShowWarning("Gold !!!!!!!!!!!!!!!!!!!!"); // 한글이 깨져서 일단  영어로 작성했습니다. 나중에 수정.
            holdButton.StopHold();// 홀드 코루틴 제어
        }
    }
    public void ExpandMaxLevel(int amount) // 확인용
    {
        data.maxLevel += amount;
        Refresh();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ExpandMaxLevel(1);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            ExpandMaxLevel(-1);
        }
    }


}
