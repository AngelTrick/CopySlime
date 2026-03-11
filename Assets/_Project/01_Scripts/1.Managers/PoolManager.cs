using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scripts.Managers
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance 
        { 
            get;
            private set;
        }

        // 각 프리팹별로 보관할 풀(Queue)을 딕셔너리로 관리
        // Key: 프리팹 이름, Value: 비활성화된 오브젝트들의 큐
        private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 풀에서 오브젝트를 꺼내오거나 없으면 새로 생성함
        /// </summary>
        /// 꺼낼 프리팹, 위치, 회전값을 인자로
        public GameObject Pop(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            string key = prefab.name;

            // 해당 이름의 풀이 없다면 새로 생성
            if (!poolDictionary.ContainsKey(key))
            {
                poolDictionary.Add(key, new Queue<GameObject>());
            }

            GameObject obj;

            // 큐에 비활성화된 오브젝트가 있다면 꺼냄
            if (poolDictionary[key].Count > 0)
            {
                obj = poolDictionary[key].Dequeue();
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
            }
            else
            {
                // 큐가 비어있다면 새로 생성 
                obj = Instantiate(prefab, position, rotation);
                obj.name = key; // (Clone) 글자를 지워야 나중에 다시 넣을 때 key가 일치함
            }

            return obj;
        }

        /// <summary>
        /// 사용이 끝난 오브젝트를 풀로 반납 (비활성화)
        /// </summary>
        public void Push(GameObject obj)
        {
            string key = obj.name;

            if (!poolDictionary.ContainsKey(key))
            {
                // 예외 상황: 풀에 없는 오브젝트를 넣으려 할 때
                Debug.LogWarning($"넣으려는 풀에 {key}가 존재하지 않습니다. 오브젝트가 파괴됩니다.");
                Destroy(obj);
                return;
            }

            obj.SetActive(false);
            poolDictionary[key].Enqueue(obj);
        }
    }
}
