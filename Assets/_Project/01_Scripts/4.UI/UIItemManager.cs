using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemManager : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private ItemSlot itemSlotPrefab;
    [SerializeField] private int itemCount = 20; // 더 좋은 방법은 생각

    void Start()
    {
        CreateItems();
    }

    void CreateItems()
    {
    }

}
