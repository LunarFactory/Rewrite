using UnityEngine;

namespace Core
{
    public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance; // 외부에서 접근하기 쉽게 싱글톤화

    public Transform target;
    public float smoothTime = 0.15f;
    private Vector3 velocity = Vector3.zero;

    private void Awake()
    {
        Instance = this; // 씬에 하나만 존재하므로 인스턴스 할당
    }

    // [핵심] 이제 매 프레임 찾지 않습니다.
    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position;
        targetPosition.z = transform.position.z;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    // 타겟을 외부에서 설정해주는 함수
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
}