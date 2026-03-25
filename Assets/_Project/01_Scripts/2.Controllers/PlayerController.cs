using System.Collections.Generic; 
using UnityEngine;               


public class PlayerController : MonoBehaviour
{
    // 스탯이 변경될 때 UI에 알림을 보내는 이벤트 (UI 갱신용)
    public event System.Action OnStatsChanged;

    [Header("기본 스탯")]
    public string characterName;          // 캐릭터 이름
    public int attackPower { get; private set; }  // 공격력 (외부에서 읽기만 가능, 내부에서만 수정)
    public float critDamage { get; private set; } // 치명타 데미지 배율 (%) → 100%면 2배
    public float critRate { get; private set; }   // 치명타 확률 (%) → 25%면 1/4 확률로 치명타
    public float luck { get; private set; }       // 운 → 추가 골드 획득 확률/양에 영향
    public float attackSpeed { get; private set; }// 공격 속도 증가율 (%)
    public int skillSlots { get; private set; }   // 장착 가능한 스킬 슬롯 수

    [Header("장착된 스킬 (SO 드래그 앤 드롭)")]
    [SerializeField] private List<SkillData> equippedSkills = new List<SkillData>();
    // 인스펙터에서 드래그 앤 드롭으로 설정 가능한 스킬 리스트
    public List<SkillData> EquippedSkills => equippedSkills;
    // 외부에서 읽기 전용 접근 가능 (수정은 불가)

    /// <summary>
    /// 공격 실행 메서드 → 치명타 여부에 따라 데미지 계산
    /// </summary>
    public float Attack()
    {
        // Random.value는 0~1 사이의 랜덤값 반환
        // critRate(%)를 100으로 나눈 값보다 작으면 치명타 발생
        bool isCrit = Random.value < (critRate / 100f);

        // 치명타 여부에 따라 데미지 계산
        float damage = isCrit
            ? attackPower * (1 + critDamage / 100f) // 치명타 시: 공격력 × (1 + 배율)
            : attackPower;                          // 일반 공격 시: 기본 공격력만 적용

        return damage; // 최종 데미지 반환
    }

    /// <summary>
    /// 골드 획득 메서드 → 운(luck)에 따라 추가 골드 계산
    /// </summary>
    public float FarmGold(float baseGold)
    {
        // luck(%)만큼 추가 골드 획득
        float bonus = baseGold * (luck / 100f);
        return baseGold + bonus; // 기본 골드 + 추가 골드 반환
    }

    /// <summary>
    /// DataManager에서 플레이어 레벨을 가져와 공격력 스탯 갱신
    /// </summary>
    public void UpdateStatsFromData()
    {
        int level = 1; // 기본 레벨 (DataManager 없을 경우 1로 설정)

        if (DataManager.Instance != null)
        {
            try
            {
                // DataManager에서 레벨 가져오기 (현재 주석 처리됨)
                // level = Mathf.Max(1, DataManager.Instance.AttackLevel); 
                // 최소 1레벨 보장
            }
            catch
            {
                level = 1; // 예외 발생 시 기본값 유지
            }
        }

        int baseAttack = 50;       // 기본 공격력
        float growthRate = 1.1f;   // 성장률 (레벨이 오를수록 증가율 적용)

        // 공격력 계산 공식:
        // 기본 공격력 + (레벨별 증가치 × 성장률^(레벨-1))
        attackPower = Mathf.RoundToInt(baseAttack + (level * 5) * Mathf.Pow(growthRate, level - 1));
    }

    /// <summary>
    /// 게임 시작 시 초기화
    /// </summary>
    void Start()
    {
        UpdateStatsFromData(); // DataManager 기반으로 공격력 갱신

        // 기본 캐릭터 스탯 설정
        characterName = "용사"; // 캐릭터 이름
        critDamage = 150f;      // 치명타 배율 (150% → 2.5배)
        critRate = 25f;         // 치명타 확률 (25%)
        luck = 10f;             // 운 (10% 추가 골드)
        attackSpeed = 20f;      // 공격 속도 증가율 (20%)
        skillSlots = 3;         // 스킬 슬롯 3개

        NotifyUI(); // 시작 시 UI 갱신 이벤트 호출

        // 테스트용: 5번 공격 실행 (데미지 계산 확인)
        for (int i = 0; i < 5; i++)
        {
            Attack();
        }

        // 테스트용: 골드 파밍 (100 골드 기준)
        FarmGold(100);
    }

    /// <summary>
    /// UpgradeData를 받아서 스탯 업그레이드 적용
    /// </summary>
    public void ApplyUpgrade(UpgradeData data)
    {
        // 업그레이드 이름에 따라 해당 스탯 증가
        switch (data.name)
        {
            case "공격력":
                attackPower += Mathf.RoundToInt(data.value); // 공격력 증가
                break;
            case "치명타 확률":
                critRate += data.value; // 치명타 확률 증가
                break;
            case "운":
                luck += data.value; // 운 증가
                break;
            case "공격 속도":
                attackSpeed += data.value; // 공격 속도 증가
                break;
            default:
                // 알 수 없는 업그레이드 이름일 경우 경고 출력
                Debug.LogWarning($"알 수 없는 업그레이드: {data.name}");
                break;
        }

        NotifyUI(); // UI 갱신 이벤트 호출
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
