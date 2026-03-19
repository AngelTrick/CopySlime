using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gold : MonoBehaviour
{
    [Header("골드 설정")]
    public int amount = 10; //골드 양
    public float collectDistance = 0.5f; //플레이어가 획득하는 거리
    public float magnetDistance = 6.0f;
    public float moveSpeed = 10f; //플레이어에게 날아가는 속도

    [Header("튕기기 설정")]
    public float groundY = 0f; //바닥으로 인식할 Y 좌표
    public float bounciness = 0.6f; //튕기는 탄성 (0~1 사이)
    private int _bounceCount = 0;
    public int maxBounce = 2; //최대 몇 번 튕길지

    [Header("튕기기 속도 조절")]
    public float gravityScale = -40f; //중력을 기본값(-9.8)보다 훨씬 높게 설정
    public float explosionForceUp = 12f; //위로 솟구치는 힘의 최댓값
    public float explosionForceSide = 5f; //옆으로 퍼지는 힘의 최댓값

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
        _bounceCount = 0; //튕김 횟수 초기화

        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
            _rb.drag = 0.3f; //공기 저항
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;

            Vector3 pushForce;

            if (useExplosion) // 상자일 때만 실행!
            {
                pushForce = new Vector3(Random.Range(-explosionForceSide, explosionForceSide), 
                    Random.Range(explosionForceUp * 0.8f, explosionForceUp),
                    Random.Range(-1f, 1f));
            }
            else
            {
                pushForce = new Vector3(Random.Range(-2f, 2f), Random.Range(6f, 8f), 0f);
            }
            _rb.AddForce(pushForce, ForceMode.Impulse);
        }
        FindPlayer();

        CancelInvoke("EnableCollection");
        Invoke("EnableCollection", 0.4f);
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
            FindPlayer();
            return;
        }


        if (!_isCollecting)
        {
            if (StageManager.Instance.stageController != null && StageManager.Instance.stageController.isMoving)
            {
                transform.Translate(Vector3.left * 2.0f * Time.deltaTime, Space.World);
            }
            
        float distance = Vector3.Distance(transform.position, _playerTransform.position); //플레이어와 골드의 위치 계산

            if (_canCollect && distance < magnetDistance)
            {
                StartCollecting();
            }
        }
        else
        {
            //플레이어에게 날아가기
            transform.position = Vector3.MoveTowards(transform.position, _playerTransform.position, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _playerTransform.position) < collectDistance)
            {
                Collect();
            }

            }
        }
    private void HandleBouncing()
    {
        if (_rb == null || _rb.isKinematic) return;

        if (transform.position.y <= groundY && _rb.velocity.y < 0)
        {
            if (_bounceCount < maxBounce)
            {
                Vector3 vel = _rb.velocity;  //속도를 반전시키고 탄성 적용
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
            DataManager.Instance.AddGold(amount);
        }
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
