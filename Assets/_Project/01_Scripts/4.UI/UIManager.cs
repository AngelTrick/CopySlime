using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager> // 설정도 여기에 추가 생각
{
    [Header("전체적인 메인 판넬")]
    [SerializeField] private GameObject panelSkill;
    [SerializeField] private GameObject panelAbility;
    [SerializeField] private GameObject panelDungeon;
    [SerializeField] private GameObject panelItem;
    [SerializeField] private GameObject activeSkill;
    [SerializeField] private GameObject soloActiveSkillBar; // 다른창없이 혼자 스킬창 띄울 때

    [Header("셋팅판넬")]
    [SerializeField] private GameObject settingPanel;
    [Header("배수버튼 판넬")]
    [SerializeField] private GameObject multiplePanel;

    [SerializeField] private TextMeshProUGUI warningText;

    private Coroutine currentCO;

    private GameObject currentPanel = null; // 현재 열린 패널 추적용


    [Header("임시")]
    [SerializeField] private GameObject statsPanel;

    public void ShowWarning(string message)
    {

        if (currentCO != null) // 이미 실행 중이면 중단하고 다시 시작
            StopCoroutine(currentCO);

        currentCO = StartCoroutine(ShowAndFade(message));
    }
    private IEnumerator ShowAndFade(string message)
    {
        // 텍스트 설정 & 완전히 보이게
        warningText.text = message;
        warningText.color = new Color(1f, 0.3f, 0.3f, 1f);

        yield return new WaitForSeconds(1f); // n초 대기

        // 서서히 사라지기
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            warningText.color = new Color(1f, 0.3f, 0.3f, alpha);
            yield return null;
        }

        warningText.color = new Color(1f, 0.3f, 0.3f, 0f);
    }

    public void OpenSoloSkillBar()
    {
        soloActiveSkillBar.SetActive(true);
    }
    /*
    public void OpenActiveSkill()
    {
        CloseAll();
        activeSkill.SetActive(true);
    }
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
    */
    public void OpenSkill() { TogglePanel(panelSkill); }
    public void OpenAbility() { TogglePanel(panelAbility); }
    public void OpenDungeon() { TogglePanel(panelDungeon); }
    public void OpenItem() { TogglePanel(panelItem); }

    void TogglePanel(GameObject targetPanel)
    {
        bool isSave = currentPanel == targetPanel;
        CloseAll();
        if (isSave) // 같은 버튼 다시 누르면 닫기
        {
            OpenSoloSkillBar();
            currentPanel = null; // 초기화
            return;
        }

        // 같지않으면 스킬바 닫고 그대로 열기
        activeSkill.SetActive(true);
        targetPanel.SetActive(true);

        currentPanel = targetPanel;
    }
    void CloseAll() // 전에 켜진 패널 닫기 용 함수
    {
        panelSkill.SetActive(false);
        panelAbility.SetActive(false);
        panelDungeon.SetActive(false);
        panelItem.SetActive(false);
        activeSkill.SetActive(false);
        soloActiveSkillBar.SetActive(false);

    }
    public void OpenSetting()
    {
        settingPanel.SetActive(true);
    }

    public void CloseSetting()
    {
        settingPanel.SetActive(false);
    }
    public void ToggleMultiple()
    {
        multiplePanel.SetActive(!multiplePanel.activeSelf);
    }
    public void ToggleStats()
    {
        statsPanel.SetActive(!statsPanel.activeSelf);
    }

}
