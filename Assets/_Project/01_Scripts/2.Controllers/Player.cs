using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("기본 스탯")]
    public string CharacterName;
    public int AttackPower { get; set; }
    public float CritDamage { get; set; }
    public float CritRate { get; set; }
    public float Luck { get; set; }
    public float AttackSpeed { get; set; }
    public int SkillSlots { get; set; }

    [Header("장착된 스킬 (SO 드래그 앤 드롭)")]
    [SerializeField] private List<SkillData> equippedSkills = new List<SkillData>();

    public List<SkillData> EquippedSkills => equippedSkills;

    public void ShowStatus() 
    {
        Debug.Log($"캐릭터 이름: {CharacterName}");
        Debug.Log($"공격력: {AttackPower}");
        Debug.Log($"치명타 공격력: {CritDamage}%");
        Debug.Log($"치명타 확률: {CritRate}%");
        Debug.Log($"럭(추가 골드): {Luck}%");
        Debug.Log($"공격속도 증가: {AttackSpeed}%");
        Debug.Log($"스킬 슬롯: {SkillSlots}");
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
        bool isCrit = Random.value < (CritRate / 100f);

        float damage;
        if (isCrit)
        {
            damage = AttackPower * (1 + CritDamage / 100f);
            Debug.Log($"[CRIT] 치명타 발생! {damage:F2} 데미지를 입혔습니다.");
        }
        else
        {
            damage = AttackPower;
            Debug.Log($"[HIT] 일반 공격! {damage:F2} 데미지를 입혔습니다.");
        }

        return damage;
    }

    public float FarmGold(float baseGold)
    {
        float bonus = baseGold * (Luck / 100f);
        float totalGold = baseGold + bonus;
        Debug.Log($"[GOLD] 기본 {baseGold} + 추가 {bonus:F2} = 총 {totalGold:F2} 골드 획득");
        return totalGold;
    }

    void Start()
    {
        CharacterName = "용사";
        AttackPower = 50;
        CritDamage = 150f;
        CritRate = 25f;
        Luck = 10f;
        AttackSpeed = 20f;
        SkillSlots = 3;

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
