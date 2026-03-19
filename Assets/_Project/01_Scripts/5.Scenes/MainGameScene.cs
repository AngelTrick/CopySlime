using UnityEngine;
/*
 *[메인 씬 컨트롤러] 
 *역할 : 메인 게임 씬 100% 로딩 완료되었을 때 실행 됩니다.
 *여기서 GameManager에게 "씬 로딩 끝났으니 파밍 시작해! 라고 알려 줍니다.
 */
public class MainGameScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.MainGame;

        Debug.Log("[MainGameScene] 메인 게임 씬 세팅 완료! 파밍을 시작합니다");

        if (GameManager.Instance != null)
            GameManager.Instance.StartMainGameFarming();
    }

    public override void Clear()
    {
        Debug.Log("[mainGameScene] 메인 게임 씬 종료. 메모리를 정리합니다.");
    }
}
