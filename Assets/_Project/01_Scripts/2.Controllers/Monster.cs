using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{

    public BaseMonsterData data;

    private float currentHp;
    private float currentAtk; 
    private int currentGold;
    private bool isDead = false;

    public void Init(BaseMonsterData newData, float statsMultiplier, float rewardMultiplier)
    {
        data = newData;
        isDead = false;

        // 1. 공통 데이터 적용 (체력)
        currentHp = data.maxHp * statsMultiplier;

        // 2. 공통 데이터 적용 (골드 보상)
        currentGold = Mathf.RoundToInt(data.dropGold * rewardMultiplier);

        // 3. 보스 전용 데이터 처리 (공격력)
        if (data is BossMonsterData bossData)
        {
            currentAtk = bossData.attackPower * statsMultiplier;
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
        if (isDead) return;

        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        FindObjectOfType<Stage>().OnMonsterKilled(this.gameObject);

        if (StageManager.Instance != null)
        {
            StageManager.Instance.AddGold(currentGold);
            StageManager.Instance.AddKillCount();
        }

        if (data is BossMonsterData boss)
        {
            HandleBossClear(boss);
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
