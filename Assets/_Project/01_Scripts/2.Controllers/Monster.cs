using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour , IDamageable
{
    [Header("UI 설정")]
    public GameObject damageTextPrefab;

    public BaseMonsterData data;

    public double currentHp;
    private double _currentGold;
    private bool _isDead = false;

    private Transform _target; //테스트용 타겟

    //private bool _isBoss = false;

    public void Init(BaseMonsterData newData, double statsMultiplier, double rewardMultiplier)
    {
        data = newData;
        _isDead = false;
        gameObject.SetActive(true);

        //공통 데이터 적용 (체력)
        currentHp = (double)data.maxHp * statsMultiplier;

        //공통 데이터 적용 (골드 보상)
        double calculatedGold = (double)data.dropGold * rewardMultiplier;
        if (calculatedGold < 1.0)
        {
            _currentGold = 1.0;
        }
        else
        {
            _currentGold = calculatedGold;
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

    public void TakeDamage(double damage)
    {
        if (_isDead) return;

        currentHp -= damage;

        if (damageTextPrefab != null)
        {
            GameObject textGo = PoolManager.Instance.Pop(damageTextPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);

            DamageText dmgText = textGo.GetOrAddComponent<DamageText>();

            if (dmgText != null)
            {
                dmgText.Setup(damage, false);
            }

        }

            if (currentHp <= 0)
        {
            Die();
        }
    }
    private void DropRewards()
    {
        if (data.goldPrefab != null)
        {
            GameObject goldGo = PoolManager.Instance.Pop(data.goldPrefab, transform.position, Quaternion.identity);
            Gold goldScript = goldGo.GetComponent<Gold>();
            if (goldScript != null)
            {
                goldScript.Init(_currentGold, data is BossMonsterData);
            }
        }
        if (data is BossMonsterData bossData && bossData.skinShardPrefab != null)
        {
                Vector3 shardPos = transform.position + Vector3.up * 0.5f;
                GameObject shardGo = PoolManager.Instance.Pop(bossData.skinShardPrefab, shardPos, Quaternion.identity);

                SkinShard shardScript = shardGo.GetComponent<SkinShard>();
                if (shardScript != null) 
                {
                    shardScript.Init(bossData.dropShardCount, true);
                }
            
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
