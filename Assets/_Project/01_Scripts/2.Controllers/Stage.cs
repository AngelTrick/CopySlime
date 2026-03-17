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

    [Header("몬스터 프리팹")]
    public GameObject monsterBasePrefab;

    [Header("이동 및 배경 설정")]
    public Transform[] backgrounds; //루핑할 배경들
    public float backgroundWidth = 20f; //배경 가로 길이
    public float movingDuration = 0.5f; //몬스터 처치 후 이동 시간

    [Header("보스전 전용 설정")]
    public Sprite bossBackgroundSprite; //보스 맵 배경 이미지
    private Sprite _originalFieldSprite; //원래 필드 배경 이미지
    public bool isBossLevel = false; //현재 보스전 상태인지 확인
    
    public void StartNewWave(BaseMonsterData data)
    {
        if (data is BossMonsterData) //보스 데이터인지 체크
        {
            isBossLevel = true;
        }
        else
        {
            isBossLevel = false;
        } 
        SpawnMonsterGroup(data);

        StopAllCoroutines();
        StartCoroutine(MoveWorldRoutine());
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
                spawnPos = new Vector3(bossSpawnPos, 0, 0);
            }
            else
            {
                spawnPos = new Vector3(spawnOffset + (i * monsterDistance), 0, 0);
            }

            GameObject go = PoolManager.Instance.Pop(monsterBasePrefab, spawnPos, Quaternion.identity);

            //몬스터의 능력치 초기화
            Monster monster = go.GetComponent<Monster>();
            if (monster != null)
            {
                float statsMul = StageManager.Instance.currentStageData.statsMultiplier;
                float rewardMul = StageManager.Instance.currentStageData.rewardMultiplier;
                monster.Init(data, statsMul, rewardMul);
            }

            activeMonsters.Add(go); //리스트에 추가해서 관리 시작
        }
    }
    public void EnterBossMap()
    {
        StopAllCoroutines(); //현재 진행 중인 배경 이동이 있다면 멈춤
        isMoving = false;

        foreach (Transform bg in backgrounds)
        {
            SpriteRenderer sr = bg.GetComponent<SpriteRenderer>();
            if (sr != null && bossBackgroundSprite != null)
            {
                //현재의 일반 필드 이미지를 저장
                if (_originalFieldSprite == null) _originalFieldSprite = sr.sprite;

                //배경 이미지를 보스 맵용으로 교체
                sr.sprite = bossBackgroundSprite;
            }
        }
    }
    public void ReturnToField()
    {
        isBossLevel = false; //보스전 상태 해제
        foreach (Transform bg in backgrounds)
        {
            SpriteRenderer sr = bg.GetComponent<SpriteRenderer>();
            if (sr != null && _originalFieldSprite != null)
            {
                //저장해뒀던 원래 필드 이미지로 복구
                sr.sprite = _originalFieldSprite;
            }
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

        float currentDistance;

        if (isBossLevel)
        {
            currentDistance = 5.0f;
        }
        else
        {
            currentDistance = monsterDistance;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player"); //플레이어 찾기 사거리 계산용

        while (true)
        {
            if (activeMonsters.Count > 0)
            {
                GameObject firstTarget = activeMonsters[0];

                if (firstTarget != null && playerObj != null)
                {
                    float distance = Vector3.Distance(firstTarget.transform.position, playerObj.transform.position);

                    float targetRange = 2.0f; //기본값

                    if (firstTarget.TryGetComponent<Monster>(out var m)) targetRange = m.data.attackRange;
                    else if (firstTarget.TryGetComponent<TreasureChest>(out var t)) targetRange = t.attackRange;

                    if (distance <= targetRange)
                    {
                        break;
                    }
                }
            }
            else
            {
                break;
            }

            float deltaTime = Time.deltaTime;
            float speed;

            if (elapsed < movingDuration)
            {
                speed = (currentDistance / movingDuration);
            }
            else
            {
                speed = 2.0f; 
            }
            float currentSpeed = speed;

            Vector3 step = Vector3.left * speed * Time.deltaTime;

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
            bg.Translate(step);

            if (bg.position.x <= -backgroundWidth) //화면 왼쪽 끝에 도달하면 오른쪽 끝으로 이동
            {
                bg.position += new Vector3(backgroundWidth * backgrounds.Length, 0, 0);
            }
        }
    }
}
