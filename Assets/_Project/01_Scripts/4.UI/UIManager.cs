using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager> // 설정도 여기에 추가 생각
{
    public GameObject panelSkill;
    public GameObject panelAbility;
    public GameObject panelDungeon;
    public GameObject panelItem;

    public TextMeshProUGUI warningText;
    private Coroutine currentCO;

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
