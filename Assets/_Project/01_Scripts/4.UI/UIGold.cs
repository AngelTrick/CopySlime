using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIGold : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private float gold = 0;

    void Update()
    {
        gold += 100f * Time.deltaTime; // 임시

        goldText.text = $"Gold: {FormatNumber(gold)}"; // 나중에 다른걸로 바꿀생각해보기

    }

    public void AddGold() // 임시 버튼용
    {
        gold += 100000f;
    }
    private string FormatNumber(float number) //일단은 K, M~~ 으로 띄움 나중에 더 생각하기
    {
        string[] suffixes = { "", "K", "M", "B", "T" };
        int suffixIndex = 0;

        while (number >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            number /= 1000;
            suffixIndex++;
        }

        return $"{number:0.##}{suffixes[suffixIndex]}";
    }
}
