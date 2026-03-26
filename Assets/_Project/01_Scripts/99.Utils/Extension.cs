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
    // Ex) 재화가 1백만 이라면 1,000,000 처럼 3자리마다 쉼표로 끝어 준다.
    public static string ToCurrencyString (this long number)
    {
        // "N0" (Number with 0 decimal places) 포맷 사용
        // C# 에서 3자리마다 짤라서 보여 준다.
        return number.ToString("N0");
    }

    // [핵심 3 : 방치형 전용 단위 변환기
    // 팀원 누구나 실수 (float/double) 뒤에 .ToIdleCurrencyString()를 붙이면 알파벳 단위로 변환됩니다.
    public static string ToIdleCurrencyString(this float number)
    {
        // 1만 미만은 알파벳 없이 정확한 숫자와 콤마로 표기 (예: 9,999)
        if(number < 10000)
        {
            return number.ToString("N0");
        }

        //단위 배열 (필요시 aa, ab, ac 등 무한 확장 가능)
        string[] suffixes = { "", "K", "M", "B", "T", "aa", "ab", "ac" };
        int suffixIndex = 0;

        while (number >= 1000f && suffixIndex < suffixes.Length - 1)
        {
            number /= 1000f;
            suffixIndex++;
        }

        // 소수점 둘째 자리까지 표시 (예: 1.25k, 150.30M)
        return $"{number:F2}{suffixes}{suffixIndex}";
    }
    public static string ToIdleCurrencyString(this double number)
    {
        // 1만 미만은 알파벳 없이 정확한 숫자와 콤마로 표기 (예: 9,999)
        if (number < 10000)
        {
            return number.ToString("N0");
        }

        //단위 배열 (필요시 aa, ab, ac 등 무한 확장 가능)
        string[] suffixes = { "", "K", "M", "B", "T", "aa", "ab", "ac" };
        int suffixIndex = 0;

        while (number >= 1000f && suffixIndex < suffixes.Length - 1)
        {
            number /= 1000f;
            suffixIndex++;
        }

        // 소수점 둘째 자리까지 표시 (예: 1.25k, 150.30M)
        return $"{number:F2}{suffixes}{suffixIndex}";
    }
}
