using UnityEngine;

namespace Core
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public float smoothTime = 0.15f;
        private Vector3 velocity = Vector3.zero;

        void LateUpdate()
        {
            if (target == null) return;
            Vector3 targetPosition = target.position;
            targetPosition.z = transform.position.z; // Keep offset

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }
}
