using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gold : MonoBehaviour
{
    [Header("--- 골드 설정 ---")]
    public int amount = 10; //골드 양
    public float collectDistance = 0.5f; //플레이어가 획득하는 거리
    public float magnetDistance = 6.0f;
    public float moveSpeed = 10f; //플레이어에게 날아가는 속도

    private Transform _playerTransform; //플레이어 위치
    private bool _isCollecting = false; //플레이어에게 날아가는지 체크
    private bool _canCollect = false;
    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb != null) _rb.useGravity = false;
    }

    public void Init(int goldAmount, bool useExplosion = false)
    {
        amount = goldAmount; //넘겨받은 금액
        _isCollecting = false;
        _canCollect = false;

        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
            _rb.drag = 0.3f; //공기 저항
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;

            if (useExplosion) // 상자일 때만 실행!
            {
                Vector3 pushForce = new Vector3(Random.Range(-3f, 3f), Random.Range(5f, 9f), Random.Range(-1f, 1f));
                _rb.AddForce(pushForce, ForceMode.Impulse);
            }
            else
            {
                Vector3 minorForce = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(3f, 3f), 0f);
                _rb.AddForce(minorForce, ForceMode.Impulse);
            }
        }
        FindPlayer();

        CancelInvoke("EnableCollection");
        Invoke("EnableCollection", 0.5f);
    }
    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
    }
    private void EnableCollection()
    {
        _canCollect = true;
    }
    void Update()
    {
        if (_playerTransform == null)
        {
            FindPlayer();
            return;
        }

        float distance = Vector3.Distance(transform.position, _playerTransform.position); //플레이어와 골드의 위치 계산

        if (!_isCollecting)
        {
            if (StageManager.Instance.stageController != null && StageManager.Instance.stageController.isMoving)
            {
                transform.Translate(Vector3.left * 2.0f * Time.deltaTime, Space.World);
            }
            
            if (_canCollect && distance < magnetDistance)
            {
                _isCollecting = true;
                if (_rb != null)
                {
                    _rb.useGravity = false;
                    _rb.isKinematic = true; //자석일 땐 물리 무시
                }
            }
        }
        else
        {
            //플레이어에게 날아가기
            transform.position = Vector3.MoveTowards(transform.position, _playerTransform.position, moveSpeed * Time.deltaTime);

            if (distance < collectDistance) Collect();
        }
    }

    private void Collect()
    {
        if (StageManager.Instance != null) 
        { 
            StageManager.Instance.AddGold(amount);
        }
        if (PoolManager.Instance != null) 
        { 
            PoolManager.Instance.Push(this.gameObject);
        }
    }
}
