using System.Collections;
using UnityEngine;

public class AutoReturn : MonoBehaviour
{
    [Tooltip("오브젝트가 활성화된 후 풀로 반납될 때까지의 시간 (초)")]
    [SerializeField] private float _lifeTime = 1f;

    // 오브젝트가 PoolManager.Pop() 등을 통해 켜질(SetActive(true)) 때마다 자동으로 실행됨
    private void OnEnable()
    {
        StartCoroutine(ReturnToPoolCo());
    }

    private IEnumerator ReturnToPoolCo()
    {
        // 지정된 수명만큼 대기
        yield return new WaitForSeconds(_lifeTime);

        // 대기가 끝나면 자신을 풀로 반납
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Push(gameObject);
        }
        else
        {
            // 만약 씬에 PoolManager가 없는 테스트 환경이라면 그냥 끄기만 함
            gameObject.SetActive(false);
        }
    }
}