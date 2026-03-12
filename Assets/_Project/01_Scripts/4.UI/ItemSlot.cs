using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemValue;
    [SerializeField] private TextMeshProUGUI itemCost;
    [SerializeField] private Button upgradeButton;

    public void SetItem(UpgradeData data)
    {
        itemIcon.sprite = data.icon;
        itemName.text = data.name;
        itemValue.text = data.value.ToString();
        itemCost.text = data.cost.ToString();
    }
}
