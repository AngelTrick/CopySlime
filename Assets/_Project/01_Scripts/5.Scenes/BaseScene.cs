using UnityEngine;
using UnityEngine.EventSystems;

/*
 * [지역 씬 컨트롤러 (부모 클래스)
 * 역할 : 씬 시작 할 때 무조건 실행 되어야 하는 초기 셋팅 
 */

public abstract class BaseScene : MonoBehaviour
{
    public Define.Scene SceneType { get;  set; } = Define.Scene.Unknown;

    protected virtual void Awake()
    {
        Init();
    }


    // 씬 마다 다르게 구현 될 초기화 세팅
    protected virtual void Init()
    {
        Object evt = GameObject.FindObjectOfType(typeof(EventSystem));
        if(evt == null)
        {
            GameObject go = new GameObject("@EnventSystem");
            go.GetOrAddComponent<EventSystem>();
            go.GetOrAddComponent<StandaloneInputModule>();
        }
    }
    
    // 다른 씬으로 넘어 갈 때 메모리를 비워 주는 함수
    public abstract void Clear();
}
