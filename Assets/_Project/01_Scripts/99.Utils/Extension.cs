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

    public static string ToCurrencyString(this double number)
    {
        return number.ToString("N0");
    }
}
