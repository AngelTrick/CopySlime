using UnityEngine;

// 데미지를 받을 수 있는 모든 객체가 반드시 가져야 할 규약
public interface IDamageable
{
    // 데미지를 받는 함수
    void TakeDamage(double damage);
}