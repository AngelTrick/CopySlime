using System.Collections.Generic;
using UnityEngine;

public class SkillManager : Singleton<SkillManager>
{
    [Header("Skill Settings")]
    [SerializeField] private LayerMask _enemyLayer;

    private Dictionary<string, float> _cooldownTimers = new Dictionary<string, float>();
    private PlayerController _player;

    protected override void Awake()
    {
        base.Awake();

        _player = FindObjectOfType<PlayerController>();
        if (_player == null)
        {
            Debug.LogWarning("[SkillManager] Player를 찾을 수 없습니다.");
        }
    }

    private void Update()
    {
        if (_player == null || _player.EquippedSkills.Count == 0) return;

        UpdateCooldowns();
        TryCastSkills();
    }

    private void UpdateCooldowns()
    {
        foreach (SkillData skill in _player.EquippedSkills)
        {
            if (skill == null) continue;

            if (!_cooldownTimers.ContainsKey(skill.SkillId))
            {
                _cooldownTimers.Add(skill.SkillId, 0f);
            }

            if (_cooldownTimers[skill.SkillId] > 0)
            {
                _cooldownTimers[skill.SkillId] -= Time.deltaTime;
            }
        }
    }

    private void TryCastSkills()
    {
        foreach (SkillData skill in _player.EquippedSkills)
        {
            if (skill == null) continue;

            if (_cooldownTimers[skill.SkillId] <= 0)
            {
                CastSkill(skill);
            }
        }
    }

    private void CastSkill(SkillData skill)
    {
        List<Transform> targets = skill.FindTargets(_player.transform.position, _enemyLayer);

        if (targets.Count == 0) return;

        _cooldownTimers[skill.SkillId] = skill.Cooldown;

        if (skill.EffectPrefab != null)
        {
            // 타겟 수만큼 반복하며 개별적으로 위치와 방향을 계산하여 투사체 소환
            foreach (Transform target in targets)
            {
                Vector3 spawnPosition;
                Vector3 fireDirection;

                // 1. 새롭게 추가된 SpawnType에 따른 위치 및 방향 분기 처리
                if (skill.SpawnType == SpawnPositionType.Target)
                {
                    // 적 위치 기준 소환 (예: 낙뢰)
                    spawnPosition = target.position + skill.SpawnOffset;
                    fireDirection = Vector3.down;
                }
                else
                {
                    // 플레이어 위치 기준 소환 (예: 검기, 파이어볼)
                    spawnPosition = _player.transform.position + skill.SpawnOffset;
                    // 직선형 게임이므로 플레이어가 바라보는 정면(right)으로 고정 발사
                    fireDirection = _player.transform.right;
                }

                // 2. 계산된 위치에 PoolManager를 통해 생성
                GameObject effect = PoolManager.Instance.Pop(skill.EffectPrefab, spawnPosition, Quaternion.identity);

                Projectile projectile = effect.GetComponent<Projectile>();
                if (projectile != null)
                {
                    // 3. 데미지 계산 및 방향(Vector3) 기반 Init 호출
                    float finalDamage = _player.attackPower * skill.DamageMultiplier;
                    projectile.Init(fireDirection, finalDamage);
                }
            }
        }
    }
}