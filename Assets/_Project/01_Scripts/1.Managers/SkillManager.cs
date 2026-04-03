using System.Collections;
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
    private float _basicAttackTimer = 0f; // 일반공격(평타)의 기본 쿨타임
    private float _actionDelayTimer = 0f; // 현재 진행 중인 액션(모션)의 남은 딜레이 시간

    private PlayerController _player;
    private Animator _playerAnimator;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        // GameManager의 플레이어 연결 이벤트를 구독
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerSpawned += InitPlayer;

            // 만약 SkillManager가 늦게 켜져서 이미 플레이어가 등록되어 있다면 즉시 초기화
            if (GameManager.Instance.CurrentPlayer != null)
            {
                InitPlayer(GameManager.Instance.CurrentPlayer);
            }
        }
    }

    // 메모리 누수를 방지하기 위한 이벤트 구독 해지
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerSpawned -= InitPlayer;
        }
    }

    // GameManager로부터 플레이어 정보를 전달받는 함수
    private void InitPlayer(PlayerController playerController)
    {
        _player = playerController;

        if (_player != null)
        {
            _playerAnimator = _player.GetComponentInChildren<Animator>();
            Debug.Log("[SkillManager] GameManager로부터 플레이어 정보 수신 완료!");
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

        // 모션 딜레이 처리: 딜레이가 남아있다면 아무 스킬도 쏘지 않고 리턴 (Animation Lock)
        if (_actionDelayTimer > 0)
        {
            _actionDelayTimer -= Time.deltaTime;
            return;
        }

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

    // 불필요한 매개변수를 제거하고 전역 변수 _player를 사용하도록 수정
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
                    Debug.Log(_cooldownTimers[skill.SkillId]);
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

        // 수동 클릭 시에도 모션 딜레이 중복 시전 방지
        if (_actionDelayTimer > 0) return;

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
        // Transform 리스트 대신 Vector3(좌표) 리스트를 받아오도록 변경
        List<Vector3> spawnPositions = skill.GetSpawnPositions(_player.transform.position, _enemyLayer);

        if (spawnPositions.Count == 0) return false;

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
            // 한꺼번에 소환하지 않고 코루틴을 돌려 좌표들에 순차적으로 발사
            StartCoroutine(SpawnSkillEffectsRoutine(skill, spawnPositions));
        }

        // 스킬 시전에 성공했다면, 해당 스킬의 CastDelay만큼 전체 행동을 정지시킴
        _actionDelayTimer = skill.CastDelay;

        return true;
    }

    // 좌표(Vector3)를 받아 처리하도록 변경된 순차 발사 코루틴 함수
    private IEnumerator SpawnSkillEffectsRoutine(SkillData skill, List<Vector3> spawnPositions)
    {
        float delay = skill.Duration > 0 ? skill.Duration / spawnPositions.Count : 0f;
        WaitForSeconds wait = delay > 0 ? new WaitForSeconds(delay) : null;

        foreach (Vector3 targetPos in spawnPositions)
        {
            Vector3 spawnPosition;
            Vector3 fireDirection;

            if (skill.SpawnType == SpawnPositionType.Target)
            {
                // SkillData에서 오프셋 연산이 끝난 좌표를 그대로 사용
                spawnPosition = targetPos;
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

                // 데이터에서 직접 관통 여부(IsPiercing)를 읽어와서 투사체에 주입
                projectile.Init(fireDirection, finalDamage, skill.IsPiercing, skill.HitSound);
            }

            // 계산된 시간만큼 대기한 후 다음 이펙트를 소환
            if (wait != null) yield return wait;
        }
    }

    // 에디터 환경에서만 동작하도록 전처리기를 추가하여 빌드 성능 최적화
#if UNITY_EDITOR
    // 에디터 씬 뷰에서 스킬들의 사거리를 시각적으로 확인하기 위한 기즈모
    private void OnDrawGizmos()
    {
        // 플레이어가 없거나 게임 실행 전(에디터 상태)일 때는 플레이어를 직접 찾아줌
        PlayerController playerTarget = _player;
        if (playerTarget == null)
        {
            playerTarget = FindObjectOfType<PlayerController>();
            if (playerTarget == null) return;
        }

        // 1. 기본 공격 사거리 그리기 (노란색 선)
        if (_basicAttackData != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTarget.transform.position, _basicAttackData.SkillRange);
        }

        // 2. 장착된 오토 스킬들의 사거리 그리기 (청록색 선)
        if (playerTarget != null && playerTarget.EquippedSkills != null)
        {
            Gizmos.color = Color.cyan;
            foreach (SkillData skill in playerTarget.EquippedSkills)
            {
                if (skill != null)
                {
                    Gizmos.DrawWireSphere(playerTarget.transform.position, skill.SkillRange);
                }
            }
        }
    }
#endif
}