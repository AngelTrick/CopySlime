using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // UI에서 스탯이 변경되었음을 알리기 위한 이벤트
    // 다른 스크립트(UI 매니저 등)가 이 이벤트를 구독하면 스탯 변경 시 자동으로 UI 갱신 가능
    public event System.Action OnStatsChanged;

    [Header("기본 스탯")]
    public string characterName;                     // 캐릭터 이름
    public double attackPower { get; private set; }  // 공격력 (외부에서 읽기만 가능, 내부에서만 수정)
    public double critDamage { get; private set; }   // 치명타 데미지 배율 (%) → 100%면 2배
    public double critRate { get; private set; }     // 치명타 확률 (%) → 25%면 1/4 확률로 치명타
    public double luck { get; private set; }         // 운 → 추가 골드 획득 확률/양에 영향
    public double attackSpeed { get; private set; }  // 공격 속도 증가율 (%)
    public int skillSlots { get; private set; }      // 장착 가능한 스킬 슬롯 수

    [Header("장착된 스킬 (SO 드래그 앤 드롭)")]
    [SerializeField] private List<SkillData> equippedSkills = new List<SkillData>();
    // 인스펙터에서 드래그 앤 드롭으로 설정 가능한 스킬 리스트
    public List<SkillData> EquippedSkills => equippedSkills; // 외부에서 읽기 전용 접근 가능

    /// <summary>
    /// 공격 실행 메서드 → 치명타 여부에 따라 데미지 계산
    /// </summary>
    public double Attack()
    {
        // Random.value는 0~1 사이의 랜덤값 반환
        // critRate(%)를 100으로 나눈 값보다 작으면 치명타 발생
        bool isCrit = Random.value < (critRate / 100.0);

        // 치명타 여부에 따라 데미지 계산
        double damage = isCrit
            ? attackPower * (1 + critDamage / 100.0) // 치명타 시: 공격력 × (1 + 배율)
            : attackPower;                           // 일반 공격 시: 기본 공격력만 적용

        return damage; // 최종 데미지 반환
    }

    /// <summary>
    /// 골드 획득 메서드 → 운(luck)에 따라 추가 골드 계산
    /// </summary>
    public double FarmGold(double baseGold)
    {
        // luck(%)만큼 추가 골드 획득
        double bonus = baseGold * (luck / 100.0);
        return baseGold + bonus; // 기본 골드 + 추가 골드 반환
    }

    /// <summary>
    /// DataManager에서 스탯 레벨들을 가져와 실제 능력치로 갱신
    /// </summary>
    public void UpdateStatsFromData()
    {
        // 1. DataManager가 없으면 기본 1레벨로 세팅
        int atkLv = DataManager.Instance != null ? DataManager.Instance.AttackLevel : 1;
        int critDmgLv = DataManager.Instance != null ? DataManager.Instance.CritDamageLevel : 1;
        int critRateLv = DataManager.Instance != null ? DataManager.Instance.CritRateLevel : 1;
        int luckLv = DataManager.Instance != null ? DataManager.Instance.LuckLevel : 1;
        int atkSpeedLv = DataManager.Instance != null ? DataManager.Instance.AttackSpeedLevel : 1;

        // 2. 공격력 공식 적용 (지수 성장 공식)
        double baseAttack = 10;       // 기본 공격력
        double growthRate = 1.1;      // 성장률 (레벨이 오를수록 증가율 적용)
        attackPower = baseAttack + (atkLv * 5) * System.Math.Pow(growthRate, atkLv - 1);

        // 3. 나머지 스탯 공식 적용 (기본값 + (레벨-1) * 렙당증가치)
        // 치명타 데미지 (기본 150% + 렙당 1% 증가)
        critDamage = 150.0 + ((critDmgLv - 1) * 1.0);

        // 치명타 확률 (기본 25% + 렙당 0.1% 증가)
        critRate = 25.0 + ((critRateLv - 1) * 0.1);

        // 운 (기본 10% + 렙당 0.5% 증가)
        luck = 10.0 + ((luckLv - 1) * 0.5);

        // 공격 속도 (기본 20% + 렙당 2% 증가)
        attackSpeed = 20.0 + ((atkSpeedLv - 1) * 2.0);

        Debug.Log($"[Player] 스탯 갱신 완료! (공격력:{attackPower}, 치확:{critRate}%)");

        // 스탯 변경 알림 → UI 갱신
        NotifyUI();
    }

    /// <summary>
    /// 게임 시작 시 초기화
    /// </summary>
    void Start()
    {
        // DataManager에서 데이터가 바뀔 때마다 스탯 갱신 연결
        if (DataManager.Instance != null)
            DataManager.Instance.OnDataChanged += UpdateStatsFromData;

        // 최초 스탯 갱신
        UpdateStatsFromData();

        // 기본 캐릭터 스탯 설정
        characterName = "용사";
        skillSlots = 3;          // 스킬 슬롯 3개

        // UI 갱신 이벤트 호출
        NotifyUI();

        //  [변경] 
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterPlayer(this);
        }
    }

    //  [추가] 
    private void OnDestroy()
    {
        if (DataManager.Instance != null)
        {
            DataManager.Instance.OnDataChanged -= UpdateStatsFromData;
        }
    }

    /// <summary>
    /// UI에 스탯 변경 알림 보내기
    /// </summary>
    private void NotifyUI()
    {
        // 이벤트 구독자가 있을 경우 호출
        OnStatsChanged?.Invoke();
    }
}
