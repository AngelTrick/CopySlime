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
    private double _damage;
    private float _timer;

    public void Init(Vector3 direction, double damage)
    {
        _direction = direction.normalized;
        _damage = damage;
        _timer = 0f; // 풀에서 꺼낼 때마다 타이머 초기화
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
        // 1. LayerMask를 이용해 타겟 레이어(적, 상자 등)인지 1차 확인
        if (((1 << other.gameObject.layer) & _enemyLayer) != 0)
        {
            // 2. 충돌한 오브젝트가 IDamageable 인터페이스를 가지고 있는지 확인
            IDamageable damageableTarget = other.GetComponent<IDamageable>();

            if (damageableTarget != null)
            {
                // 인터페이스를 통해 데미지 전달
                damageableTarget.TakeDamage(_damage);
                Debug.Log($"[Projectile] {other.name}에게 {_damage:F2} 데미지 전달 완료!");
            }

            // 타격이 끝났으므로 자신을 창고(풀)로 반납
            PoolManager.Instance.Push(gameObject);
        }
    }
}