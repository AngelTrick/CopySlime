using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [Header("투사체 설정")]
    [Tooltip("투사체의 비행 속도")]
    [SerializeField] private float _speed = 15f;

    [Tooltip("투사체의 최대 생존 시간 (도달하지 못할 경우를 대비한 안전장치)")]
    [SerializeField] private float _lifeTime = 3f;

    [Tooltip("충돌을 감지할 적의 레이어")]
    [SerializeField] private LayerMask _enemyLayer;

    // SkillManager로부터 전달받을 내부 데이터
    private Transform _target;
    private float _damage;
    private float _timer;

    public void Init(Transform target, float damage)
    {
        _target = target;
        _damage = damage;
        _timer = 0f; // 풀에서 꺼내질 때마다 타이머 초기화
    }

    private void Update()
    {
        // 1. 수명 초과 시 안전하게 풀로 반납
        _timer += Time.deltaTime;
        if (_timer >= _lifeTime)
        {
            PoolManager.Instance.Push(gameObject);
            return;
        }

        // 2. 타겟이 파괴되었거나(null) 풀로 반납되어 비활성화(activeInHierarchy == false)된 경우 예외 처리
        if (_target == null || !_target.gameObject.activeInHierarchy)
        {
            PoolManager.Instance.Push(gameObject);
            return;
        }

        // 3. 타겟을 향해 이동 (유도탄 로직)
        Vector3 direction = (_target.position - transform.position).normalized;
        transform.position += direction * _speed * Time.deltaTime;

        // 투사체의 앞부분이 타겟을 바라보게 회전
        // (2D 스프라이트를 3D 공간에 띄우는 것이므로, 리소스 방향에 따라 수정이 필요할 수 있음)
        transform.LookAt(_target);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Tag 대신 LayerMask를 이용한 충돌 대상 검사 (비트 연산 활용)
        if (((1 << other.gameObject.layer) & _enemyLayer) != 0)
        {
            Debug.Log($"[Projectile] 적 명중! 전달할 데미지: {_damage:F2}");

            // TODO: other.GetComponent<Enemy>()를 호출하여 데미지 전달 로직 작성

            // 타격이 끝났으므로 자신을 창고(풀)로 반납
            PoolManager.Instance.Push(gameObject);
        }
    }
}