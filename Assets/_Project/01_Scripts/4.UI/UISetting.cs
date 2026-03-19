using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISetting : MonoBehaviour
{
    public GameObject settingPanel;
    public void OpenSkillPanel()
    {
        settingPanel.SetActive(true);
    }

    public void CloseSkillPanel()
    {
        settingPanel.SetActive(false);
    }
}
