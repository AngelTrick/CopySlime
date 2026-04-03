using System;
using UnityEngine;

public static class Extension
{
    // [핵심 1 GetOrAddComponennt]
    // " 이 컴포넌트가 있으면 가져오고, 없으면 내가 붙여서 가져오기"
    // if(GetComponent == null) AddComponent 하는 수고 덜기 위함
    public static T GetOrAddComponent<T> (this GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }

    // [ 핵심 2 영수증 스타일 화폐 단위 변환기 (Currency Formatter)
    // Ex) 재화가 1백만 이라면 1,000,000 처럼 3자리마다 쉼표로 끝어 준다

    //==========================================================
    // [과도기 유지 코드] 기존에 팀원들이 쓰던 함수들 (빨간 줄 방지용) 
    //==========================================================
    [Obsolete("이 함수는 구버전입니다. 앞으로 ToSmartCurrency()를 사용해 주세요")]
    public static string ToCurrencyString (this long number)
    {
        // "N0" (Number with 0 decimal places) 포맷 사용
        // C# 에서 3자리마다 짤라서 보여 준다.
        return number.ToString("N0");
    }

    // [핵심 3 : 방치형 전용 단위 변환기
    // 팀원 누구나 실수 (float/double) 뒤에 .ToIdleCurrencyString()를 붙이면 알파벳 단위로 변환됩니다.
    [Obsolete("이 함수는 구버전입니다. 앞으로 ToSmartCurrency()를 사용해 주세요")]
    public static string ToIdleCurrencyString(this float number)
    {
        return ((double)number).ToSmartCurrency();
    }
    [Obsolete("이 함수는 구버전입니다. 앞으로 ToSmartCurrency()를 사용해 주세요")]
    public static string ToIdleCurrencyString(this double number)
    {
        return number.ToSmartCurrency();
    }

    //==========================================================
    // [신규 핵심 코드] 스마트 변환기 시스템 (옵션 기능 대비) 
    //==========================================================

    // 영수증 방식 ( 예: 1,000,000)
    public static string ToCommaFormat(this double number)
    {
        return number.ToString("N0");
    }

   // 알파벳 방식 ( 예: 1.50M)
   public static string ToAlphabetFormat(this double number)
    {
        if (number < 10000) return number.ToString("N0");

        string[] suffixes = { "", "K", "M", "B", "T", "aa", "ab", "ac" };
        int suffixIndex = 0;

        while (number >= 1000d && suffixIndex < suffixes.Length - 1)
        {
            number /= 1000d;
            suffixIndex++;
        }

        return $"{number:F2}{suffixes[suffixIndex]}";
    }

    public static string ToSmartCurrency(this double number)
    {
        // 1 이면 알파멧 모드, 0 이면 영수증 모드 (기본값 1)
        bool useCommaMode = PlayerPrefs.GetInt("Option_CommaCurrency", 1) == 1;

        if (useCommaMode)
        {
            return number.ToCommaFormat();
        }
        else
        {
            return number.ToAlphabetFormat();
        }
    }

    public static string ToSmartCurrency(this float number) => ToSmartCurrency((double)number);
}
