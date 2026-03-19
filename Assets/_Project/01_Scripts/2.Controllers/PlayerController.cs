using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("기본 스탯")]
    public string characterName;
    public int attackPower { get; set; }
    public float critDamage { get; set; }
    public float critRate { get; set; }
    public float luck { get; set; }
    public float attackSpeed { get; set; }
    public int skillSlots { get; set; }

    [Header("장착된 스킬 (SO 드래그 앤 드롭)")]
    [SerializeField] private List<SkillData> equippedSkills = new List<SkillData>();

    public List<SkillData> EquippedSkills => equippedSkills;

    public void ShowStatus()
    {
        Debug.Log($"캐릭터 이름: {characterName}");
        Debug.Log($"공격력: {attackPower}");
        Debug.Log($"치명타 공격력: {critDamage}%");
        Debug.Log($"치명타 확률: {critRate}%");
        Debug.Log($"럭(추가 골드): {luck}%");
        Debug.Log($"공격속도 증가: {attackSpeed}%");
        Debug.Log($"스킬 슬롯: {skillSlots}");
        Debug.Log("장착된 스킬:");

        if (equippedSkills.Count > 0)
        {
            foreach (var skill in equippedSkills)
            {
                Debug.Log($"- {skill.SkillName} (등급:{skill.Grade}, 타입:{skill.Type})");
            }
        }
        else
        {
            Debug.Log("없음");
        }
    }

    public float Attack()
    {
        bool isCrit = Random.value < (critRate / 100f);

        float damage;
        if (isCrit)
        {
            damage = attackPower * (1 + critDamage / 100f);
            Debug.Log($"[CRIT] 치명타 발생! {damage:F2} 데미지를 입혔습니다.");
        }
        else
        {
            damage = attackPower;
            Debug.Log($"[HIT] 일반 공격! {damage:F2} 데미지를 입혔습니다.");
        }

        return damage;
    }

    public float FarmGold(float baseGold)
    {
        float bonus = baseGold * (luck / 100f);
        float totalGold = baseGold + bonus;
        Debug.Log($"[GOLD] 기본 {baseGold} + 추가 {bonus:F2} = 총 {totalGold:F2} 골드 획득");
        return totalGold;
    }
    public void UpdateStatsFromData()
    {
        // DataManager가 없거나 AttackLevel이 정의되지 않았을 경우 대비
        int level = 1; // 기본 레벨

        if (DataManager.Instance != null)
        {
            try
            {
                level = Mathf.Max(1, DataManager.Instance.AttackLevel); // AttacKLevel 부분 데이터 매니저에서 
                                                                        // 추가를 해줘야 attackLevel 빨간줄이 사라집니다 . 
                                                                        // 예시 코드 입니다 . 
                                                                        // public class DataManager : MonoBehaviour 
                                                                        //{
                                                                        //public static DataManager Instance ; 
                                                                        //[Header("플레이어 데이터")]
                                                                        //public int AttackLevel = 1; 

                // private void Awake()
                //{
                // if (Instance == null)
                // {
                // Instance = this ; 
                // DontDestroyOnLoad(gameObject);
                //}

                // else 
                //{
                // Destroy(gameObject);
                //}






            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Player] AttackLevel 접근 실패 → 기본 Lv.1 적용 ({e.Message})");
                level = 1;
            }
        }
        else
        {
            Debug.LogWarning("[Player] DataManager가 존재하지 않습니다. 기본 Lv.1 적용!");
        }

        int baseAttack = 50;
        float growthRate = 1.1f;

        attackPower = Mathf.RoundToInt(baseAttack + (level * 5) * Mathf.Pow(growthRate, level - 1));

        Debug.Log($"[Player] Lv.{level} 스탯 적용 완료 → 공격력: {attackPower}");
    }


    void Start()
    {
        UpdateStatsFromData();


        characterName = "용사";

        critDamage = 150f;
        critRate = 25f;
        luck = 10f;
        attackSpeed = 20f;
        skillSlots = 3;

        ShowStatus();

        foreach (var skill in equippedSkills)
        {
            Debug.Log($"스킬 [{skill.SkillName}] 사용! " +
                      $"배율:{skill.DamageMultiplier}, 쿨타임:{skill.Cooldown}, 범위:{skill.SkillRange}");
        }

        for (int i = 0; i < 5; i++)
        {
            Attack();
        }

        FarmGold(100);
    }

}
