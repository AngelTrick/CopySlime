using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    [Header("상자 설정")]
    public float maxHp = 50f;          // 상자의 체력 (몇 번 때려야 부서질지)
    private float _currentHp;
    private bool _isDestroyed = false;

    [Header("이동 설정")]
    public float attackRange = 2.0f; //플레이어와 거리

    [Header("보상 설정")]
    public GameObject goldPrefab;
    public int goldCount = 10; //뿌려지는 동전 수
    public int goldAmountPerPiece = 50; //동전 1개당 금액

    void OnEnable()
    {
        _currentHp = maxHp;
        _isDestroyed = false;
    }
    
    public void TakeDamageH(float damage)
    {
        if (_isDestroyed) return;

        _currentHp -= damage;

        if (_currentHp <= 0)
        {
            Explode(); 
        }
    }
    void Update()
    {
        if (_isDestroyed) return;

        if (StageManager.Instance.stageController != null && StageManager.Instance.stageController.isMoving)
        {
            transform.Translate(Vector3.left * 2.0f * Time.deltaTime);
        }
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
                    goldScript.Init(goldAmountPerPiece, true);
                }
            }
        }
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnWaveCompleted();
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
