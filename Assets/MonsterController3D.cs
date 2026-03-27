using Assets.PixelFantasy.PixelMonsters.Common.Scripts;
using Assets.PixelFantasy.PixelMonsters.Common.Scripts.ExampleScripts;
using System.Linq;
using UnityEngine;

// 3D 물리 컴포넌트로 요구사항 변경
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MonsterAnimation))]
public class MonsterController3D : MonoBehaviour
{
    public Vector2 Input;
    public bool IsGrounded;

    public float Acceleration = 40f;
    public float MaxSpeed = 8f;
    public float JumpForce = 1000f;
    public float Gravity = 70f;

    private Collider _collider;
    private Rigidbody _rigidbody;
    private MonsterAnimation _animation;

    private bool _jump;

    public void Start()
    {
        _collider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
        _animation = GetComponent<MonsterAnimation>();

        // 3D 물리 엔진에서 2D 횡스크롤처럼 완벽하게 동작하게 만드는 핵심 세팅
        _rigidbody.useGravity = false; // 스크립트 내부에서 자체 중력을 계산하므로 엔진 중력은 끔
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
    }

    public void FixedUpdate()
    {
        var state = _animation.GetState();

        if (state == MonsterState.Die) return;

        // 3D이므로 Vector3 사용
        Vector3 velocity = _rigidbody.velocity;

        if (Input.x == 0)
        {
            if (IsGrounded)
            {
                velocity.x = Mathf.MoveTowards(velocity.x, 0, Acceleration * 3f * Time.fixedDeltaTime);
            }
        }
        else
        {
            float maxSpeed = MaxSpeed;
            float acceleration = Acceleration;

            if (_jump)
            {
                acceleration /= 2f;
            }

            velocity.x = Mathf.MoveTowards(velocity.x, Input.x * maxSpeed, acceleration * Time.fixedDeltaTime);
            Turn(velocity.x);
        }

        if (IsGrounded)
        {
            if (!_jump)
            {
                if (Input.x == 0)
                {
                    _animation.Ready();
                }
                else
                {
                    _animation.Run();
                }
            }

            if (Input.y > 0 && !_jump)
            {
                _jump = true;
                // AddForce도 Vector3 기반으로 변경
                _rigidbody.AddForce(Vector3.up * JumpForce);
                _animation.Jump();
            }
        }
        else
        {
            velocity.y -= Gravity * Time.fixedDeltaTime;

            if (velocity.y < 0)
            {
                _jump = true;
                _animation.Fall();
            }
        }

        _rigidbody.velocity = velocity;
    }

    private void Turn(float direction)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(direction) * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    private Collider _ground;

    // OnCollisionEnter2D -> OnCollisionEnter로 3D 충돌 이벤트 변경
    public void OnCollisionEnter(Collision collision)
    {
        // 3D Collision의 contact point 판정 유지
        if (collision.contacts.All(i => i.point.y <= _collider.bounds.min.y + 0.1f))
        {
            IsGrounded = true;
            _ground = collision.collider;

            if (_jump)
            {
                _jump = false;
                _animation.Land();
            }
        }
    }

    // OnCollisionExit2D -> OnCollisionExit로 3D 충돌 이벤트 변경
    public void OnCollisionExit(Collision collision)
    {
        if (IsGrounded && collision.collider == _ground)
        {
            IsGrounded = false;
        }
    }
}