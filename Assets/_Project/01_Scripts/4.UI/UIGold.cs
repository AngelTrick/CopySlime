using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIGold : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;

    
    public void AddGold() // 임시 버튼용
    {
        DataManager.Instance.AddGold(100000);
    }

    private void Start()
    {
        // 1. 데이터가 변경될 때마다 UpdateGoldUI를 실행하도록 등록
        if (DataManager.Instance != null)
        {
            DataManager.Instance.OnDataChanged += UpdateGoldUI;
        }

        // 2. 게임 시작 시 현재 보유한 골드를 처음으로 표시
        UpdateGoldUI();
    }

    private void OnDestroy()
    {
        // 3. 메모리 누수 방지를 위해 오브젝트 파괴 시 이벤트 구독 해제
        if (DataManager.Instance != null)
        {
            DataManager.Instance.OnDataChanged -= UpdateGoldUI;
        }
    }

    // 4. Update() 대신 이벤트가 발생할 때만 호출되는 함수
    private void UpdateGoldUI()
    {
        if (DataManager.Instance == null) return;

        // DataManager의 실제 골드 데이터를 가져와서 포맷팅
        //goldText.text = $"Gold: {FormatNumber(DataManager.Instance.Gold)}";
        goldText.text = $"Gold: {DataManager.Instance.Gold.ToSmartCurrency()}";
    }
    /*
    private string FormatNumber(float number)
    {
        string[] suffixes = { "", "K", "M", "B", "T" };
        int suffixIndex = 0;

        while (number >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            number /= 1000;
            suffixIndex++;
        }

        // 소수점 둘째 자리까지 표시 (예: 1.25K)
        return $"{number:F2}{suffixes[suffixIndex]}";
    }
    */
}
