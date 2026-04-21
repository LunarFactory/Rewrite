using UnityEngine;

namespace Drone
{
    public class DroneBase : MonoBehaviour
    {
        [Header("Visuals")]
        public Sprite[] droneSprites; // 여기에 4장의 스프라이트 할당
        public float animFPS = 10f;
        private DroneAnimator _animator = new DroneAnimator();
        private Transform _player;
        private float _targetOffset;  // 내가 가야 할 목표 간격
        private float _currentOffset; // 현재 내가 유지 중인 간격

        public void SetCenter(Transform player, float unused) // startAngle은 이제 필요 없음
        {
            _player = player;
        }

        // 매니저가 호출할 함수: "너는 전체 중 이 각도만큼 떨어져서 돌아라"
        public void SetTargetOffset(float angle)
        {
            _targetOffset = angle;
        }
        private void Awake()
        {
            // 자식에 있는 SpriteRenderer를 찾아서 초기화
            var sr = GetComponent<SpriteRenderer>();
            _animator.Initialize(sr);
        }

        private void Start()
        {
            // 태어나자마자 매니저에 등록
            if (DroneManager.Instance != null) DroneManager.Instance.RegisterDrone(this);
            DontDestroyOnLoad(this);
        }

        private void Update()
        {
            if (_player == null || DroneManager.Instance == null) return;

            // 1. 매니저의 마스터 각도를 가져옴
            float masterRot = DroneManager.Instance.GetMasterRotation();

            // 2. 내 오프셋 각도를 목표치까지 부드럽게 이동 (자리를 스르륵 잡음)
            _currentOffset = Mathf.LerpAngle(_currentOffset, _targetOffset, Time.deltaTime * 5f);

            // 3. (마스터 각도 + 내 오프셋)으로 최종 위치 결정
            float finalAngle = (masterRot + _currentOffset) * Mathf.Deg2Rad;

            float dist = DroneManager.Instance.orbitDistance;
            Vector3 offset = new Vector3(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle), 0) * dist;

            transform.position = _player.position + offset;
            _animator.Update(Time.deltaTime, droneSprites, animFPS);
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 적 탄환과 충돌 시 (레이어나 태그 활용)
            if (collision.CompareTag("EnemyProjectile"))
            {
                Destroy(collision.gameObject);
            }
        }

        public void AddAbility<T>() where T : Component
        {
            if (GetComponent<T>() == null) gameObject.AddComponent<T>();
        }

        private void OnDestroy()
        {
            if (DroneManager.Instance != null) DroneManager.Instance.UnregisterDrone(this);
        }
    }
}