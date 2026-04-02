using UnityEngine;
using TMPro;
using UnityEngine.UI;
/*
 * [오프라인 보상 팝업 UI]
 * 유저가 게임에 복귀했을 때 방치 시간과 획득한 골드 화려하게 보여주는 창입니다.
 */
public class UIOfflineReward : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private TextMeshProUGUI _timeText;         // 방치 시간 표시 텍스트
    [SerializeField] private TextMeshProUGUI _rewardText;       // 획득 골드 표시 텍스트
    [SerializeField] private Button _confirmButton;             // 확인(수령) 버튼

    // 팝업에 떠 있는 동안 지급 대기 중인 골드량을 기억할 변수
    private double _pendingGold = 0;

    private void Awake()
    {
        //확인 버튼을 누르면 팝업창 닫는 이벤트를 자동으로 연결합니다.
        if(_confirmButton != null)
        {
            _confirmButton.onClick.RemoveAllListeners();
            _confirmButton.onClick.AddListener(OnClickConfirmButton);
        }
        else
        {
            // 이 로그가 뜬다면 100% 인스펙터 문제
            Debug.LogError("[UIOfflineReward] 확인 버튼(_confirmButton)이 인스펙터에 연결되지 않았습니다.");
        }
    }
    /// <summary>
    /// Gamemanager에서 오프라인 보상 계산이 끝난 직후 이 함수를 UIManager.cs 에서 호출 받아 출력합니다.
    /// </summary>
    /// <param name="minutes">방치한 분(Minute)</param>
    /// <param name="gold">획득한 골드</param>
    public void ShowReward(int minutes, double gold)
    {
        // 1. 팝업창 켜기
        gameObject.SetActive(true);

        // 유저가 버튼을 누를 때 줄 수 있도록 골드을 저장해 둡니다.
        _pendingGold = gold;
        // 2. 시간 표기 (예: 130분 -> 2시간 10분 형태로 변환해서 예쁘게 표기)
        int hours = minutes / 60;
        int mins = minutes % 60;

        if(hours > 0)
        {
            _timeText.text = $"자동 사냥 시간 : <color=#FFD700>{hours}시간 {mins}분</color>";
        }
        else
        {
            _timeText.text = $"자동 사냥 시간 : <color=#FFD700>{mins}분</color>"; 
        }

        //3. 골드 표시
        _rewardText.text = $"획득 골드\n<size=150%><color=#FFD700>+{gold.ToSmartCurrency()}</color></size>";

        Debug.Log($"[UIOfflineReward] 팝업 오픈! 대기 중인 골드 : {_pendingGold}");
    }

    public void OnClickConfirmButton()
    {
        Debug.Log($"[UIOfflineReward] 확인 버튼 클릭됨! 처리할 골드 : {_pendingGold}");

        // 창을 닫을 때(수령 버튼을 눌렀을 때) 실제 재화 DataManger에 지급합니다.
        if(_pendingGold > 0 )
        {
            if(DataManager.Instance != null)
            {
                DataManager.Instance.AddGold(_pendingGold);
                Debug.Log($"[UIOfflineReward] 유저가 확인 버트튼 클릭! 보상 {_pendingGold} 골드 지급 완료");
            }
            else
            {
                Debug.LogError("[UIOfflineReward] DataManager가 존재 하지 않아 지급 실패");
            }
            // 중복 수령 방지를 위해 0으로 초기화
            _pendingGold = 0;
        }

        //팝업창 끄기
        gameObject.SetActive(false);
    }
}
