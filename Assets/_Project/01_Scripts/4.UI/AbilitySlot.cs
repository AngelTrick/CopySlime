using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySlot : MonoBehaviour, ISlot
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
    private int GetTotalUpgradeCount() // 최대 레벨까지 남은 업그레이드 횟수 계산해서 반환하는 함수
    {
        int remnant = data.maxLevel - data.currentLevel;
        return Mathf.Min(multiplier, remnant);
    }
    public void Refresh()
    {
        if (data == null) return;

        itemIcon.sprite = data.icon;
        maxLv.text = $"<size=100%>{data.itemName}</size> ";
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
            double totalCost = data.GetTotalCostLevel(count);
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
        double totalCost = data.GetTotalCostLevel(count);

        if (DataManager.Instance.SpendGold(totalCost)) // 21억 이상이면 데이터매니저 포함해서 long으로 교체
        {
            data.currentLevel += count; //배수만큼 레벨업
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
    public void ExpandCurrentLevel(int amount) // 확인용
    {
        data.currentLevel += amount;
        Refresh();
    }
    public void ResetCurrentLevel(int amount) // 확인용
    {
        data.currentLevel = amount;
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
        if (Input.GetKeyDown(KeyCode.J))
        {
            ExpandCurrentLevel(-1);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            ExpandCurrentLevel(30);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            ResetCurrentLevel(1);
        }
    }

}
