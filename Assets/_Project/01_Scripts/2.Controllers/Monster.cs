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

    public void Init(BaseMonsterData newData, float statsMultiplier, float rewardMultiplier)
    {
        data = newData;
        _isDead = false;

        //공통 데이터 적용 (체력)
        _currentHp = data.maxHp * statsMultiplier;

        //공통 데이터 적용 (골드 보상)
        _currentGold = Mathf.RoundToInt(data.dropGold * rewardMultiplier);

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
}
