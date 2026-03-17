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

    //private bool _isBoss = false;

    [Header("보상 드랍 설정")]
    public GameObject goldPrefab;
    public GameObject skinShardPrefab; //조각

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
    private void DropRewards()
    {
        if (goldPrefab != null)
        {
            GameObject goldGo = PoolManager.Instance.Pop(goldPrefab, transform.position, Quaternion.identity);
            Gold goldScript = goldGo.GetComponent<Gold>();
            if (goldScript != null)
            {
                goldScript.Init(_currentGold, false);
            }
        }
        if (data is BossMonsterData && skinShardPrefab != null)
        {
            Vector3 shardPos = transform.position + Vector3.up * 0.5f;
            GameObject shardGo = PoolManager.Instance.Pop(skinShardPrefab, shardPos, Quaternion.identity);
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
            DropRewards();
            StageManager.Instance.AddKillCount();
        }

        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Push(this.gameObject);
        }

        gameObject.SetActive(false);
    }
}
