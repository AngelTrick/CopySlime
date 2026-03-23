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
        // [수정된 부분] 보스 버튼은 언제든 누를 수 있어야 하므로 상시 활성화!
        if (bossButton != null)
        {
            bossButton.interactable = true;

            // 코드로 안전하게 버튼 클릭 이벤트 연결
            // 인스펙터에서 일일이 연결 안 해도 알아서 아래 함수가 실행됩니다!
            bossButton.onClick.AddListener(OnClickBossButton);
        }
    }
    public void OnClickBossButton()
    {
        // StageManager에게 보스 소환 명령 내리기
        if (StageManager.Instance != null)
        {
            StageManager.Instance.ChallengeBoss();
            Debug.Log("[UIStage] 보스 소환 명령 전달 완료! 보스전이 시작됩니다.");

            // (참고) 사운드 매니저 연동 시 아래 주석 해제하시면 됩니다!
            // SoundManager.Instance.PlaySFX(클릭사운드);
        }
    }
    public void OnMonsterKilled()
    {
        if (isWait)
        {
            Debug.Log("카운트가 다참"); // 반복보상으로 교체
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
    }

    public void UpdateFillBar()
    {
        float targetFill = (float)currentCount / stageCount;

        stageBar.fillAmount = targetFill;

    }
    public void OnBarCompleted()
    {
        isWait = true;
        Debug.Log("100%"); // 게이지 반복 보상용으로 사용
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnMonsterKilled();
        }
    }

}
