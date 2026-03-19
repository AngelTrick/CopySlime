using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIStage : MonoBehaviour
{
    public Image stageBar; // 스테이지바 이미지 연결
    public int stageCount = 10;
    public int currentCount = 0;
    public bool isWait = false;
    public Button bossButton; // 보스스테이지버튼이랑 연결

    void Start()
    {
        stageBar.fillAmount = 0f;
        UpdateButtonState();
    }
    public void OnMonsterKilled()
    {
        if (isWait)
        {
            Debug.Log("카운트가 다참");
            return;
        }
        currentCount++;
        currentCount = Mathf.Clamp(currentCount, 0, stageCount);

        UpdateFillBar();

        if (currentCount >= stageCount)
        {
            OnBarCompleted();
        }
    }
    
    public void ResetBar()
    {
        currentCount = 0;
        isWait = false;
        stageBar.fillAmount = 0f;
        UpdateButtonState();
    }

    public void UpdateFillBar()
    {
        float targetFill = (float)currentCount / stageCount;

        stageBar.fillAmount = targetFill;

    }
    public void OnBarCompleted()
    {
        isWait = true;
        UpdateButtonState();
        Debug.Log("특정 몬스터"); // 나중에 뭘넣는지 얘기 보스활성화 or 다른거
    }
    void UpdateButtonState()
    {
        //bossButton.gameObject.SetActive(isWait); // 비활성화
        bossButton.interactable = isWait; // 반투명
    }
    /* //특정몬스터를 소환시키고 버튼은 그냥 활성화 일 때 or 다른거 구상일때
    public void OnBarCompleted()
    {
        isWait = true;
        SpecialMonster();
        Debug.Log("특정 몬스터"); // 나중에 뭘넣는지 얘기 보스활성화 or 다른거
    }
    public void SpecialMonster()
    {
        StartCoroutine(FillBar());
    }
    IEnumerator FillBar()
    {
        yield return new WaitForSeconds(2f);
        ResetBar();
    }
    public void ResetBar()
    {
        isWait = false;
        currentCount = 0;
        stageBar.fillAmount = 0f;
    }
    */


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnMonsterKilled();
        }
    }
}
