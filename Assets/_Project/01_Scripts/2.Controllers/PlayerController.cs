using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // [이벤트] 스탯 변경을 알리기 위한 이벤트
    // 다른 스크립트(UI 매니저 등)가 이 이벤트를 구독하면,
    // 플레이어 스탯이 갱신될 때 자동으로 UI를 업데이트할 수 있음
    public event System.Action OnStatsChanged;

    [Header("기본 스탯")]
    public string characterName;                     // 캐릭터 이름 (AuthManager 또는 기본값으로 설정)
    public double attackPower { get; private set; }  // 공격력 (외부에서 읽기만 가능, 내부에서만 수정)
    public double critDamage { get; private set; }   // 치명타 데미지 배율 (%) → 100%면 2배 데미지
    public double critRate { get; private set; }     // 치명타 확률 (%) → 25%면 1/4 확률로 치명타 발생
    public double luck { get; private set; }         // 운 → 추가 골드 획득량/확률에 영향
    public double attackSpeed { get; private set; }  // 공격 속도 증가율 (%)
    public int skillSlots { get; private set; }      // 장착 가능한 스킬 슬롯 수

    [Header("장착된 스킬 (SO 드래그 앤 드롭)")]
    [SerializeField] private List<SkillData> equippedSkills = new List<SkillData>();
    // 인스펙터에서 ScriptableObject(SkillData)를 드래그 앤 드롭으로 설정 가능
    // 외부에서는 읽기만 가능하도록 프로퍼티 제공
    public List<SkillData> EquippedSkills => equippedSkills;

    /// <summary>
    /// [공격 실행 메서드]
    /// 치명타 여부를 랜덤으로 판정하여 최종 데미지를 계산 후 반환
    /// </summary>
    public double Attack()
    {
        // Random.value → 0~1 사이의 랜덤값 반환
        // critRate(%)를 100으로 나눈 값보다 작으면 치명타 발생
        bool isCrit = Random.value < (critRate / 100.0);

        // 치명타 여부에 따라 데미지 계산
        double damage = isCrit
            ? attackPower * (1 + critDamage / 100.0) // 치명타 시: 공격력 × (1 + 배율)
            : attackPower;                           // 일반 공격 시: 기본 공격력만 적용

        return damage; // 최종 데미지 반환
    }

    /// <summary>
    /// [골드 획득 메서드]
    /// luck(운) 수치에 따라 추가 골드를 계산하여 반환
    /// </summary>
    public double FarmGold(double baseGold)
    {
        // luck(%)만큼 추가 골드 획득
        double bonus = baseGold * (luck / 100.0);
        return baseGold + bonus; // 기본 골드 + 추가 골드 반환
    }

    /// <summary>
    /// [스탯 갱신 메서드]
    /// DataManager에서 현재 레벨 데이터를 가져와 실제 능력치로 변환
    /// </summary>
    public void UpdateStatsFromData()
    {
        // DataManager가 없으면 기본 1레벨로 세팅
        int atkLv = DataManager.Instance != null ? DataManager.Instance.AttackLevel : 1;
        int critDmgLv = DataManager.Instance != null ? DataManager.Instance.CritDamageLevel : 1;
        int critRateLv = DataManager.Instance != null ? DataManager.Instance.CritRateLevel : 1;
        int luckLv = DataManager.Instance != null ? DataManager.Instance.LuckLevel : 1;
        int atkSpeedLv = DataManager.Instance != null ? DataManager.Instance.AttackSpeedLevel : 1;

        // 공격력 공식 (지수 성장)
        double baseAttack = 10;       // 기본 공격력
        double growthRate = 1.1;      // 성장률 (레벨이 오를수록 증가율 적용)
        attackPower = baseAttack + (atkLv * 5) * System.Math.Pow(growthRate, atkLv - 1);

        // 치명타 데미지 (기본 150% + 렙당 1% 증가)
        critDamage = 150.0 + ((critDmgLv - 1) * 1.0);

        // 치명타 확률 (기본 25% + 렙당 0.1% 증가)
        critRate = 25.0 + ((critRateLv - 1) * 0.1);

        // 운 (기본 10% + 렙당 0.5% 증가)
        luck = 10.0 + ((luckLv - 1) * 0.5);

        // 공격 속도 (기본 20% + 렙당 2% 증가)
        attackSpeed = 20.0 + ((atkSpeedLv - 1) * 2.0);

        Debug.Log($"[Player] 스탯 갱신 완료! (공격력:{attackPower}, 치확:{critRate}%)");

        // UI 갱신 알림
        NotifyUI();
    }

    /// <summary>
    /// [게임 시작 시 초기화]
    /// - GameManager에 플레이어 등록
    /// - DataManager 이벤트 구독
    /// - 최초 스탯 갱신
    /// - AuthManager 닉네임 연동
    /// - 기본 스킬 슬롯 설정
    /// </summary>
    void Start()
    {
        // GameManager에 현재 플레이어 등록
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterPlayer(this);
        }

        // DataManager에서 데이터 변경 이벤트 구독 → 스탯 자동 갱신
        if (DataManager.Instance != null)
            DataManager.Instance.OnDataChanged += UpdateStatsFromData;

        // 최초 스탯 갱신
        UpdateStatsFromData();

        // [수정됨] AuthManager를 통해 발급받은 임시 닉네임 덮어씌우기
        if (AuthManager.Instance != null && !string.IsNullOrEmpty(AuthManager.Instance.CurrentNickname))
        {
            characterName = AuthManager.Instance.CurrentNickname;
            Debug.Log($"[PlayerController] 닉네임 연동 완료: {characterName}");
        }
        else
        {
            // 타이틀을 거치지 않고 메인 씬에서 바로 테스트할 때를 위한 방어 코드
            characterName = "용사";
            Debug.Log("[PlayerController] AuthManager가 없어 기본 이름 '용사'를 사용합니다.");
        }

        // 기본 스킬 슬롯 개수 설정
        skillSlots = 3;

        // UI 갱신 이벤트 호출
        NotifyUI();
    }

    /// <summary>
    /// [OnDestroy]
    /// 오브젝트가 파괴될 때 DataManager 이벤트 구독 해제
    /// (메모리 누수 방지)
    /// </summary>
    private void OnDestroy()
    {
        if (DataManager.Instance != null)
        {
            DataManager.Instance.OnDataChanged -= UpdateStatsFromData;
        }
    }

    /// <summary>
    /// [UI 알림 메서드]
    /// 스탯 변경 이벤트를 호출하여 UI 매니저 등에서 갱신할 수 있도록 함
    /// </summary>
    private void NotifyUI()
    {
        OnStatsChanged?.Invoke();
    }
}
