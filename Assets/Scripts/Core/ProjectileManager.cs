using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Core
{
    public class ProjectileManager : MonoBehaviour
    {
        public static ProjectileManager Instance { get; private set; }

        // 프리팹별로 풀을 관리하는 딕셔너리
        private Dictionary<GameObject, IObjectPool<GameObject>> _pools =
            new Dictionary<GameObject, IObjectPool<GameObject>>();

        private void Awake() => Instance = this;

        public GameObject Get(GameObject prefab)
        {
            if (!_pools.ContainsKey(prefab))
            {
                // 새로운 프리팹용 풀 생성
                _pools[prefab] = new ObjectPool<GameObject>(
                    createFunc: () => Instantiate(prefab),
                    actionOnGet: (obj) => obj.SetActive(true),
                    actionOnRelease: (obj) => obj.SetActive(false),
                    actionOnDestroy: (obj) => Destroy(obj),
                    collectionCheck: false,
                    defaultCapacity: 50,
                    maxSize: 200
                );
            }
            return _pools[prefab].Get();
        }

        public void Release(GameObject prefab, GameObject instance)
        {
            if (_pools.ContainsKey(prefab))
                _pools[prefab].Release(instance);
            else
                Destroy(instance); // 풀이 없으면 그냥 파괴(예외 처리)
        }
    }
}
