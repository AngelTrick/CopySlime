using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

/* 
 * [ 전역 씬 매니저]
 * 역할 : 씬 이동 , 비동기 로딩 (로딩창 뛰우기) , 씬 이동 전 메모리 정리
 */
public class SceneManagerEx : Singleton<SceneManagerEx>
{
    // 씬 컨트롤러 찾는 프로퍼티
    public BaseScene CurrentScene { get { return GameObject.FindObjectOfType<BaseScene>(); } }

    string GetSceneName(Define.Scene type)
    {
        switch (type)
        {
            case Define.Scene.Boot: return "00_Boot";
            case Define.Scene.Title: return "01_Title";
            case Define.Scene.MainGame: return "02_MainGame";
        }
        return System.Enum.GetName(typeof(Define.Scene), type);
    }

    public void LoadScene(Define.Scene type)
    {
        CurrentScene?.Clear();

        SceneManager.LoadScene(GetSceneName(type));
    }

    public void LoadSceneAsync(Define.Scene type)
    {
        CurrentScene?.Clear();
        StartCoroutine(LoadSceneRoutine(type));
    }

    private IEnumerator LoadSceneRoutine(Define.Scene type)
    {
        // 1. Resources 폴더에서 로딩 UI 프리팹 꺼내서 띄우기
        GameObject loadingPrefab = Resources.Load<GameObject>("UI/UILoading");
        GameObject loadingUI = null;

        if(loadingPrefab != null)
        {
            loadingUI = Instantiate(loadingPrefab);
            DontDestroyOnLoad(loadingUI); // 씬이 넘어가는 중에도 파괴되지 않게 보호!
        }

        string sceneName =  GetSceneName(type);
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        // 씬 100% 로드 되기 전까지 못 넘어 가도록 막기
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            if(op.progress >= 0.9f)
            {
                //로딩이 너무 빠르면 로딩창을 볼 새도 없이 넘어가므로 0.5초 딜레이
                yield return new WaitForSeconds(1.5f);
                op.allowSceneActivation = true;
            }
            yield return null;
        }
        // 2. 씬 이동이 완전히 끝나면 로딩창 파괴!
        if (loadingUI != null) Destroy(loadingUI); 
    }

}
