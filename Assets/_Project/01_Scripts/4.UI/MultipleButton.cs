using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultipleButton : MonoBehaviour
{
    [Header("배수 버튼 설정")]
    [Tooltip("배수 버튼들 (현재, x1, x10)")]
    [SerializeField] private Button[] buttons;
    [SerializeField] private int[] multipliers = { 1, 1, 10 }; // { 현재, x1, x10 }

    [Header("현재 배수 표시용 텍스트")]
    [SerializeField] private TextMeshProUGUI currentMultiplierText;

    [Header("선택 색상, 기본 색상")]
    [SerializeField] private Color selectedColor = new Color(1.0f, 0.65f, 0.0f, 1.0f); // 슬레이어 느낌색상 선택
    [SerializeField] private Color normalColor = Color.white;

    public event System.Action<int> OnMultiplierChanged;

    private int currentMultiplier = 1; // 현재 배수

    void Start()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => OnMultipleButtonClick(index));
        }
        OnMultipleButtonClick(0); 
    }

    void OnMultipleButtonClick(int index)
    {
        currentMultiplier = multipliers[index];

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].image.color = (i == index) ? selectedColor : normalColor;
        }
        if (currentMultiplierText != null)
        {
            currentMultiplierText.text = $"x{currentMultiplier}";
        }
        OnMultiplierChanged?.Invoke(currentMultiplier);
    }

    public int GetMultiplier()
    {
        return currentMultiplier;
    }
}