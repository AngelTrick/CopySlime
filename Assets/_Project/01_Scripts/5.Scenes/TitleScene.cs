using UnityEngine;

public class TitleScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        // 정체성 선언
        SceneType = Define.Scene.Title;

        Debug.Log("[TitleScene] 타이틀 씬 로드 완료! 세팅을 시작합니다.");
    
        //2. 타이틀 씬 진입 시 해야 할 일
        // TODO : SoundManager.Instance.Play("BGM_Title")
        // TODO : UIManager.Instance.ShowUI("UI_Title")
    }

    private void Update()
    {
        // 3. 유저 입력 대기 : 화면의 아무 곳이나 마우스 클릭(터치)하면 다음 씬으로 넘어 감
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[TItleScene] 화면 터치 감지! 메인 게임으로 진입");

            // TODO : SceneManagerEX 안 로딩 을 완료 한다음에 주석 해제
            // SceneManagerEx.Instance.LoadSceneAsync(Define.Scene.MainGame);

            // 현재는 테스트 용으로 클릭 감지만 함
        }
    }

    public override void Clear()
    {
        // 4.메인 씬으로 넘어 가기 전에 메모리 초기화
        Debug.Log("[TItleScene] 타이틀 씬으로 떠납니다.");
    }
}
