using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour  // 메인 SO들어오면 변수들 애기진행 조율해서 집어 넣기
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI maxLv;
    [SerializeField] private TextMeshProUGUI currentLv;
    [SerializeField] private TextMeshProUGUI increase;
    [SerializeField] private TextMeshProUGUI upgradeCost;
    [SerializeField] private Button upgradeButton;

    [SerializeField] private UIHoldButton holdButton;

    private TempItemData data;
    private int multiplier = 1; // 기본 배수 x1
    public void SetItem(TempItemData data)
    {
        this.data = data;
        Refresh();
    }
    public void SetMultiplier(int multi)
    {
        multiplier = multi;
        Refresh();
    }
    private int GetTotalUpgradeCount() // 최대 레벨까지 남은 업그레이드 횟수 계산해서 반환하는 함수
    {
        int remnant = data.maxLevel - data.currentLevel;
        return Mathf.Min(multiplier, remnant);
    }
    private void Refresh()
    {
        itemIcon.sprite = data.icon;
        maxLv.text = $"<size=100%>{data.itemName}</size> " +
                     $"<size=60%>Max Lv.{data.maxLevel}</size>";
        currentLv.text = $"Lv.{data.currentLevel}";

        if (data.IsMaxLevel())
        {
            increase.text = $"{data.GetCurrentValue()}";
            upgradeCost.text = "MAX";
            upgradeButton.interactable = false;
        }
        else
        {
            int count = GetTotalUpgradeCount();
            int totalCost = data.GetTotalCostLevel(count);
            float nextValue = data.GetValueAfterLevel(count);

            increase.text = $"{data.GetCurrentValue()} -> {nextValue}";
            upgradeCost.text = totalCost.ToString();
            upgradeButton.interactable = true;
        }
    }

    public void OnClickUpgradeButton()
    {
        if (data.IsMaxLevel()) // 혹시 모를 방지
        {
            Debug.Log("최대 레벨입니다.");

            holdButton.StopHold();
            return;
        }

        int count = GetTotalUpgradeCount(); // 총 업그레이드 비용
        int totalCost = data.GetTotalCostLevel(count);

        if (DataManager.Instance.SpendGold(totalCost)) // 21억 이상이면 데이터매니저 포함해서 long으로 교체
        {
            data.currentLevel += count; //배수만큼 레벨업
            Refresh();
        }
        else
        {
            Debug.Log("골드가 부족합니다!");
            UIManager.Instance.ShowWarning("Gold !!!!!!!!!!!!!!!!!!!!"); // 한글이 깨져서 일단  영어로 작성했습니다. 나중에 수정.
            holdButton.StopHold();// 홀드 코루틴 제어
        }
    }



}
