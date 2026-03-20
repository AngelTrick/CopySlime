using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemManager : Singleton<UIItemManager>
{
    [SerializeField] private Transform content;
    [SerializeField] private TTItembase database;
    [SerializeField] private ItemSlot itemSlotPrefab;
    [SerializeField] private MultipleButton multipleButton;


    //[SerializeField] private int itemCount = 20; // 더 좋은 방법을 생각

    private List<ItemSlot> itemSlots = new List<ItemSlot>(); //슬롯 목록 저장용

    private void Start()
    {
        if (database == null) { Debug.LogError("Database null!!!!!!!!!"); return; }
        if (itemSlotPrefab == null) { Debug.LogError("Prefab null!!!!!!!!!"); return; }
        if (content == null) { Debug.LogError("Content null!!!!!!!!!"); return; }

        List<TempItemData> list = database.GetAll();
        if (list == null || list.Count == 0)
        {
            Debug.LogError("Database 리스트 비어있음!!!!!!!!!");
            return;
        }

        Debug.Log($"아이템 개수: {list.Count}");

        foreach (TempItemData data in list)
        {
            ItemSlot slot = Instantiate(itemSlotPrefab, content);
            slot.SetItem(data);
            itemSlots.Add(slot);
        }
        if (multipleButton != null)
        {
            multipleButton.OnMultiplierChanged += (multi) =>
            {
                foreach (ItemSlot slot in itemSlots)
                {
                    slot.SetMultiplier(multi);
                }
            };
        }
    }

}
