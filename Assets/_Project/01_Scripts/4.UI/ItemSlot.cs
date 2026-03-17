using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour  // 메인 SO들어오면 변수들 애기진행 조율해서 집어 넣기
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI maxLv;
    [SerializeField] private TextMeshProUGUI currentLv;
    [SerializeField] private TextMeshProUGUI increase;
    [SerializeField] private TextMeshProUGUI upgradeCost;
    [SerializeField] private Button upgradeButton;

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
        increase.text = $"{data.currentValue} -> {data.nextValue}";
        upgradeCost.text = data.upgradeCost.ToString();
    }
}
