using System.Collections.Generic;
using UnityEngine;

public class DummyPlayer_SkillTest : MonoBehaviour
{
    [Header("Test Settings")]
    [Tooltip("F키를 눌러 테스트할 스킬 데이터 SO")]
    [SerializeField] private SkillData _testSkill;

    [Tooltip("스킬이 타겟팅할 적의 레이어")]
    [SerializeField] private LayerMask _enemyLayer;

    [Tooltip("가상의 플레이어 공격력")]
    [SerializeField] private float _dummyAttackPower = 100f;

    private void Update()
    {
        // F 키를 누를 때마다 수동으로 스킬 발사 시도
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryCastManualSkill();
        }
    }

    private void TryCastManualSkill()
    {
        if (_testSkill == null) return;

        List<Transform> targets = _testSkill.FindTargets(transform.position, _enemyLayer);

        if (targets.Count == 0) return;

        if (_testSkill.EffectPrefab != null)
        {
            foreach (Transform target in targets)
            {
                Vector3 spawnPosition;
                Vector3 fireDirection;

                // 스킬 데이터에 따라 소환 위치와 날아갈 방향 결정
                if (_testSkill.SpawnType == SpawnPositionType.Target)
                {
                    // 타겟의 위치 + 오프셋 (예: 적 머리 위 3m)
                    spawnPosition = target.position + _testSkill.SpawnOffset;
                    // 적 머리 위에서 소환되므로 아래(Down)로 내리꽂힘
                    fireDirection = Vector3.down;
                }
                else
                {
                    // 플레이어 위치 + 오프셋 (예: 플레이어 앞쪽 1m)
                    spawnPosition = transform.position + _testSkill.SpawnOffset;
                    // 직선 게임이므로 무조건 앞(right)으로 날아감
                    fireDirection = transform.right;
                }

                GameObject effect = PoolManager.Instance.Pop(_testSkill.EffectPrefab, spawnPosition, Quaternion.identity);

                Projectile projectile = effect.GetComponent<Projectile>();
                if (projectile != null)
                {
                    float finalDamage = _dummyAttackPower * _testSkill.DamageMultiplier;
                    projectile.Init(fireDirection, finalDamage);
                }
            }
        }
    }

    // 씬 뷰에서 스킬 사거리를 파란색 원으로 그려주는 기즈모 함수
    private void OnDrawGizmos()
    {
        if (_testSkill != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _testSkill.SkillRange);
        }
    }
}