using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player 캐릭터 클래스
/// - MonoBehaviour를 상속받아 Unity에서 동작
/// - 공격, 스킬 장착, 골드 파밍 등 다양한 기능 포함
/// </summary>
public class Player : MonoBehaviour
{
    // ------------------ 캐릭터 기본 스탯 ------------------
    public string characterName;
    public int attackPower;
    public float critDamage;
    public float critRate;
    public float luck;
    public float attackSpeed;
    public int skillSlots;

    // ------------------ 스킬 관련 ------------------
    [SerializeField]
    private List<SkillData> _skills = new List<SkillData>();

    
    public List<SkillData> EquippedSkills => _skills;

    public void ShowStatus()
    {
        Debug.Log("========================================");
        Debug.Log($"캐릭터 이름: {characterName}");
        Debug.Log($"공격력: {attackPower}");
        Debug.Log($"치명타 공격력: {critDamage}%");
        Debug.Log($"치명타 확률: {critRate}%");
        Debug.Log($"럭(추가 골드): {luck}%");
        Debug.Log($"공격속도 증가: {attackSpeed}%");
        Debug.Log($"스킬 슬롯: {skillSlots}");
        Debug.Log("장착된 스킬:");

        if (_skills.Count > 0)
        {
            foreach (var skill in _skills)
            {               
                Debug.Log($"- {skill.SkillName} (ID:{skill.SkillId}, 데미지배율:{skill.DamageMultiplier}, 쿨타임:{skill.Cooldown}s)");
            }
        }
        else
        {
            Debug.Log("없음");
        }
        Debug.Log("========================================");
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

    void Start()
    {
       
        characterName = "DarkSlayer";
        attackPower = 120;
        critDamage = 50;
        critRate = 25;
        luck = 10;
        attackSpeed = 15;
        skillSlots = 3;

        ShowStatus();
    }
}