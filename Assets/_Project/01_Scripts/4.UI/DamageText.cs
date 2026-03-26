using UnityEngine;
using TMPro;
using DG.Tweening;

/*
 * [데미지 텍스트 팝업]
 * 역할 : 몬스터가 맞았을 때 머리 위로 숫자가 떠오르며 스르륵 사라지는 연출
 */

public class DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshPro _textMesh;     // World Space 용 TextMeshPro

    [Header("DOTween 연출 설정")]
    public float moveDistance = 1.5f;   // 위로 올라갈 총 거리 (속도 대신 거리로 직관적 변경)
    public float destroyTime = 0.8f;    // 화면에 머무는 시간 (연출 시간) 

    public void Setup(float damage, bool isCrit = false)
    {
        // 폴링 재사용 시 꼬임 방지를 위해 현재 오브젝트의 모든 트윈 강제 종료
        transform.DOKill();
        _textMesh.DOKill();

        // 1. 데미지 텍스트 세팅
        _textMesh.text = damage.ToString("N0");

        // 위치 살짝 랜덤 (여러 개 겹치지 않도록 퍼트림)
        transform.position += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0f, 0.5f), 0f);
        // X 좌표 [-0.5 ~ 0.5] , Y 좌표 [0 ~ 0.5] , Z 좌표 [0]

        // 2. 크리티컬 여부에 따른 초기 세팅 & 스케일 연출
        if (isCrit)
        {
            _textMesh.color = new Color(1f, 0.2f, 0.22f, 1f); // 찐한 빨간색
            _textMesh.fontSize = 8f;
            _textMesh.fontStyle = FontStyles.Bold;

            // [DOTween 기술 연출] 크리티컬 시 텍스트가 0.5 크기에서 1로 통통 튀며(OutBack) 커짐!
            transform.localScale = Vector3.one * 0.5f;
            transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }
        else
        {
            _textMesh.color = Color.white;
            _textMesh.fontSize = 5f;
            _textMesh.fontStyle = FontStyles.Normal;

            transform.localScale = Vector3.one;             // 일반 타격은 스케일 효과 없음
        }

        // 3. DOTween 이동 & 페이드 아웃 애니메이션 실행

        // Y축으로 moveDistance 만큼 위로 스르륵 올라가기 (점점 느려지는 OutCubic 곡선)
        transform.DOMoveY(transform.position.y + moveDistance, destroyTime).SetEase(Ease.OutCubic);

        // 투명도 0으로 서서히 페이드아웃 되며
        // 연출 100% 끝나면 (OnComplete) 알아서 PoolManager로 반납!
        _textMesh.DOFade(0f, destroyTime).SetEase(Ease.InQuint).OnComplete(() =>
        {
            PoolManager.Instance.Push(gameObject);
        });
    }
}
