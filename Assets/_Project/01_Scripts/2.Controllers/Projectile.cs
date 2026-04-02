using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [Header("투사체 설정")]
    [Tooltip("투사체의 비행 속도 (낙뢰처럼 제자리에 꽂히는 스킬은 0으로 설정)")]
    [SerializeField] private float _speed = 15f;

    [Tooltip("충돌을 감지할 적의 레이어")]
    [SerializeField] private LayerMask _enemyLayer;

    // SkillManager로부터 전달받을 내부 데이터
    private Vector3 _direction;
    private double _damage;
    private bool _isPiercing; // 무한 관통 여부 판단용 변수
    private AudioClip _hitSound; // 타격음 보관용 변수

    // 다단 히트 방어용: 이 투사체가 이미 때린 적들의 명단
    private HashSet<IDamageable> _hitTargets = new HashSet<IDamageable>();

    // Init 함수 확장: 횟수(int) 대신 관통 여부(bool)를 받습니다.
    public void Init(Vector3 direction, double damage, bool isPiercing, AudioClip hitSound = null)
    {
        _direction = direction.normalized;
        _damage = damage;
        _isPiercing = isPiercing;
        _hitSound = hitSound;

        _hitTargets.Clear();
    }

    private void Update()
    {
        if (_speed > 0)
        {
            transform.position += _direction * _speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & _enemyLayer) != 0)
        {
            IDamageable damageableTarget = other.GetComponentInParent<IDamageable>();

            if (damageableTarget != null)
            {
                if (_hitTargets.Contains(damageableTarget)) return;

                damageableTarget.TakeDamage(_damage);
                _hitTargets.Add(damageableTarget);

                if (_hitSound != null)
                {
                    SoundManager.Instance.PlaySFX(_hitSound, true);
                }

                Debug.Log($"[Projectile] {other.name}에게 {_damage:F2} 데미지! (관통 여부: {_isPiercing})");

                // 관통 스킬이 '아닐' 경우에만 스스로를 창고(풀)로 반납
                if (!_isPiercing)
                {
                    PoolManager.Instance.Push(gameObject);
                }
            }
        }
    }

    // 에디터 환경에서만 기즈모를 그리도록 전처리기 추가
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = Color.red;

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

        if (col is BoxCollider box)
        {
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }

        Gizmos.matrix = oldMatrix;
    }
#endif
}