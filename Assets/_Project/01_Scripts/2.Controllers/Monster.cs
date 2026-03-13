using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{

    public BaseMonsterData data;

    private float _currentHp;
    private float _currentAtk; 
    private int _currentGold;
    private bool _isDead = false;

    private float _attackTimer;

    private Transform _target; //테스트용 타겟

    public bool isCollidingWithPlayer = false; //플레이어와 충돌 상태 확인

    public void Init(BaseMonsterData newData, float statsMultiplier, float rewardMultiplier)
    {
        data = newData;
        _isDead = false;
        _attackTimer = 0f;
        isCollidingWithPlayer = false; //충돌 초기화

        //공통 데이터 적용 (체력)
        _currentHp = data.maxHp * statsMultiplier;

        //공통 데이터 적용 (골드 보상)
        _currentGold = Mathf.RoundToInt(data.dropGold * rewardMultiplier);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player"); //테스트용 플레이어 찾기

        //보스 전용 데이터 처리 (공격력)
        if (data is BossMonsterData bossData)
        {
            _currentAtk = bossData.attackPower * statsMultiplier;
        }

        SpawnModel();
    }

    private void SpawnModel()
    {
        foreach (Transform child in transform) { Destroy(child.gameObject); }

        if (data != null && data.modelPrefab != null)
        {
            Instantiate(data.modelPrefab, transform);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isCollidingWithPlayer = true;
        }
    }

    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        _currentHp -= damage;

        if (_currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        _isDead = true;

        FindObjectOfType<Stage>().OnMonsterKilled(this.gameObject);

        if (StageManager.Instance != null)
        {
            StageManager.Instance.AddGold(_currentGold);
            StageManager.Instance.AddKillCount();
        }

        if (data is BossMonsterData boss)
        {
            HandleBossClear(boss);
        }

        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Push(this.gameObject);
        }

        gameObject.SetActive(false);
    }
    private void HandleBossClear(BossMonsterData boss)
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.GoToNextStage();
        }
    }
    void Update()
    {
        if (_isDead || data == null || _target == null) return;

        if (data is BossMonsterData bossData) //보스 데이터일 때 공격
        {
            HandleBossAI(bossData);
        }
    }
    private void HandleBossAI(BossMonsterData bossData)
    {
        if (_target == null) //더미가 사라졌을 때 대비
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                _target = playerObj.transform;
            }
            else
            {
                return;
            }
        }

        float distance = Vector3.Distance(transform.position, _target.position); //실시간 거리 계산

        if (!isCollidingWithPlayer && distance > bossData.attackRange) //플레이어랑 멀리 있으면 충돌하도록 이동
        {
            float directionX;
            float gap = _target.position.x - transform.position.x;

            if (gap > 0) directionX = 1f;
            else directionX = -1f;

            transform.Translate(new Vector3(directionX, 0, 0) * Time.deltaTime * 2.0f);
        }
        else
        {
            _attackTimer += Time.deltaTime; //사거리 안이면 공격
            if (_attackTimer >= bossData.attackSpeed) //쿨다임마다 공격
            {
                AttackPlayer();
                _attackTimer = 0f;
            }
        }
    }
    private void AttackPlayer()
    {
        Debug.Log($"{data.monsterName}의 공격! {_currentAtk} 데미지를 입혔습니다."); //플레이어랑 연결 자리
    }
}
