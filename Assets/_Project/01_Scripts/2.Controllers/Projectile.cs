using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [Header("투사체 설정")]
    [Tooltip("투사체의 비행 속도 (낙뢰처럼 제자리에 꽂히는 스킬은 0으로 설정)")]
    [SerializeField] private float _speed = 15f;

    [Tooltip("투사체의 최대 생존 시간 (도달하지 못할 경우를 대비한 안전장치)")]
    [SerializeField] private float _lifeTime = 3f;

    [Tooltip("충돌을 감지할 적의 레이어")]
    [SerializeField] private LayerMask _enemyLayer;

    // SkillManager로부터 전달받을 내부 데이터
    private Vector3 _direction;
    private float _damage;
    private float _timer;

    public void Init(Vector3 direction, float damage)
    {
        _direction = direction.normalized;
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

        // 2. 설정된 방향으로 직선 이동 
        if (_speed > 0)
        {
            transform.position += _direction * _speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // LayerMask를 이용한 충돌 대상 검사 (비트 연산 활용)
        if (((1 << other.gameObject.layer) & _enemyLayer) != 0)
        {
            Debug.Log($"[Projectile] 적 명중! 전달할 데미지: {_damage:F2}");

            // TODO: other.GetComponent<Enemy>()를 호출하여 데미지 전달 로직 작성

            // 타격이 끝났으므로 자신을 창고(풀)로 반납
            PoolManager.Instance.Push(gameObject);
        }
    }
}