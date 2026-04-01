using UnityEngine;
using TMPro;
using DG.Tweening;

/*
 * [타이틀 화면 UI 연출]
 * 역할 : " 화면을 터치해서 시작하세요" 텍스트를 부드럽게 깜빡이게 만듭니다.
 */
public class UITitle : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _touchText;

    [Tooltip("한 번 어두워지거나 밝아지는데 걸리는 시간 (초)")]
    public float blinkDuration = 1f; 

    private void Awake()
    {
        if (_touchText == null) _touchText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (_touchText == null) return;

        // 텍스트의 투명도(Alpha)를 0.2f 까지 blinkDuration(1초) 동안 내립니다.
        // SetLoops(-1, LoopType.Yoyo) : 영원히(-1) 원래대로 돌아갔다 다시 투명해지길(Yoyo) 반복합니다.
        // SetEase(Ease.InOutSine) : 깜빡임이 아주 부드럽게 이어지도록 곡선을 줍니다.

        _touchText.DOFade(0.2f, blinkDuration)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
    }

    // 유저가 터치해서 로딩이 시작되면 텍스트를 숨깁니다.
    public void HideText()
    {
        // 오브젝트 끄기 전에 작동 중이던 DOTween를 깔끔하게 죽여줍니다(kill)
        _touchText.DOKill();
        gameObject.SetActive(false);
    }
}
