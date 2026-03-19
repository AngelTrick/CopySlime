using System.Collections;
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
        // TODO : 이 부분에 "로딩 중 ...." UI 패널 키기

        string sceneName =  GetSceneName(type);
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        // 씬 100% 로드 되기 전까지 못 넘어 가도록 막기
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            if(op.progress >= 0.9f)
            {
                op.allowSceneActivation = true;
            }
        }
        yield return op;
    }

    // TODO : "로딩 중 ... " UI 패널 끄기
}
