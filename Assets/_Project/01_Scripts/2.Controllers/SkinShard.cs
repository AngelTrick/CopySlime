using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinShard : MonoBehaviour
{
    [Header("조각 설정")]
    public int amount = 1; //실제 조각 개수
    public float collectDistance = 0.5f; //플레이어 획득 거리
    public float magnetDistance = 6.0f; //자석 활성화 거리
    public float moveSpeed = 12f; //플레이어에게 날아가는 속도

    [Header("튕기기 설정")]
    public float groundY = 0f; //바닥 Y 좌표
    public float bounciness = 0.4f; // 탄성
    private int _bounceCount = 0;
    public int maxBounce = 1; //튕김 수

    [Header("튕기기 속도 조절")]
    public float gravityScale = -20f; //중력
    public float explosionForceUp = 10f; //위로 솟구치는 힘
    public float explosionForceSide = 4f; //옆으로 퍼지는 힘

    private Transform _playerTransform;
    private bool _isCollecting = false;
    private bool _canCollect = false;
    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb != null) _rb.useGravity = false;
    }

    public void Init(int skinAmount, bool useExplosion = true)
    {
        amount = skinAmount;
        _isCollecting = false;
        _canCollect = false;
        _bounceCount = 0;

        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
            _rb.drag = 0.5f;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;

            Vector3 pushForce = new Vector3(Random.Range(-explosionForceSide, explosionForceSide),
                Random.Range(explosionForceUp * 0.8f, explosionForceUp), 0f);

            _rb.AddForce(pushForce, ForceMode.Impulse);
        }

        FindPlayer();
        CancelInvoke("EnableCollection");
        Invoke("EnableCollection", 0.7f);
    }
    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;
    }
    private void EnableCollection()
    {
        _canCollect = true;
    }

    void FixedUpdate()
    {
        if (!_isCollecting && _rb != null && !_rb.isKinematic)
        {
            _rb.AddForce(Vector3.up * gravityScale, ForceMode.Acceleration);

            HandleBouncing();
        }
    }

    void Update()
    {
        if (_playerTransform == null) 
        { 
            FindPlayer(); return;
        }

        if (!_isCollecting)
        {
            if (StageManager.Instance.stageController != null && StageManager.Instance.stageController.isMoving)
            {
                transform.Translate(Vector3.left * 2.0f * Time.deltaTime, Space.World);
            }

            float distance = Vector3.Distance(transform.position, _playerTransform.position);
            if (_canCollect && distance < magnetDistance) StartCollecting();
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, _playerTransform.position, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _playerTransform.position) < collectDistance)
            {
                Collect();
            }
        }
    }
    private void HandleBouncing()
    {
        if (transform.position.y <= groundY && _rb.velocity.y < 0)
        {
            if (_bounceCount < maxBounce)
            {
                Vector3 vel = _rb.velocity;
                vel.y = -vel.y * bounciness;
                _rb.velocity = vel;
                _bounceCount++;
            }
            else
            {
                _rb.velocity = Vector3.zero;
                _rb.isKinematic = true;
                transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
            }
        }
    }
    private void StartCollecting()
    {
        _isCollecting = true;
        if (_rb != null) 
        { 
            _rb.isKinematic = true;
        }
    }
    private void Collect()
    {
        if (DataManager.Instance != null)
        {
            //DataManager.Instance.AddSkinShard(amount);
        }
        if (StageManager.Instance != null)
        {
            //  StageManager.Instance.AddSkinShard(amount);
        }

        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Push(this.gameObject);
        }
    }
}
