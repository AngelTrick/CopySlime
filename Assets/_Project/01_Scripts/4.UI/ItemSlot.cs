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
    public void SetItem(TempItemData data)
    {
        this.data = data;
        Refresh();
    }

    private void Refresh()
    {
        itemIcon.sprite = data.icon;
        maxLv.text = $"<size=100%>{data.itemName}</size> " +
                     $"<size=60%>Max Lv.{data.maxLevel}</size>";
        currentLv.text = $"Lv.{data.currentLevel}";
        increase.text = $"{data.GetCurrentValue()} -> {data.GetNextValue()}";
        upgradeCost.text = data.IsMaxLevel() ? "MAX" : data.GetUpgradeCost().ToString();
        upgradeButton.interactable = !data.IsMaxLevel(); // 최대 레벨이면 버튼 비활성화 or MAX표시 이미지로 교체
    }

    public void OnClickUpgradeButton()
    {
        if (data.IsMaxLevel()) // 혹시 모를 방지
        {
            Debug.Log("최대 레벨입니다.");

            holdButton.StopHold();
            return;
        }

        int cost = data.GetUpgradeCost(); ; // 업그레이드 비용

        // SpendGold() 가 알아서 돈이 부족한지 검사하고, 충분하면 깎아준 뒤 true를 반환합니다!
        if (DataManager.Instance.SpendGold(cost))
        {
            Debug.Log("결제 성공! 스탯을 올려주세요.");

            // TODO: Player.AttackPower 등 실제 스탯 상승 로직 실행
            data.currentLevel++;
            Refresh(); // 현재 슬롯 텍스트 갱신
        }
        else
        {
            Debug.Log("골드가 부족합니다!");
            UIManager.Instance.ShowWarning("Gold !!!!!!!!!!!!!!!!!!!!"); // 한글이 깨져서 일단  영어로 작성했습니다. 나중에 수정.
            holdButton.StopHold();// 홀드 코루틴 제어

        }
    }



}
