using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _applicationIsQuitting = false;  // 앱이 종료 되는 중인지 체크

    public static T Instance
    {
        get
        {
            // 1. 인스턴스가 아직 없을 때만 생성을 시도합니다.
            if(_instance == null)
            {
                // 2. [핵심 방어막] 앱이 꺼지는 중이라면 , 억지로 새로 만들지 말고 null로 반환
                if (_applicationIsQuitting)
                {
                    return null;
                }
                
                _instance = FindObjectOfType<T>();
                if(_instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name);
                   _instance = obj.AddComponent<T>();
                }
            }    
            // 4. 이미 인스턴스가 잘 살아 있다면 (앱 종료 중이든 아니든) 정상적으로 반환됩니다.
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
