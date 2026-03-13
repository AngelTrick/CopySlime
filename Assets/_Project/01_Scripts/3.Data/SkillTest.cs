using System.Collections.Generic;
using UnityEngine;

public class SkillTester : MonoBehaviour
{
    [Header("Test Settings")]
    [Tooltip("테스트할 스킬 데이터 (SO)")]
    [SerializeField] private SkillData _testSkill;

    [Tooltip("검출할 적의 레이어")]
    [SerializeField] private LayerMask _enemyLayer;

    private void Update()
    {
        // 스페이스바를 누르면 검출 테스트 실행
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestFindTargets();
        }
    }

    private void TestFindTargets()
    {
        if (_testSkill == null)
        {
            Debug.LogWarning("테스트할 스킬 데이터가 비어있습니다.");
            return;
        }

        // SkillData의 탐색 로직 호출
        List<Transform> targets = _testSkill.FindTargets(transform.position, _enemyLayer);

        // 결과 출력
        Debug.Log("=================================");
        Debug.Log($"스킬 이름: {_testSkill.SkillName} / 타겟팅 방식: {_testSkill.TargetType}");
        Debug.Log($"검출된 타겟 수: {targets.Count}명 (최대 {_testSkill.MaxTargetCount}명)");

        for (int i = 0; i < targets.Count; i++)
        {
            // 검출된 더미의 이름과, 플레이어로부터의 거리를 함께 출력
            float distance = Vector3.Distance(transform.position, targets[i].position);
            Debug.Log($"[{i}] 이름: {targets[i].name} / 거리: {distance:F2}");
        }
        Debug.Log("=================================");
    }

   
    private void OnDrawGizmos()
    {
        if (_testSkill != null)
        {
            Gizmos.color = Color.red;
            // 플레이어 위치를 중심으로 스킬 사거리만큼의 와이어 스피어를 그림
            Gizmos.DrawWireSphere(transform.position, _testSkill.SkillRange);
        }
    }
}