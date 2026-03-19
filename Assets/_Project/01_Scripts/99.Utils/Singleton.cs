using UnityEditor;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _applicationIsQuitting = false;  // 앱이 종료 되는 중인지 체크

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                return null;
            }
            if(_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if(_instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name);
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(transform.root.gameObject);
            // 자식으로 있다면 최상단 부모 통째로 살려내고
            // 부모가 없다면 자기자신 정상 작동
            // 한 이유 : 하이어레키 창에서 관리 법으로 부모 (Managers) 자식 (Game,Data,Pool , etc..) 가져 가기 ?문에
        }
        else Destroy(gameObject);
    }

    private void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }

    private void OnDestroy()
    {
        _applicationIsQuitting = true;
    }
}
