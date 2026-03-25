using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureChest : MonoBehaviour, IDamageable
{
    [Header("상자 설정")]
    public float maxHp = 50f;
    private float _currentHp;
    private bool _isDestroyed = false;

    [Header("보상 설정")]
    public GameObject goldPrefab;
    public int goldCount = 10;
    private int _calculatedGoldPerPiece;

    [Header("이동 및 사거리 설정")]
    public float attackRange = 2.0f;

    [Header("UI 설정")]
    public GameObject damageTextPrefab;

    private SpriteRenderer _sr;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    public void Init(TreasureData data, int totalGoldAmount)
    {
        _isDestroyed = false;

        this.maxHp = data.maxHp;
        this._currentHp = maxHp;
        this.goldCount = data.goldCount;

        transform.localScale = Vector3.one * data.chestScale;

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
        if (damageTextPrefab != null)
        {
            GameObject textGo = PoolManager.Instance.Pop(damageTextPrefab, transform.position + Vector3.up * 1.2f, Quaternion.identity);

            DamageText dmgText = textGo.GetOrAddComponent<DamageText>();
            if (dmgText != null)
            {
                dmgText.Setup(damage, false);
            }
        }
            if (_currentHp <= 0) Explode();
    }
}
