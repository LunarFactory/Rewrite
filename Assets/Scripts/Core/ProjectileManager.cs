using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Weapon;

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

        public void ClearAllProjectiles()
        {
            // 필드에 존재하는 모든 Projectile 컴포넌트를 찾습니다.
            // (성능이 걱정된다면 리스트로 관리하는 방식도 있지만,
            // 웨이브 종료 시 한 번만 실행되므로 이 방식이 가장 확실하고 구현이 쉽습니다.)
            Projectile[] activeProjectiles = FindObjectsByType<Projectile>();

            foreach (Projectile proj in activeProjectiles)
            {
                // 각 투사체 내부의 반납 로직(Deactivate 등)을 호출합니다.
                // 이 메서드는 우리가 앞서 작성했던 'Release' 로직을 포함해야 합니다.
                proj.Deactivate();
            }

            Debug.Log("<color=orange>[Manager]</color> 모든 투사체가 제거되었습니다.");
        }
    }
}
