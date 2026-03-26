using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SlotGroup
{
    public string label;
    public GameObject panelObject;
    public Transform content;
    public TTItembase database;
    public GameObject slotPrefab;
    public MultipleButton multipleButton;
}
public class UIItemManager : Singleton<UIItemManager> // 나중에 연결한거 없다면 UISlotManager로 이름 바꾸기
{
    [Header("슬롯 설정 (능력치, 아이템, 스킬 순서로 등록)")]
    [SerializeField] private List<SlotGroup> slotGroups = new List<SlotGroup>();

    //[SerializeField] private MultipleButton multipleButton;
    private List<ISlot> allSlots = new List<ISlot>();
    public List<SlotGroup> GetSlotGroups() => slotGroups;
    private void Start()
    {
        InitAllSlots();
        //SetupMultipleButton();
    }
    private void InitAllSlots()
    {
        foreach (var group in slotGroups)
        {
            if (!isSlotCheck(group)) continue;
            List<ISlot> currentGroupSlots = new List<ISlot>();

            foreach (TempItemData data in group.database.GetAll())
            {
                GameObject slotGO = Instantiate(group.slotPrefab, group.content); // 슬롯 생성
                var slot = slotGO.GetComponent<ISlot>();
                if (slot != null)
                {
                    slot.SetItem(data);
                    currentGroupSlots.Add(slot);
                    allSlots.Add(slot);
                }
            }
            if (group.multipleButton != null)
            {
                MultiplierHelper helper = new MultiplierHelper(currentGroupSlots);

                group.multipleButton.OnMultiplierChanged += helper.UpdateMultiplier;
                helper.UpdateMultiplier(group.multipleButton.GetMultiplier());
            }
            /*
            if (group.multipleButton != null)
            {
                group.multipleButton.OnMultiplierChanged += (multi) => // 람다식 말고 나중에 변경해보기
                {
                    foreach (var slot in currentGroupSlots) slot.SetMultiplier(multi);
                };
                // 초기 배수 적용
                foreach (var slot in currentGroupSlots)
                {
                    slot.SetMultiplier(group.multipleButton.GetMultiplier());
                }
            }
            */
            Debug.Log($"[{group.label}] 슬롯 생성 완료"); // 확인용
        }
    }
    private bool isSlotCheck(SlotGroup group)
    {
        if (group.content == null || group.database == null || group.slotPrefab == null)
        {
            Debug.LogWarning($"[{group.label}] 누락됨 인스펙터확인");
            return false;
        }
        return true;
    }
    /*
      private void SetupMultipleButton() // 나중에 지우거나 사용하기
      {
          if (multipleButton == null) return;

          ApplyMultiplier(multipleButton.GetMultiplier());
          multipleButton.OnMultiplierChanged += ApplyMultiplier;
      }
      private void ApplyMultiplier(int multi)
      {
          foreach (var slot in allSlots)
          {
              if (slot != null) slot.SetMultiplier(multi);
          }
    }
    */
    public class MultiplierHelper
    {
        private List<ISlot> _targetSlots;

        public MultiplierHelper(List<ISlot> targetSlots)
        {
            _targetSlots = targetSlots;
        }

        public void UpdateMultiplier(int multi)
        {
            foreach (var slot in _targetSlots)
            {
                if (slot != null) slot.SetMultiplier(multi);
            }
        }
    }
    public GameObject GetCurrentGroupButton(GameObject currentPanel) // 현재 어느 패널인지 받아서 자식
    {
        foreach (var group in slotGroups)
        {
            /*
            if (group.content.parent.gameObject == currentPanel || group.content.gameObject == currentPanel)
            {
                return group.multipleButton != null ? group.multipleButton.gameObject : null;
            }
            */
            if (group.panelObject == currentPanel) // 이름이나 부모가 아닌 객체 자체를 비교
            {
                return group.multipleButton?.gameObject;
            }
        }
        return null;
    }
}
