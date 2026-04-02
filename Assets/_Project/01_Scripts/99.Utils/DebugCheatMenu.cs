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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private bool _isShow = false;

    private Rect _windowRect = new Rect(20, 20, 320, 650);

    // 페이지 구분을 위한 변수
    private int _currentTab = 0;
    private string[] _tabNames = { "치트 기능", "플레이어 스탯" };

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

        _windowRect = GUILayout.Window(999, _windowRect, DrawCheatMenu, "개발자 전용 치트 콘솔");
    }
    private void DrawCheatMenu(int windowID)
    {
        GUILayout.Space(10);

        // 상단 탭(페이지) 버튼 생성
        _currentTab = GUILayout.Toolbar(_currentTab, _tabNames, GUILayout.Height(35));
        GUILayout.Space(15);

        // 탭 번호에 따라 다른 화면을 그려줍니다.
        if(_currentTab == 0)
        {
            DrawCheatTab(); // 1페이지 : 기존 치트 기능
        }
        if(_currentTab == 1)
        {
            DrawStatTab(); // 2페이지 : 스탯 표 
        }
        // 창을 마우스로 드래그해서 움직일 수 있도록 도와줌
        GUI.DragWindow(new Rect(0, 0, 10000, 10000));
    } 
    /// <summary>
    /// [페이지 1] 치트 기능들
    /// </summary>
    private void DrawCheatTab()
    {
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
        bool _isPlayableScene = (SceneManagerEx.Instance.CurrentScene == null) ||
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
        // 오프라인 시간 조작 치트 및 실시간 확인
        //=====================================================================
        GUILayout.Label("[오프라인 시간 조작 및 확인]", EditorStyles.boldLabel);

        //--- 실시간 오프라인 시간 확인 로직---
        string _lastTimeStr = DataManager.Instance.LastLogoutTime;
        if (!string.IsNullOrEmpty(_lastTimeStr))
        {
            try
            {
                System.DateTime lastLogoutTime = System.DateTime.Parse(_lastTimeStr);
                System.TimeSpan timepassed = System.DateTime.Now - lastLogoutTime;

                //OnGui가 매 프레임 실행 되므로 시간이 실시간으로 올라가는 것을 볼 수 있음
                GUI.color = Color.cyan;
                GUILayout.Label($" 누적 오프라인 시간 : {timepassed.TotalMinutes:F1}분 지남", EditorStyles.helpBox);
                GUI.color = Color.white;
            }
            catch
            {
                GUILayout.Label(" 시간 데이터 오류", EditorStyles.helpBox);
            }
        }
        else
        {
            GUILayout.Label("저장된 로그아웃 시간 없음", EditorStyles.helpBox); 
        }
        // ====================================================================

        if(GUILayout.Button("마지막 접속을 1시간 전으로 세팅", GUILayout.Height(40)))
        {
            System.DateTime fakeTime = System.DateTime.Now.AddHours(-1);
            // [수정됨] DataManager 통해 JSON 즉시 저장 26/03/22  
            DataManager.Instance.SetLogoutTImeForCheat(fakeTime.ToString());
            Debug.Log("[Cheat] 로그아웃 시간이 1시간 전으로 조작 되었습니다. 유니티 플레이를 껐다가 다시 켜보세요");
        }
        if (GUILayout.Button("마지막 접속을 24시간 전으로 세팅", GUILayout.Height(40)))
        {
            System.DateTime fakeTime = System.DateTime.Now.AddHours(-24);
            DataManager.Instance.SetLogoutTImeForCheat(fakeTime.ToString());
            Debug.Log("[Cheat] 로그아웃 시간이 24시간 전으로 조작 되었습니다. 유니티 플레이를 껐다가 다시 켜보세요");
        }

        // UI팀원 연동 확인용 다이렉트 버튼
        GUILayout.Space(5);
        GUI.color = Color.green;
        if(GUILayout.Button("UI 팝업 즉시 강제 띄우기 (테스트용)", GUILayout.Height(40)))
        {
            if(UIManager.Instance != null)
            {
                // 강제로 120분 방치. 6000골드 획득한 것 처럼 UI만 띄움
                UIManager.Instance.ShowOfflineReward(120, 6000);
                Debug.Log("[Cheat] UI 오프라인 강제 호출 완료");
            }
            else
            {
                Debug.LogWarning("[Cheat] UIManger가 없습니다.");
            }
        }
        GUI.color = Color.white;

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
    }
    private void DrawStatTab()
    {
        PlayerController player = null;
        if(GameManager.Instance != null)
        {
            player = GameManager.Instance.CurrentPlayer;
        }

        // 못 찾았다면 에러 메시지 출력
        if(player == null)
        {
            GUILayout.Space(20);
            GUILayout.Label("메인 게임 씬이 아닙니다. \n 플레이어를 찾을 수 없습니다.", EditorStyles.helpBox);
            return;
        }

        //스탯 표시용 GUI 스타일 꾸미기
        GUIStyle statStyle = new GUIStyle(GUI.skin.label);
        statStyle.fontSize = 13;
        statStyle.richText = true;      // 색생 태그 사용 허용

        GUILayout.Label("<b>[현재 플레이어 스탯 & 레벨 현향]</b>", statStyle);
        GUILayout.Space(5);

        // 보기 좋게 박스 안에 가두기
        GUILayout.BeginVertical("box");

        // 실제 계산된 스탯과, DataManager에 저장된 레벨을 매칭해 보여줍니다.

        GUILayout.Label($"공격력 : <color=#facc15>{player.attackPower.ToSmartCurrency()}</color> <color=#9ca3af>(Lv.{DataManager.Instance.AttackLevel})</color>",statStyle);
        GUILayout.Label($"치명타 확률 : <color=#facc15>{player.critRate:F1}%</color> <color=#9ca3af>(Lv.{DataManager.Instance.CritRateLevel})</color>", statStyle);
        GUILayout.Label($"치명타 데미지 : <color=#facc15>{player.critDamage:F1}%</color> <color=#9ca3af>(Lv.{DataManager.Instance.CritDamageLevel})</color>", statStyle);
        GUILayout.Label($"공격 속도 : <color=#facc15>{player.attackSpeed:F1}%</color> <color=#9ca3af>(Lv.{DataManager.Instance.AttackSpeedLevel})</color>", statStyle);
        GUILayout.Label($"골드 획득 운 : <color=#facc15>{player.luck:F1}%</color> <color=#9ca3af>(Lv.{DataManager.Instance.AttackLevel})</color>", statStyle);

        GUILayout.EndVertical();

        GUILayout.Space(20);

        // 스탯 동기화 버튼
        if(GUILayout.Button("현재 레벨 기반 스탯 재계산", GUILayout.Height(40)))
        {
            player.UpdateStatsFromData();
            Debug.Log("[Cheat] 플레이어 스탯 수동 갱신 완료");
        }

        GUILayout.Space(10);
        GUILayout.Label("<color=#9ca3af> * 위 스탯은 공식이 적용된 실제 최종 수치입니다.</color>", statStyle);
    }
#endif
}
