using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scripts.Data
{
    // 스킬의 성질을 정의하는 열거형
    public enum SkillType
    {
        Strike,     // 물리 타격형 (검기 등)
        Magic,      // 마법형 (낙뢰, 메테오 등)
        Buff        // 버프형 (공격력 증가, 속도 증가 등)
    }

    // 스킬의 등급을 정의하는 열거형 (슬레이어 키우기의 핵심 과금 모델)
    public enum SkillGrade
    {
        Normal,
        Magic,
        Rare,
        Epic,
        Legendary,
        Mythic
    }

    [CreateAssetMenu(fileName = "SkillData_New", menuName = "Project/Skill Data")]
    public class SkillData : ScriptableObject
    {
        [Header("Basic Information")]
        public string skillId;             // 데이터 관리를 위한 고유 ID (예: SKILL_001)
        public string skillName;           // 인게임에 표시될 스킬 이름
        public SkillGrade skillGrade;      // 스킬 등급
        public SkillType skillType;        // 스킬 종류

        [Header("Cost & Time")]
        public float cooldown;             // 재사용 대기시간 (초)
        public float duration;             // 지속 시간 (버프 스킬일 경우 사용, 즉발기면 0)

        [Header("Combat Mechanics")]
        public float damageMultiplier;     // 데미지 배율 (예: 2.5f면 공격력의 250%)
        public int maxTargetCount;         // 최대 타격 가능 몬스터 수
        public int hitCountPerTarget;      // 대상 당 타격 횟수 (다단 히트 스킬용)

        [Header("Visual Resources")]
        public Sprite skillIcon;           // UI에 표시될 스킬 아이콘
        public GameObject effectPrefab;    // PoolManager에서 꺼내올 이펙트/투사체 프리팹
    }
}

