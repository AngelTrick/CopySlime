using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager> // 설정도 여기에 추가 생각
{
    public GameObject panelSkill;
    public GameObject panelAbility;
    public GameObject panelDungeon;
    public GameObject panelItem;
    public void OpenSkill()
    {
        CloseAll();
        panelSkill.SetActive(true);
    }

    public void OpenAbility()
    {
        CloseAll();
        panelAbility.SetActive(true);
    }

    public void OpenDungeon()
    {
        CloseAll();
        panelDungeon.SetActive(true);
    }

    public void OpenItem()
    {
        CloseAll();
        panelItem.SetActive(true);
    }
    void CloseAll() // 전에 켜진 패널 닫기 용 함수
    {
        panelSkill.SetActive(false);
        panelAbility.SetActive(false);
        panelDungeon.SetActive(false);
        panelItem.SetActive(false);
    }

}
