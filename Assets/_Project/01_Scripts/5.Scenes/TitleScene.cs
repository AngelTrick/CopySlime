using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/*
 * [타이틀 씬 컨트롤러 (슬레이어 RPG 스타일)
 * 1. 0~100% 로딩 게이지 연출 및 실제 유저 데이터 패치
 * 2. 완료 후 Touch To Start 깜빡임
 * 3. 터치 시 SceneManagerEx의 UILoading 패널 띄우며 메인 씬 진입
 */
public class TitleScene : BaseScene
{
    [Header("UI 연동(Touch To Start)")]
    public GameObject gameLogo;
    public UITitle uiTitle;

    [Header("UI 연동 (로딩 게이지바")]
    public GameObject loadingPanel;             
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI loadingStateText;

    private bool _isReadyToStart = false;
    private bool _isLoadingNextScene = false;

    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Title;

        //1. 처음엔 로고와 Touch 텍스트 끄고, 로딩 패널만 켜기
        if (gameLogo != null) gameLogo.SetActive(false);
        if (uiTitle != null) uiTitle.gameObject.SetActive(false);
        if (loadingPanel != null) loadingPanel.gameObject.SetActive(true);

        // 초기값 세팅
        if (progressBar != null) progressBar.value = 0f;
        if (progressText != null) progressText.text = "0%";

        // 2. 타이틀 BGM 재생
        // if(SoundManager.Instance != null) SoundManager.Instance.PlayBGM(titleBgm);

        // 3. 로딩 연출 시작!
        StartCoroutine(TitleLoadingRoutine());
    }

    private IEnumerator TitleLoadingRoutine()
    {
        // [1단계] 0~50%: 리소스 체크
        if (loadingStateText != null) loadingStateText.text = "리소스 무결성 검사 중...";

        // DOTween으로 만든 코루틴 대기! (1초 동안 0 -> 0.5)
        yield return FillProgressDOTween(0f, 0.5f, 1.0f);

        //[2단계] 50% : 실제 유저 데이터 로드
        if (loadingStateText != null) loadingStateText.text = "유저 세이브 데이터 동기화 중...";
        if (DataManager.Instance != null) DataManager.Instance.LoadGameData();

        yield return new WaitForSeconds(0.3f);

        //[3단계] 50~100%: 게임 환경 셋팅 (연출)
        if (loadingStateText != null) loadingStateText.text = "게임 환결 설정 중...";

        // DOTween으로 마저 채우기! (1초 동안 0.5 -> 1.0)
        yield return FillProgressDOTween(0.5f, 1.0f, 1.0f);

        //[4단계] 100% 도달
        if (loadingStateText != null) loadingStateText.text = "로딩 완료";
        yield return new WaitForSeconds(0.2f);

        //UI 교체
        if (loadingStateText != null) loadingPanel.SetActive(false);
        if (gameLogo != null) gameLogo.SetActive(true);
        if (uiTitle != null) uiTitle.gameObject.SetActive(true);

        _isReadyToStart = true;
    }

    private IEnumerator FillProgressDOTween(float start, float end, float duration)
    {
        //DOVirtual.Float: 가상의 숫자를 지정한 시간(duration) 동안 start에서 end로 부드럽게 바꿔줍니다.
        Tween tween = DOVirtual.Float(start, end, duration, (value) =>
        {
            // 값이 바뀔 때마다 슬라이더와 텍스트를 동시에 업데이트:
            if (progressBar != null) progressBar.value = value;
            if (progressText != null) progressText.text = $"{Mathf.FloorToInt(value * 100)}%";

        }).SetEase(Ease.OutCubic);  // 끝날 때 쯤 살짝 느려지는 아주 고급스러운 애니메이션 곡선

        // 이 트윈 연출이 완전히 끝날 때까지 코루틴을 대기 시킵니다.
        yield return tween.WaitForCompletion();
    }
    private void Update()
    {
        if (_isReadyToStart && Input.GetMouseButtonDown(0) && !_isLoadingNextScene)
        {
            _isLoadingNextScene = true;

            // 1. 깜빡이던 "Touch to Start" 텍스트 숨기기
            if (uiTitle != null) uiTitle.HideText(); 

            SceneManagerEx.Instance.LoadSceneAsync(Define.Scene.MainGame);

        }
    }

    public override void Clear()
    {
        StopAllCoroutines();

        DOTween.KillAll();
        // 4.메인 씬으로 넘어 가기 전에 메모리 초기화
        if(SoundManager.Instance != null)
        {
            // SoundManager.Instance.StopBGM(); // 타이틀 브금 끄기
        }
        Debug.Log("[TItleScene] 타이틀 씬으로 떠납니다.");
    }
}
