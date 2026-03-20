using System;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    // [1. 게임 상태 정의]
    // 추후에 관리 차원에서 변경 할 확장 성 있음
    public enum GameState
    {
        Boot,
        StageFarming,
        BossChallenge,
        MenuOpened
    }

    // [2. 현재 상태 변수]
    //외부에서 public get 으로 읽기만 하고, 내부에서 private set  을 통해 수정 가능
    public GameState CurrentState { get; private set; } = GameState.Boot;

    // [3. 상태 변경 이벤트]
    // 상태 바뀔 때 UI 나 StageManager 가 감지 가능 하도록 이벤트 선언
    //<이전 상태, 지금 상태>
    public event Action<GameState, GameState> OnStateChanged;

    protected override void Awake()
    {
        base.Awake();
        Application.targetFrameRate = 60;

        // [추가 될 기능 1 : 화면 꺼짐 방지]
        // 플레이 중일 때는 화면이 꺼지면 안된다.
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void Start()
    {
        // Start 에서 바로 파밍 시작 X , 초기화 (Boot) 후 실행
        ChangeState(GameState.Boot);
        InitializeGame();
    }

    // [추가 될 기능 2 : 매니저 초기화 순서 제거 (Bootstrapping)
    // 초기화 순서를 고정을 해둠으로써 에러를 방지 한다.
    private void InitializeGame()
    {
        Debug.Log("[GameManager] 초기화 진행 중...");

        DataManager.Instance.LoadGameData();    // 1. 게임 세이브 파일 로드
        //SoundManager.Instance.LoadSetting();
        //UIManager.Instance.Init();
        Debug.Log("[GameManager] 게임 초기화 완료");
    }

    //[추가 된 함수 ] 메인 게임 씬이 완전히 열렸을 때 외부에서 호출해 줄 함수
    public void StartMainGameFarming()
    {
        Debug.Log("[GameManager] 메인 씬 확인! 본격적인 파밍을 시작합니다.");
        ChangeState(GameState.StageFarming);
    }

    // [4. 핵심 기능 : 상태 변경]
    // 다른 스크립트에서 사용시 Ex) GameManger.Intance.ChangeState(....) 
    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        GameState previousState = CurrentState;
        CurrentState = newState;

        Debug.Log($"[GameManager]  상태 변환 : {previousState} -> {newState}");

        HandleStateLogic(previousState, newState);

        OnStateChanged?.Invoke(previousState, newState);
    }

    private void HandleStateLogic(GameState fromState, GameState toState)
    {
        //방치형에서는 Time.timeScale = 0; 은 사용 하질 않는다.
        // 사용 한다면 UI 상호 작용 이나 카메라에 사용한다.
        switch (toState)
        {
            case GameState.MenuOpened:
                //Time.timeScale = 1 유지 
                Debug.Log("Farming continue in the background");
                break;
            case GameState.StageFarming:
                break;
            case GameState.BossChallenge:
                break;

        }
    }
    // 상태 변환 테스트 용 함수
    public void ToggleMenu()
    {
        if (CurrentState == GameState.MenuOpened) ChangeState(GameState.StageFarming);
        else ChangeState(GameState.MenuOpened);
    }

    //=============================================================
    // (아래) 방치형 게임이 있어야 할 확장 구역(스켈레톤 으로 작성 후 채워 나갈 예정)
    //=============================================================

    // [추가 될 기능 3 : 오프라인 보상 및 백그라운드 처리
    // 플레이어가 게임을 끄거나 홈 화면으로 나갈 시 처리 하는 부분
    private void OnApplicationPause(bool isPause)
    {
        if (isPause)
        {
            // 앱이 백그라운드 내려감 (일시정지)
            DataManager.Instance?.SaveLogoutTime();
            DataManager.Instance?.SaveGameData();
            Debug.Log("[GameManager] 앱이 백그라운드로 전환 됨 . 데이터 임시 저장");
        }
        else
        {
            // 앱으로 다시 돌아옴 
            // TODO : 나갔던 시간 과 지금 시간 비교 해서 '오프라인 보상 팝업' 띄우기
            Debug.Log("[GameManager] 앱으로 복귀 , 오프라인 보상 계산 시작");
        }
    }

    // [추가 될 기능 4 : 절전 모드 (방치형 필수)
    // 유저가 배터리를 아끼기 위해서 '절전 모드' 버튼 눌렀을 때 화면은 어둡게 만듭니다.
    public void ToggleSleepMode(bool isEnable)
    {
        // TODO : UIManager를 통해 검은색 반 투명 패널을 뛰우거나 프레임 30으로 낮추는 로직
    }

    // [추가 될 기능 5 : 안전한 게임 종료]
    // 유저가 게임을 완전히 껐을 때 호출
    private void OnApplicationQuit()
    {
        //TODO : 게임이 꺼지기 직전에 마지막으로 데이터를 한번 더 꽉 묶어서 저장합니다.
        DataManager.Instance?.SaveGameData();
        Debug.Log("[GameManager] 게임 종료. 데이터 안전 저장 완료");
    }
}

