using System.Collections.Generic;
using UnityEngine;

    public class PoolManager : Singleton<PoolManager>
    {
    
        private Dictionary<string, Queue<GameObject>> _poolDictionary = new Dictionary<string, Queue<GameObject>>();

        protected override void Awake()
        {
            base.Awake();
        }

        public GameObject Pop(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            string key = prefab.name;

            if (!_poolDictionary.ContainsKey(key))
            {
                _poolDictionary.Add(key, new Queue<GameObject>());
            }

            GameObject obj;

            if (_poolDictionary[key].Count > 0)
            {
                obj = _poolDictionary[key].Dequeue();
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
            }
            else
            {
                obj = Instantiate(prefab, position, rotation);
                obj.name = key; // (Clone) 글자 제거
            }

            return obj;
        }

        public void Push(GameObject obj)
        {
            string key = obj.name;

            if (!_poolDictionary.ContainsKey(key))
            {
                Debug.LogWarning($"넣으려는 풀에 {key}가 존재하지 않습니다. 오브젝트가 파괴됩니다.");
                Destroy(obj);
                return;
            }

            obj.SetActive(false);
            _poolDictionary[key].Enqueue(obj);
        }
    }
