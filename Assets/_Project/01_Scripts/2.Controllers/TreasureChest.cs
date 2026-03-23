using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    [Header("상자 설정")]
    public float maxHp = 50f; //상자의 체력
    private float _currentHp;
    private bool _isDestroyed = false;

    [Header("이동 설정")]
    public float attackRange = 2.0f; //플레이어와 거리

    [Header("보상 설정")]
    public GameObject goldPrefab;
    public int goldCount = 10; //뿌려지는 동전 수
    private int _calculatedGoldPerPiece; //동전 1개당 금액

    public void Init(int totalGoldAmount)
    {
        _isDestroyed = false;
        _currentHp = maxHp;
        gameObject.SetActive(true);

        if (goldCount > 0)
        {
            _calculatedGoldPerPiece = totalGoldAmount / goldCount;
        }
        else
        {
            _calculatedGoldPerPiece = totalGoldAmount;
        }
    }
    void OnEnable()
    {
        _currentHp = maxHp;
        _isDestroyed = false;
    }
    
    private void Explode()
    {
        if (_isDestroyed) return;
        _isDestroyed = true;

        for (int i = 0; i < goldCount; i++)
        {
            if (goldPrefab != null)
            {
                GameObject goldGo = PoolManager.Instance.Pop(goldPrefab, transform.position, Quaternion.identity);
                Gold goldScript = goldGo.GetComponent<Gold>();

                if (goldScript != null)
                {
                    goldScript.Init(_calculatedGoldPerPiece, true);
                }
            }
        }
        if (StageManager.Instance != null && StageManager.Instance.stageController != null)
        {
            StageManager.Instance.stageController.OnMonsterKilled(this.gameObject);
        }

        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Push(this.gameObject);
        }

        gameObject.SetActive(false);
    }
    public void TakeDamage(float damage) 
    {
        if (_isDestroyed) return;
        _currentHp -= damage;
        if (_currentHp <= 0) Explode();
    }
}
