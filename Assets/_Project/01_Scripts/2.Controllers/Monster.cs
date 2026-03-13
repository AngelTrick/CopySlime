using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{

    public BaseMonsterData data;

    private float _currentHp;
    private int _currentGold;
    private bool _isDead = false;

    private Transform _target; //테스트용 타겟

    private bool _isBoss = false;

    public void Init(BaseMonsterData newData, float statsMultiplier, float rewardMultiplier)
    {
        data = newData;
        _isDead = false;

        //공통 데이터 적용 (체력)
        _currentHp = data.maxHp * statsMultiplier;

        //공통 데이터 적용 (골드 보상)
        _currentGold = Mathf.RoundToInt(data.dropGold * rewardMultiplier);

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
        if (_isDead) return;
        _isDead = true;

        if (StageManager.Instance != null && StageManager.Instance.stageController != null)
        {
            StageManager.Instance.stageController.OnMonsterKilled(this.gameObject);
        }

        if (StageManager.Instance != null)
        {
            StageManager.Instance.AddGold(_currentGold);
            StageManager.Instance.AddKillCount();
        }

        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Push(this.gameObject);
        }

        gameObject.SetActive(false);
    }
}
