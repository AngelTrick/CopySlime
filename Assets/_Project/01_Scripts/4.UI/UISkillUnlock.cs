using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISkillUnlock : MonoBehaviour 
{
    public List<UISkill> skills; // 나중에 프리팹화해서 자동추가까지 생각하기
    public int currentLevel = 1;
    void Start()
    {
        UpdateSlots();
    }

    public void OnLevelUp(int newLevel)
    {
        currentLevel = newLevel;
        UpdateSlots();
    }

    void UpdateSlots()
    {
        foreach (UISkill slot in skills)
        {
            bool unlocked = currentLevel >= slot.unlockLevel;
            slot.SetLock(!unlocked);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) // 확인 용
        {
            OnLevelUp(currentLevel + 1);
            Debug.Log($"현재 레벨 : {currentLevel}");
        }
    }


}
