using UnityEditor;
using UnityEngine;

/*[개발자 전용 치트 메뉴]
 * 용도 : 팀원이 게임 테스트 할 때 쓸 메뉴
 * 코드 수정하지 않고 게임 화면에서 즉시 돈을 늘리거나 스테이지 넘길 수 있게 도와 주는 툴
 * 사용법 :
 * 1. 빈 오브젝트나 GameManager에 스크립트 붙인다.
 * 2. 싫행 후 PC -> F12 , 모바일 좌측 상단 누르면 치트창 활성화
 */

public class DebugCheatMenu : MonoBehaviour
{
#if UNITY_EDITOR || DEVLOPMENT_BUILD
    private bool _isShow = false;

    private Rect _windowRect = new Rect(20, 20, 300, 400);

    private void Update()
    {
        //PC 테스트용 : F12 눌렀을 시 ON/OFF
        if (Input.GetKeyDown(KeyCode.F12)) _isShow = !_isShow;

        //모바일 테스트용 : 화면 좌측 상단 (50 X 50 픽셀) 터치시 ON/OFF
        if (Input.GetMouseButtonDown(0))
        {
            if (Input.mousePosition.x < 150 && Input.mousePosition.y > Screen.height - 150)
                _isShow = !_isShow;
        }
    }
    private void OnGUI()
    {
        if (!_isShow) return;

        float targetWidth = Screen.width * 0.85f; 
        float scale = targetWidth / 300f;

        scale = Mathf.Max(1.5f, scale);
        if(scale > 0)
        {
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1));
        }

        _windowRect = GUILayout.Window(999, _windowRect, DrawCheetMenu, "개발자 전용 치트 콘솔");
    }


    private void DrawCheetMenu(int windowID)
    {
        GUILayout.Space(10);
        GUILayout.Label("여기는 팀원들을 위한 공간");
        GUILayout.Label($"현재 스테이지 : {DataManager.Instance.CurrentStage}");
        GUILayout.Label($"현재 골드 : {DataManager.Instance.Gold}");
        GUILayout.Space(10);


        //=====================================================================
        // 재화 관련 치트
        //=====================================================================
        GUILayout.Label("[재화 치트]", EditorStyles.boldLabel);
        if(GUILayout.Button("골드 +10,000 ", GUILayout.Height(40)))
        {
            DataManager.Instance.AddGold(10000);
            Debug.Log("[Cheat] 골드 10,000 추가 완료");
        }

        if(GUILayout.Button("카피 조각 +1,000", GUILayout.Height(40)))
        {
            DataManager.Instance.Addfragments(1000);
            Debug.Log("[Cheat] 카피 조각 1,000 추가 완료");
        }

        GUILayout.Space(10);

        //=====================================================================
        // 스테이지 관련 치트
        //=====================================================================
        GUILayout.Label("[스테이지 치트]", EditorStyles.boldLabel);

        // [테스트 로직] CurrentScene이 null이면 팀원의 개인 테스트 씨능로 간주하여 치트 허용
        bool _isPlayableScene = (SceneManagerEx.Instance.CurrentScene != null) ||
                                (SceneManagerEx.Instance.CurrentScene.SceneType == Define.Scene.MainGame);
        if(GUILayout.Button("다음 스테이지로 강제 이동", GUILayout.Height(40)))
        {
            //1. SceneManagerEx를 통해 현재 씬이 '메인 게임' 인지확인
            if(_isPlayableScene)
            {
                if(StageManager.Instance != null)
                {
                    StageManager.Instance.GoToNextStage();
                    Debug.Log("[Cheat] 스테이지 스킵 완료");
                }
            }
            else
            {
                Debug.LogWarning("[Cheat] 메인 게임 씬에서만 작동하는 치트입니다.");
            }
        }
        if (GUILayout.Button("보스 강제 소환", GUILayout.Height(40)))
        {
            //2. 타이틀 화면 이나 로딩 중일 때 버튼을 눌러서 버그가 나는 것을 방지
            if(_isPlayableScene)
            {
                if(StageManager.Instance != null)
                {
                    StageManager.Instance.ChallengeBoss();
                    Debug.Log("[Cheat] 보스 강제 소환!");
                }
            }
            else
            {
                Debug.LogWarning("[Cheat] 메인 게임 씬에서만 작동하는 치트입니다.");
            }
        }

        GUILayout.Space(20);

        //=====================================================================
        // 데이터 초기화
        //=====================================================================
        GUI.color = Color.red; //버튼 색상 빨간색으로 변경
        if(GUILayout.Button("세이브 데이터 완전히 날리기 (초기화)", GUILayout.Height(40)))
        {
            DataManager.Instance.ResetData();

            Debug.LogWarning("[Cheat] 모든 데이터가 삭제 되고 화면이 0으로 갱신 됩니다.");
        }
        GUI.color = Color.white; // 색상 원상 복구

        // 창을 마우스로 드래그해서 움직일 수 있도록 도와줌
        GUI.DragWindow(new Rect(0, 0, 10000, 10000));

    }
#endif
}
