using System.Collections.Generic;
using UnityEngine;

public class SkillManager : Singleton<SkillManager>
{
    [Header("Skill Settings")]
    [Tooltip("스킬이 타겟팅할 적의 레이어")]
    [SerializeField] private LayerMask _enemyLayer;

    // 스킬별 남은 쿨타임을 추적하기 위한 딕셔너리
    private Dictionary<string, float> _cooldownTimers = new Dictionary<string, float>();

    // 플레이어의 정보와 위치를 참조하기 위한 캐싱 변수
    private Player _player;

    protected override void Awake()
    {
        base.Awake();

        // 합의된 룰: 최초 기동 시 Find 계열 함수를 사용하여 Player 스크립트 캐싱
        _player = FindObjectOfType<Player>();
        if (_player == null)
        {
            Debug.LogWarning("[SkillManager] Player를 찾을 수 없습니다.");
        }

        // 기존에 있던 _equippedSkills 초기화 루프는 삭제됨
    }

    private void Update()
    {
        // 플레이어가 없거나, 장착된 스킬이 하나도 없으면 작동하지 않음
        if (_player == null || _player.EquippedSkills.Count == 0) return;

        UpdateCooldowns();
        TryCastSkills();
    }

    private void UpdateCooldowns()
    {
        // Player 스크립트에서 장착된 스킬 목록을 실시간으로 가져옴
        foreach (SkillData skill in _player.EquippedSkills)
        {
            if (skill == null) continue;

            // 딕셔너리에 해당 스킬의 타이머가 없다면 (새로 장착된 스킬이라면) 즉시 0으로 추가
            if (!_cooldownTimers.ContainsKey(skill.SkillId))
            {
                _cooldownTimers.Add(skill.SkillId, 0f);
            }

            // 쿨타임 감소 로직
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

            // 쿨타임이 다 찬 스킬만 발사 시도
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
            GameObject effect = PoolManager.Instance.Pop(skill.EffectPrefab, _player.transform.position, Quaternion.identity);

            // 투사체 발사 로직 연동 대기 중
        }
    }
}