using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{

    public BaseMonsterData data;

    private float currentHp;
    private bool isDead = false;

    void OnEnable()
    {
        if (data != null)
        {
            InitializeMonster();
        }
    }

    private void InitializeMonster()
    {
        currentHp = data.maxHp;
        isDead = false;
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

        if (data is BossMonsterData boss)
        {
            HandleBossClear(boss);
        }

        gameObject.SetActive(false);
    }
    private void HandleBossClear(BossMonsterData boss)
    {

    }
}
