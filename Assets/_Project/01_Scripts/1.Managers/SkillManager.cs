using System.Collections.Generic;
using UnityEngine;

public class SkillManager : Singleton<SkillManager>
{
    [Header("Skill Settings")]
    [SerializeField] private LayerMask _enemyLayer;

    [Header("Basic Attack Settings")]
    [Tooltip("일반 공격으로 사용할 스킬 데이터 (SO)")]
    [SerializeField] private SkillData _basicAttackData;

    // 오토 모드 상태 변수 (기본값은 true로 설정하여 시작 시 자동 사냥)
    private bool _isAutoMode = true;

    private Dictionary<string, float> _cooldownTimers = new Dictionary<string, float>();
    private float _basicAttackTimer = 0f;

    private PlayerController _player;
    private Animator _playerAnimator;

    protected override void Awake()
    {
        base.Awake();

        // 룰 준수: 최초 기동 시에만 Find 계열 함수를 사용하여 캐싱 (성능 최적화)
        _player = FindObjectOfType<PlayerController>();

        if (_player != null)
        {
            _playerAnimator = _player.GetComponentInChildren<Animator>();
        }
        else
        {
            Debug.LogWarning("[SkillManager] PlayerController를 찾을 수 없습니다.");
        }
    }

    // UI 버튼에서 오토 모드를 켜고 끌 때 호출할 외부 접근용 함수
    public void ToggleAutoMode(bool isOn)
    {
        _isAutoMode = isOn;
        Debug.Log($"[SkillManager] 자동 스킬 발사 모드: {_isAutoMode}");
    }

    private void Update()
    {
        if (_player == null) return;

        UpdateCooldowns();

        bool isSkillCasted = false;

        // 오토 모드가 켜져 있을 때만 장착된 스킬의 쿨타임을 검사하고 자동 발사
        if (_isAutoMode)
        {
            isSkillCasted = TryCastAutoSkills();
        }

        // 스킬이 발사되지 않은 프레임이면서 평타 데이터가 있다면 평타 발사 시도
        // (방치형 특성상 평타는 수동 모드라도 쿨타임마다 계속 나감)
        if (!isSkillCasted && _basicAttackData != null)
        {
            TryCastBasicAttack();
        }
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

        if (_basicAttackTimer > 0)
        {
            _basicAttackTimer -= Time.deltaTime;
        }
    }

    private bool TryCastAutoSkills()
    {
        if (_player.EquippedSkills.Count == 0) return false;

        foreach (SkillData skill in _player.EquippedSkills)
        {
            if (skill == null) continue;

            if (_cooldownTimers[skill.SkillId] <= 0)
            {
                if (CastSkill(skill))
                {
                    _cooldownTimers[skill.SkillId] = skill.Cooldown;
                    return true;
                }
            }
        }
        return false;
    }

    // UI 스킬 슬롯 터치를 통한 수동 스킬 발사 함수
    public void CastSkillManually(int slotIndex)
    {
        // 인덱스 방어 코드: 슬롯 범위를 벗어난 터치 무시
        if (_player == null || slotIndex < 0 || slotIndex >= _player.EquippedSkills.Count) return;

        SkillData skillToCast = _player.EquippedSkills[slotIndex];
        if (skillToCast == null) return;

        // 쿨타임 검사 (수동으로 눌러도 쿨타임 중이면 발사 불가)
        if (_cooldownTimers.ContainsKey(skillToCast.SkillId) && _cooldownTimers[skillToCast.SkillId] > 0)
        {
            Debug.Log($"[SkillManager] {skillToCast.SkillName} 스킬은 아직 쿨타임 중입니다.");
            return;
        }

        // 발사 시도 및 쿨타임 초기화
        if (CastSkill(skillToCast))
        {
            _cooldownTimers[skillToCast.SkillId] = skillToCast.Cooldown;
        }
    }

    private void TryCastBasicAttack()
    {
        if (_basicAttackTimer <= 0)
        {
            if (CastSkill(_basicAttackData))
            {
                double speedMultiplier = 100.0 / System.Math.Max(1.0, _player.attackSpeed);
                _basicAttackTimer = (float)(_basicAttackData.Cooldown * speedMultiplier);
            }
        }
    }

    private bool CastSkill(SkillData skill)
    {
        // Tag 대신 LayerMask를 이용한 검출 방식 유지
        List<Transform> targets = skill.FindTargets(_player.transform.position, _enemyLayer);

        if (targets.Count == 0) return false;

        if (_playerAnimator != null && !string.IsNullOrEmpty(skill.AnimTriggerName))
        {
            _playerAnimator.SetTrigger(skill.AnimTriggerName);
        }

        if (skill.CastSound != null)
        {
            SoundManager.Instance.PlaySFX(skill.CastSound, true);
        }

        if (skill.EffectPrefab != null)
        {
            foreach (Transform target in targets)
            {
                Vector3 spawnPosition;
                Vector3 fireDirection;

                if (skill.SpawnType == SpawnPositionType.Target)
                {
                    spawnPosition = target.position + skill.SpawnOffset;
                    fireDirection = Vector3.down;
                }
                else
                {
                    spawnPosition = _player.transform.position + skill.SpawnOffset;
                    fireDirection = _player.transform.right;
                }

                GameObject effect = PoolManager.Instance.Pop(skill.EffectPrefab, spawnPosition, Quaternion.identity);
                Projectile projectile = effect.GetComponent<Projectile>();

                if (projectile != null)
                {
                    double finalDamage = _player.attackPower * skill.DamageMultiplier;
                    projectile.Init(fireDirection, finalDamage);
                }
            }
        }

        return true;
    }
}