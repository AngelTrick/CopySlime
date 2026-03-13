using System.Collections.Generic;
using UnityEngine;

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
    Closest,
    Random,
    AllInRange
}

[CreateAssetMenu(fileName = "SkillData_New", menuName = "Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string _skillId;
    [SerializeField] private string _skillName;
    [SerializeField] private SkillGrade _skillGrade;
    [SerializeField] private SkillType _skillType;

    [Header("발동 조건과 지속시간")]
    [SerializeField] private float _cooldown;
    [SerializeField] private float _duration;

    [Header("스킬 작동 방식")]
    [SerializeField] private float _damageMultiplier;
    [SerializeField] private float _skillRange;
    [SerializeField] private TargetingType _targetType;
    [SerializeField] private int _maxTargetCount;
    [SerializeField] private int _hitCountPerTarget;

    [Header("아이콘 및 프리팹")]
    [SerializeField] private Sprite _skillIcon;
    [SerializeField] private GameObject _effectPrefab;

    public string SkillId => _skillId;
    public string SkillName => _skillName;
    public SkillGrade Grade => _skillGrade;
    public SkillType Type => _skillType;
    public float Cooldown => _cooldown;
    public float Duration => _duration;
    public float DamageMultiplier => _damageMultiplier;
    public float SkillRange => _skillRange;
    public TargetingType TargetType => _targetType;
    public int MaxTargetCount => _maxTargetCount;
    public int HitCountPerTarget => _hitCountPerTarget;
    public Sprite SkillIcon => _skillIcon;
    public GameObject EffectPrefab => _effectPrefab;
   
    
       //스킬 범위 내의 적을 탐색하고 조건에 맞게 필터링하여 반환하는 함수
    public List<Transform> FindTargets(Vector3 casterPosition, LayerMask enemyLayer)
    {
        List<Transform> finalTargets = new List<Transform>();

        // 오버랩 스피어를 사용해 범위 내의 모든 콜라이더 검출
        Collider[] colliders = Physics.OverlapSphere(casterPosition, _skillRange, enemyLayer);

        if (colliders.Length == 0)
        {
            return finalTargets;
        }

        List<Transform> allTargets = new List<Transform>();
        for (int i = 0; i < colliders.Length; i++)
        {
            allTargets.Add(colliders[i].transform);
        }

        // 2. 타겟팅 방식(TargetingType)에 따른 분기 처리
        switch (_targetType)
        {
            case TargetingType.Closest:
                // 람다식을 활용한 거리 기준 오름차순 정렬
                allTargets.Sort((a, b) =>
                    Vector3.Distance(casterPosition, a.position).CompareTo(Vector3.Distance(casterPosition, b.position)));

                // 정렬된 리스트에서 최대 타격 수만큼만 앞에서부터 뽑아옴
                int closeCount = Mathf.Min(allTargets.Count, _maxTargetCount);
                for (int i = 0; i < closeCount; i++)
                {
                    finalTargets.Add(allTargets[i]);
                }
                break;

            case TargetingType.Random:
                // 제비뽑기 방식의 랜덤 추출
                int randomCount = Mathf.Min(allTargets.Count, _maxTargetCount);
                for (int i = 0; i < randomCount; i++)
                {
                    // 남은 타겟들 중 무작위 번호표(인덱스) 하나를 뽑음
                    int randomIndex = Random.Range(0, allTargets.Count);

                    // 뽑힌 번호표에 해당하는 타겟을 최종 리스트에 넣음
                    finalTargets.Add(allTargets[randomIndex]);

                    // 중복해서 뽑히지 않도록 원본 리스트에서 해당 타겟을 제거함
                    allTargets.RemoveAt(randomIndex);
                }
                break;

            case TargetingType.AllInRange:
                // Physics.OverlapSphere가 찾은 범위 내의 '모든' 적을 리스트에 추가.
                for (int i = 0; i < allTargets.Count; i++)
                {
                    finalTargets.Add(allTargets[i]);
                }
                break;
        }

        // 3. 필터링이 완료된 최종 타겟 리스트 반환
        return finalTargets;
    }
}