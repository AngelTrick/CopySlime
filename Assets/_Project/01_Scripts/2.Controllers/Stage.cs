using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage : MonoBehaviour
{
    public bool isMoving = false; //현재 배경이나 몬스터가 이동 중인지 확인

    public List<GameObject> activeMonsters = new List<GameObject>();

    [Header("스폰 몬스터 수")]
    public int monstersPerGroup = 10;

    [Header("몬스터 사이의 간격")]
    public float monsterDistance = 3.0f;

    [Header("몬스터 스폰 설정")]
    public float spawnOffset = 5.0f; //일반 몬스터 스폰 위치
    public float bossSpawnPos = 8.0f; //보스 몬스터 스폰 위치
    public float spawnHeight = 0f; //몬스터 스폰 높이
     
    [Header("몬스터 프리팹")]
    public GameObject monsterBasePrefab;

    [Header("이동 및 배경 설정")]
    public List<Transform> backgrounds = new List<Transform>(); //루핑할 배경들
    public float backgroundWidth = 40f; //배경 가로 길이
    public float movingDuration = 0.5f; //몬스터 처치 후 이동 시간

    [Header("배경 관리 설정")]
    public Transform backgroundParent;
    private GameObject _currentBackgroundObj;

    [Header("보스전 전용 설정")]
    public bool isBossLevel = false; //현재 보스전 상태인지 확인

    [Header("플레이어 탐색 설정")]
    public LayerMask playerLayer;

    [Header("배경 위치 설정")]
    public float backgroundYOffset = 0f;

    public void ChangeStageBackground(GameObject bgPrefab)
    {
        if (bgPrefab == null)
        {
            return;
        }

        if (_currentBackgroundObj != null)
        {
            Destroy(_currentBackgroundObj);
        }

        _currentBackgroundObj = Instantiate(bgPrefab, backgroundParent);
        _currentBackgroundObj.transform.localPosition = new Vector3(0, backgroundYOffset, 0);

        backgrounds.Clear();

        foreach (Transform child in _currentBackgroundObj.transform)
        {
            backgrounds.Add(child);
        }
    }
    public void StartNewWave(BaseMonsterData data)
    {
        if (data is BossMonsterData)
        {
            isBossLevel = true;
        }
        else
        {
            isBossLevel = false;
        }

        SpawnMonsterGroup(data);

        if (!isMoving)
        {
            StopAllCoroutines();
            StartCoroutine(MoveWorldRoutine());
        }
    }

    private void SpawnMonsterGroup(BaseMonsterData data)
    {
        int spawnCount;

        if (isBossLevel)
        {
            spawnCount = 1; //보스전이면 딱 1마리만 소환
        }
        else
        {
            spawnCount = monstersPerGroup; //일반 스테이지면 설정한 숫자(10)만큼 소환
        }

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPos;

            if (isBossLevel) //보스 캐릭터 스폰 좌표 8.0
            {
                spawnPos = new Vector3(bossSpawnPos, spawnHeight, 0);
            }
            else
            {
                spawnPos = new Vector3(spawnOffset + (i * monsterDistance), spawnHeight, 0);
            }

            GameObject go = PoolManager.Instance.Pop(monsterBasePrefab, spawnPos, Quaternion.identity);
            go.transform.localScale = Vector3.one * data.monsterScale;
            //몬스터의 능력치 초기화
            Monster monster = go.GetComponent<Monster>();

            if (monster != null && StageManager.Instance.currentStageData != null)
            {
                var sData = StageManager.Instance.currentStageData;

                //실제 스테이지 번호
                int actualStageNum;
                if (DataManager.Instance != null)
                {
                    actualStageNum = DataManager.Instance.CurrentStage;
                }
                else
                {
                    actualStageNum = 1;
                }

                //현제 스테이지 성장률
                double growth = sData.monsterGrowthRate;

                double exponentialMultiplier = System.Math.Pow(growth, actualStageNum - 1);

                double finalStatsMul = (double)sData.statsMultiplier * exponentialMultiplier;
                double finalRewardMul = (double)sData.rewardMultiplier * exponentialMultiplier;

                monster.Init(data, finalStatsMul, finalRewardMul);
            }

            activeMonsters.Add(go); //리스트에 추가해서 관리 시작
        }
    }
    public void EnterBossMap()
    {
        StopAllCoroutines(); //현재 진행 중인 배경 이동이 있다면 멈춤
        isMoving = false;
        isBossLevel = true;
    }
    public void ReturnToField()
    {
        isBossLevel = false; //보스전 상태 해제
    }
    public void OnMonsterKilled(GameObject killedMonster)
    {
        if (activeMonsters.Contains(killedMonster))
        {
            activeMonsters.Remove(killedMonster);
        }

        if (!isMoving)
        {
            StopAllCoroutines();
            StartCoroutine(MoveWorldRoutine());
        }
    }

    IEnumerator MoveWorldRoutine()
    {
        if (isMoving) yield break;
        isMoving = true;
        float elapsed = 0f;

        float currentDistance;

        if (isBossLevel)
        {
            currentDistance = 5.0f;
        }
        else
        {
            currentDistance = monsterDistance;
        }

        Transform playerTarget = null;
        float searchTimeout = 1f;

        while (playerTarget == null && searchTimeout > 0)
        {
            int finalMask;

            if (playerLayer.value == 0)
            {
                finalMask = 1 << LayerMask.NameToLayer("Player");
            }
            else
            {
                finalMask = playerLayer.value;
            }

            Collider[] playerColliders = Physics.OverlapSphere(Vector3.zero, 2000f, finalMask);

            if (playerColliders.Length > 0)
            {
                playerTarget = playerColliders[0].transform;
            }
            else
            {
                searchTimeout -= Time.deltaTime;
                yield return null; 
            }
        }

        if (playerTarget == null)
        {
            isMoving = false;
            yield break;
        }
        while (activeMonsters.Count > 0)
        {
            GameObject firstTarget = activeMonsters[0];

            if (firstTarget != null && playerTarget != null)
            {
                float distance = Vector3.Distance(firstTarget.transform.position, playerTarget.position);
                float targetRange = 2.0f;

                if (firstTarget.TryGetComponent<Monster>(out var m))
                {
                    targetRange = m.data.attackRange;
                }
                else if (firstTarget.TryGetComponent<TreasureChest>(out var t))
                {
                    targetRange = t.attackRange;
                }

                if (distance <= targetRange)
                {
                    break;
                }
            }
            else
            {
                break;
            }

            float speed;

            if (elapsed < movingDuration)
            {
                speed = (currentDistance / movingDuration);
            }
            else
            {
                speed = 2.0f;
            }

            Vector3 step = Vector3.left * speed * Time.deltaTime;

            MoveAndLoopBackgrounds(step);

            foreach (GameObject m in activeMonsters)
            {
                if (m != null) m.transform.Translate(step);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        isMoving = false;

        if (activeMonsters.Count == 0)
        {
            if (StageManager.Instance != null)
            {
                if (isBossLevel) //보스 처치 시 다음으로 이동
                {
                    StageManager.Instance.OnBossClear();
                }
                else
                {
                    StageManager.Instance.OnWaveCompleted(); ; //일반 몬스터 전부 처치 시 다음 무리 소환 예약
                }
            }
        }
    }

    private void MoveAndLoopBackgrounds(Vector3 step)
    {
        foreach (Transform bg in backgrounds)
        {
            if (bg == null) continue;

            bg.Translate(step);

            if (bg.position.x <= -backgroundWidth) //화면 왼쪽 끝에 도달하면 오른쪽 끝으로 이동
            {
                bg.position += new Vector3(backgroundWidth * backgrounds.Count, 0, 0);
            }
        }
    }
}
