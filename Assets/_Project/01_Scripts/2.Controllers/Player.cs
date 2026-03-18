using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스킬 데이터 클래스: 스킬의 모든 정보를 담음
/// </summary>
/*[System.Serializable]
public class SkillData
{
    public int SkillId { get; set; }          // 스킬 고유 ID
    public string SkillName { get; set; }     // 스킬 이름
    public int DamageMultiplier { get; set; } // 스킬 데미지 배율
    public float Cooldown { get; set; }       // 스킬 쿨타임 (초)

    public override string ToString()
    {
        return $"{SkillName} (ID:{SkillId}, Damage:{DamageMultiplier}, Cooldown:{Cooldown}s)";
    }
}
*/
public class Player : MonoBehaviour
{
    // ------------------ 캐릭터 기본 스탯 ------------------
    // 이제는 필드 대신 프로퍼티로 선언하여 외부에서 get/set 가능
    public string CharacterName { get; set; }   // 캐릭터 이름
    public int AttackPower { get; set; }        // 기본 공격력 (고정 수치)
    public float CritDamage { get; set; }       // 치명타 공격력 배율 (%)
    public float CritRate { get; set; }         // 치명타 확률 (%)
    public float Luck { get; set; }             // 골드 추가 획득량 (%)
    public float AttackSpeed { get; set; }      // 공격속도 증가 (%)
    public int SkillSlots { get; set; }         // 보유 가능한 스킬 슬롯 개수

    // ------------------ 스킬 관련 ------------------
    [SerializeField]
    private List<SkillData> _skills = new List<SkillData>(); // 실제 장착된 스킬 목록

    // 외부에서 읽기 전용으로 접근할 수 있도록 프로퍼티 제공
    public List<SkillData> EquippedSkills => _skills;

    /// <summary>
    /// 캐릭터 현재 상태 출력
    /// </summary>
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
        if (_skills.Count > 0)
        {
            foreach (var skill in _skills)
            {
                Debug.Log($"- {skill}");
            }
        }
        else
        {
            Debug.Log("없음");
        }
      
    }

    /// <summary>
    /// 공격 메서드: 치명타 여부를 판정하고 최종 데미지를 계산
    /// </summary>
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

    /// <summary>
    /// 골드 획득 메서드
    /// </summary>
    public float FarmGold(float baseGold)
    {
        float bonus = baseGold * (Luck / 100f);
        float totalGold = baseGold + bonus;
        Debug.Log($"[GOLD] 기본 {baseGold} + 추가 {bonus:F2} = 총 {totalGold:F2} 골드 획득");
        return totalGold;
    }

    void Start()
    {
        // 예시: 프로퍼티를 통해 값 세팅
        CharacterName = "용사";
        AttackPower = 50;
        CritDamage = 150f;
        CritRate = 25f;
        Luck = 10f;
        AttackSpeed = 20f;
        SkillSlots = 3;

        ShowStatus();

        for (int i = 0; i < 5; i++)
        {
            Attack();
        }

        FarmGold(100);
    }
}
