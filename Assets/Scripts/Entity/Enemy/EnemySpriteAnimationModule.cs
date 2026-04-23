using Unity;
using UnityEngine;

namespace Enemy
{
    [System.Serializable]
    public class EnemySpriteAnimationModule : MonoBehaviour
    {
        private SpriteRenderer _sr;
        private Rigidbody2D _rb;
        private EnemyData _data;

        private float _frameTimer;
        private int _currentFrame;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _rb = GetComponent<Rigidbody2D>();
        }

        public void SetEnemyData(EnemyData data)
        {
            _data = data;
        }

        // direction 인자를 추가하여 8방향 처리가 가능하게 합니다.
        public void UpdateAnimation(float deltaTime, Vector2? moveDirection = null)
        {
            if (_data == null || _sr == null)
                return;

            // 1. 방향 전환 (Flip) - 8방향 이미지가 없을 때만 실행하거나 기본으로 둡니다.
            if (_rb != null && Mathf.Abs(_rb.linearVelocity.x) > 0.01f)
            {
                _sr.flipX = _rb.linearVelocity.x < 0;
            }

            // 모드 A: 8방향 정지 이미지 (포탑 등)
            if (
                moveDirection.HasValue
                && _data.directionalSprites != null
                && _data.directionalSprites.Length >= 8
            )
            {
                UpdateDirectionalSprite(moveDirection.Value);
            }
            // 모드 B: 루프 애니메이션 (드론 등)
            else if (_data.animationFrames != null && _data.animationFrames.Length > 0)
            {
                UpdateLoopAnimation(deltaTime);
            }
        }

        private void UpdateDirectionalSprite(Vector2 dir)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (angle < 0)
                angle += 360f;

            // 0번이 동쪽(0도) 기준 45도씩 분할
            int index = Mathf.RoundToInt(angle / 45f) % 8;
            _sr.sprite = _data.directionalSprites[index];
        }

        private void UpdateLoopAnimation(float deltaTime)
        {
            // FPS가 0이거나 프레임이 없는 경우 DividedByZero 방지
            if (_data.defaultFPS <= 0 || _data.animationFrames.Length == 0)
                return;

            _frameTimer += deltaTime;
            if (_frameTimer >= 1f / _data.defaultFPS)
            {
                _frameTimer = 0f;
                _currentFrame = (_currentFrame + 1) % _data.animationFrames.Length;
                _sr.sprite = _data.animationFrames[_currentFrame];
            }
        }
    }
}
