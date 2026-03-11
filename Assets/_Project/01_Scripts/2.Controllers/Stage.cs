using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage : MonoBehaviour
{
    private bool isMoving = false; //현재 배경이나 몬스터가 이동 중인지 확인

    public List<GameObject> activeMonsters = new List<GameObject>();

    [Header("스폰 몬스터 수")]
    public int monstersPerGroup = 10;

    [Header("몬스터 사이의 간격")]
    public float monsterDistance = 3.0f;

    [Header("몬스터 프리팹")]
    public GameObject monsterBasePrefab;

    [Header("이동 및 배경 설정")]
    public Transform[] backgrounds; //루핑할 배경들
    public float backgroundWidth = 20f; //배경 가로 길이
    public float movingDuration = 0.5f; //몬스터 처치 후 이동 시간

    public void StartNewWave(NormalMonsterData data)
    {
        SpawnMonsterGroup(data);
    }

    private void SpawnMonsterGroup(NormalMonsterData data)
    {
        for (int i = 0; i < monstersPerGroup; i++)
        {
            Vector3 spawnPos = new Vector3(i * monsterDistance, 0, 0);
            GameObject go = Instantiate(monsterBasePrefab, spawnPos, Quaternion.identity);

            if (data.modelPrefab != null)
            {
                GameObject model = Instantiate(data.modelPrefab, go.transform);
                model.transform.localPosition = Vector3.zero; // 중심 맞추기
            }

            Monster monster = go.GetComponent<Monster>();
            if (monster != null)
            {
                float statsMul = StageManager.Instance.currentStageData.statsMultiplier;
                float rewardMul = StageManager.Instance.currentStageData.rewardMultiplier;
                monster.Init(data, statsMul, rewardMul);
            }

            activeMonsters.Add(go);
        }
    }

    public void OnMonsterKilled(GameObject killedMonster)
    {
        if (activeMonsters.Contains(killedMonster))
        {
            activeMonsters.Remove(killedMonster);
        }

        if (isMoving) return;

        //맨 앞의 몬스터가 죽으면 다음 몬스터를 위해 배경과 남은 몬스터를 밀어줌
        StopAllCoroutines();
        StartCoroutine(MoveWorldRoutine());
    }

    IEnumerator MoveWorldRoutine()
    {
        isMoving = true;
        float elapsed = 0f;

        Vector3 moveDistance = Vector3.left * monsterDistance; //몬스터 사이 간격만큼 이동

        while (elapsed < movingDuration)
        {
            float deltaTime = Time.deltaTime;
            float stepSize = (monsterDistance / movingDuration) * deltaTime;
            Vector3 step = Vector3.left * stepSize;

            MoveAndLoopBackgrounds(step); //배경 이동 및 루핑 체크

            foreach (GameObject m in activeMonsters) //남은 몬스터들도 함께 이동
            {
                if (m != null) m.transform.Translate(step);
            }

            elapsed += deltaTime;
            yield return null;
        }

        isMoving = false;

        if (activeMonsters.Count == 0)
        {
            if (StageManager.Instance != null)
                StageManager.Instance.OnWaveCompleted();
        }
    }

    private void MoveAndLoopBackgrounds(Vector3 step)
    {
        foreach (Transform bg in backgrounds)
        {
            bg.Translate(step);

            if (bg.position.x <= -backgroundWidth) //화면 왼쪽 끝에 도달하면 오른쪽 끝으로 이동
            {
                bg.position += new Vector3(backgroundWidth * backgrounds.Length, 0, 0);
            }
        }
    }
}
