using System.Collections;
using UnityEngine;

/*
 * [Boot씬 컨트롤러]
 * 역할 : 게임을 켜자마자 가장 먼저 실행 하여 필수 초기화 진행 , 타이틀 씬으로 넘어 갑니다.
 */

public class BootScene : BaseScene
{
   protected override void Init()
    {
        base.Init(); // 필수 뼈대 추가

        SceneType = Define.Scene.Boot;
        Debug.Log("[BootSecne] 시스템 부팅을 시작합니다.");

        // 1. 매니저들 완전히 셋팅될 시간을 아주 잠깐 (1프레임) 벌어 준 뒤 타이틀 넘김
        StartCoroutine(LoadTitleScene());
    }

    private IEnumerator LoadTitleScene()
    {
        yield return null;

        Debug.Log("[BootScene] 부팅 완료! 타이틀 씬으로 이동합니다!");

        //SceneManagerEx를 이용하여 타이틀 씬 이동
        SceneManagerEx.Instance.LoadScene(Define.Scene.Title);
    }

    public override void Clear()
    {
        Debug.Log("[BootScene] 부트 씬 종료. 메모리 청소 완료!");
    }
}
