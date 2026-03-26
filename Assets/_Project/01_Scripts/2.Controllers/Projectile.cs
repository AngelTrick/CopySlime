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

    // Projectile.cs 내부의 트리거 충돌 함수
    private void OnTriggerEnter(Collider other)
    {
        
        Debug.Log($"[디버그] 투사체가 무언가와 물리적으로 닿았습니다: {other.gameObject.name}");

        if (((1 << other.gameObject.layer) & _enemyLayer) != 0)
            // 1. LayerMask를 이용해 타겟 레이어(적)인지 확인
            if (((1 << other.gameObject.layer) & _enemyLayer) != 0)
        {
            // 2. 부딪힌 게 자식 껍데기더라도, 부모 계층으로 올라가서 IDamageable을 찾아냄!
            IDamageable damageableTarget = other.GetComponentInParent<IDamageable>();

            if (damageableTarget != null)
            {
                damageableTarget.TakeDamage(_damage);
                Debug.Log($"[Projectile] {other.name}에게 {_damage:F2} 데미지 전달 완료!");
            }

            // 타격이 끝났으므로 자신을 창고(풀)로 반납
            PoolManager.Instance.Push(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        // 눈에 확 띄도록 선 색상을 빨간색으로 설정
        Gizmos.color = Color.red;

        // 오브젝트의 회전값(Rotation)과 크기(Scale)를 기즈모에 정확히 반영하기 위한 매트릭스 설정
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

        // 콜라이더 종류에 맞춰 와이어프레임(선) 그리기
        if (col is BoxCollider box)
        {
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }

        // 매트릭스 원상 복구
        Gizmos.matrix = oldMatrix;
    }
}