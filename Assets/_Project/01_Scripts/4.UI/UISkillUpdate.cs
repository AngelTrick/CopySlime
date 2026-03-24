using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISkillUpdate : MonoBehaviour
{
    private List<UISkill> skillList = new List<UISkill>();
    public void RegisterSkill(UISkill skill)
    {
        if (!skillList.Contains(skill)) skillList.Add(skill);
    }
    void Update()
    {
        foreach (UISkill skill in skillList)
        {
            skill.ManualUpdate();
        }
    }
}
