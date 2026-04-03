using System.Collections.Generic;
using UnityEngine;

public enum SpawnPositionType
{
    Player,      // 플레이어 위치에서 발사 (파이어볼, 검기 등)
    Target       // 타겟 위치에서 생성 (낙뢰, 메테오 등)
}

public enum SkillType
{
    Strike,
    Magic,
    Buff
}

public enum SkillGrade
{
    Normal,
    Magic,
    Rare,
    Epic,
    Legendary,
    Mythic
}

public enum TargetingType
{
    Closest,          // 가장 가까운 적의 좌표
    Random,           // 사거리 내 무작위 적의 좌표
    AllInRange,       // 사거리 내 모든 적의 좌표
    AreaRandomBarrage // 타겟을 중심으로 일정 범위(AoE) 내 무작위 좌표 난사
}

[CreateAssetMenu(fileName = "SkillData_New", menuName = "Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string _skillId;
    [SerializeField] private string _skillName;
    [SerializeField] private SkillGrade _skillGrade;
    [SerializeField] private SkillType _skillType;

    [Header("스킬 사용 위치설정")]
    [Tooltip("스킬 이펙트가 생성될 기준 위치")]
    [SerializeField] private SpawnPositionType _spawnType;
    [Tooltip("기준 위치로부터의 오프셋 (예: 적 머리 위면 Y값을 3으로 설정)")]
    [SerializeField] private Vector3 _spawnOffset;

    [Header("쿨타임과 지속시간")]
    [SerializeField] private float _cooldown;
    [SerializeField] private float _duration;
    [Tooltip("이 스킬을 시전한 후, 다음 스킬이 나갈 때까지 대기하는 시간(초)")]
    [SerializeField] private float _castDelay = 0.5f;

    [Header("스킬 작동 방식")]
    [SerializeField] private float _damageMultiplier;
    [Tooltip("체크 시 투사체가 첫 타격 후 파괴되지 않고 모든 적을 뚫고 지나갑니다.")]
    [SerializeField] private bool _isPiercing = false;
    [Tooltip("스킬 발동을 위해 적을 인식하는 최대 거리 (어그로 반경)")]
    [SerializeField] private float _skillRange;
    [Tooltip("적뢰, 폭발 등 스킬이 실제로 피해를 입히는 타격 범위 (Area of Effect)")]
    [SerializeField] private float _areaOfEffect = 5f;
    [SerializeField] private TargetingType _targetType;
    [SerializeField] private int _maxTargetCount;
    [SerializeField] private int _hitCountPerTarget;

    [Header("아이콘과 스킬 비주얼 관련")]
    [SerializeField] private Sprite _skillIcon;
    [SerializeField] private GameObject _effectPrefab;
    [SerializeField] private string _animTriggerName = "AttackSlash";

    [Tooltip("스킬 시전 시 재생할 소리 (예: 기합, 마법 캐스팅)")]
    [SerializeField] private AudioClip _castSound;

    [Tooltip("투사체가 적에게 적중했을 때 재생할 소리 (예: 폭발음, 베는 소리)")]
    [SerializeField] private AudioClip _hitSound;

    // 프로퍼티 매핑
    public string SkillId => _skillId;
    public string SkillName => _skillName;
    public SkillGrade Grade => _skillGrade;
    public SkillType Type => _skillType;
    public float Cooldown => _cooldown;
    public float Duration => _duration;
    public float CastDelay => _castDelay;
    public float DamageMultiplier => _damageMultiplier;
    public float SkillRange => _skillRange;
    public float AreaOfEffect => _areaOfEffect;
    public TargetingType TargetType => _targetType;
    public int MaxTargetCount => _maxTargetCount;
    public int HitCountPerTarget => _hitCountPerTarget;
    public Sprite SkillIcon => _skillIcon;
    public GameObject EffectPrefab => _effectPrefab;
    public string AnimTriggerName => _animTriggerName;
    public AudioClip CastSound => _castSound;
    public AudioClip HitSound => _hitSound;
    public SpawnPositionType SpawnType => _spawnType;
    public Vector3 SpawnOffset => _spawnOffset;
    public bool IsPiercing => _isPiercing;

    // Transform 대신 좌표(Vector3) 리스트를 반환
    public List<Vector3> GetSpawnPositions(Vector3 casterPosition, LayerMask enemyLayer)
    {
        List<Vector3> spawnPositions = new List<Vector3>();

        // 1. 발동 조건 검사: SkillRange 내에 적이 최소 1마리 이상 있는지 확인
        Collider[] colliders = Physics.OverlapSphere(casterPosition, _skillRange, enemyLayer);

        // 헛스윙 방지: 적이 한 마리도 없으면 텅 빈 리스트를 반환하여 스킬 발동 취소
        if (colliders.Length == 0)
        {
            return spawnPositions;
        }

        List<Transform> allTargets = new List<Transform>();
        for (int i = 0; i < colliders.Length; i++)
        {
            allTargets.Add(colliders[i].transform);
        }

        // 2. 타겟팅의 기준점(에피센터)을 잡기 위해 가장 가까운 적 순으로 정렬
        allTargets.Sort((a, b) => Vector3.Distance(casterPosition, a.position).CompareTo(Vector3.Distance(casterPosition, b.position)));
        Transform primaryTarget = allTargets[0]; // 최초로 범위 내에 들어온 가장 가까운 적

        // 3. 타겟팅 방식(TargetingType)에 따른 좌표 계산
        switch (_targetType)
        {
            case TargetingType.Closest:
                int closeCount = Mathf.Min(allTargets.Count, _maxTargetCount);
                for (int i = 0; i < closeCount; i++)
                {
                    spawnPositions.Add(allTargets[i].position + _spawnOffset);
                }
                break;

            case TargetingType.Random:
                int randomCount = Mathf.Min(allTargets.Count, _maxTargetCount);
                for (int i = 0; i < randomCount; i++)
                {
                    int randomIndex = Random.Range(0, allTargets.Count);
                    spawnPositions.Add(allTargets[randomIndex].position + _spawnOffset);
                    allTargets.RemoveAt(randomIndex);
                }
                break;

            case TargetingType.AllInRange:
                for (int i = 0; i < allTargets.Count; i++)
                {
                    spawnPositions.Add(allTargets[i].position + _spawnOffset);
                }
                break;

            case TargetingType.AreaRandomBarrage:
                
                // 가장 가까운 적을 기준으로 무조건 "오른쪽(Vector3.right)"으로만 범위를 전개합니다.
                for (int i = 0; i < _maxTargetCount; i++)
                {
                    // 0부터 AreaOfEffect 사이의 무작위 거리 추출 (음수 없음)
                    float randomXOffset = Random.Range(0f, _areaOfEffect);

                    // 무조건 Vector3.right(X축 양수)를 곱해서 오른쪽으로만 뻗어나가게 고정
                    Vector3 randomPos = primaryTarget.position + (Vector3.right * randomXOffset);

                    spawnPositions.Add(randomPos + _spawnOffset);
                }
                break;
        }

        return spawnPositions;
    }
}