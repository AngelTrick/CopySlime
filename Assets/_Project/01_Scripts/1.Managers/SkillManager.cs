using System.Collections.Generic;
using UnityEngine;

public class SkillManager : Singleton<SkillManager>
{
    [Header("스킬 설정")]
    [SerializeField] private LayerMask _enemyLayer;

    [Header("일반공격 설정")]
    [Tooltip("일반 공격으로 사용할 스킬 데이터 (SO)")]
    [SerializeField] private SkillData _basicAttackData;

    private Dictionary<string, float> _cooldownTimers = new Dictionary<string, float>();
    private float _basicAttackTimer = 0f;

    private PlayerController _player;
    private Animator _playerAnimator;

    protected override void Awake()
    {
        base.Awake();

        _player = FindObjectOfType<PlayerController>();
        if (_player != null)
        {
            // 플레이어 캐싱 성공 시 애니메이터도 함께 가져옴
            _playerAnimator = _player.GetComponent<Animator>();
        }
    }

    private void Update()
    {
        if (_player == null) return;

        UpdateCooldowns();

        // 1. 장착된 스킬 중 쿨타임이 찬 것이 있는지 먼저 확인하고 발사
        bool isSkillCasted = TryCastSkills();

        // 2. 만약 이번 프레임에 스킬을 쓰지 않았다면, 일반 공격 발사 시도
        if (!isSkillCasted && _basicAttackData != null)
        {
            TryCastBasicAttack();
        }
    }

    private void UpdateCooldowns()
    {
        // 1. 장착 스킬 쿨타임 감소
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

        // 2. 일반 공격 쿨타임 감소
        if (_basicAttackTimer > 0)
        {
            _basicAttackTimer -= Time.deltaTime;
        }
    }

    // 스킬 발사 성공 여부를 bool로 반환하도록 변경 (일반 공격과 우선순위를 나누기 위함)
    private bool TryCastSkills()
    {
        if (_player.EquippedSkills.Count == 0) return false;

        foreach (SkillData skill in _player.EquippedSkills)
        {
            if (skill == null) continue;

            if (_cooldownTimers[skill.SkillId] <= 0)
            {
                // 캐스팅에 성공했다면 쿨타임을 채우고 true 반환
                if (CastSkill(skill))
                {
                    _cooldownTimers[skill.SkillId] = skill.Cooldown;
                    return true;
                }
            }
        }
        return false;
    }

    private void TryCastBasicAttack()
    {
        if (_basicAttackTimer <= 0)
        {
            if (CastSkill(_basicAttackData))
            {
                // 플레이어의 공격 속도(attackSpeed) 스탯을 반영하여 쿨타임 계산
                // 예: attackSpeed가 100이면 Cooldown 그대로, 200이면 절반으로 줄어듦
                float speedMultiplier = 100f / Mathf.Max(1f, _player.attackSpeed);
                _basicAttackTimer = _basicAttackData.Cooldown * speedMultiplier;
            }
        }
    }

    // [핵심] 3요소(이펙트, 사운드, 애니메이션)가 동시에 터지는 실제 발동 로직
    private bool CastSkill(SkillData skill)
    {
        List<Transform> targets = skill.FindTargets(_player.transform.position, _enemyLayer);

        if (targets.Count == 0) return false;

        // 1. 애니메이션 재생
        if (_playerAnimator != null && !string.IsNullOrEmpty(skill.AnimTriggerName))
        {
            _playerAnimator.SetTrigger(skill.AnimTriggerName);
        }

        // 2. 사운드 재생 (피치 랜덤화 true)
        if (skill.CastSound != null)
        {
            SoundManager.Instance.PlaySFX(skill.CastSound, true);
        }

        // 3. 이펙트(투사체) 소환
        if (skill.EffectPrefab != null)
        {
            foreach (Transform target in targets)
            {
                Vector3 spawnPosition;
                Vector3 fireDirection;

                // SpawnType에 따른 위치 및 방향 분기 처리
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
                    fireDirection = _player.transform.right; 
                }

                // 계산된 위치에 PoolManager를 통해 생성
                GameObject effect = PoolManager.Instance.Pop(skill.EffectPrefab, spawnPosition, Quaternion.identity);
                Projectile projectile = effect.GetComponent<Projectile>();

                if (projectile != null)
                {
                    // 데미지 계산 및 방향(Vector3) 기반 Init 호출
                    float finalDamage = _player.attackPower * skill.DamageMultiplier;
                    projectile.Init(fireDirection, finalDamage);
                }
            }
        }

        return true; // 발사 성공
    }
}