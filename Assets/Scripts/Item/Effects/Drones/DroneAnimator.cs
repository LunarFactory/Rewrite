using UnityEngine;

namespace Drone
{
    [System.Serializable]
    public class DroneAnimator
    {
        private SpriteRenderer _sr;
        private float _frameTimer;
        private int _currentFrame;

        public void Initialize(SpriteRenderer sr)
        {
            _sr = sr;
        }

        public void Update(float deltaTime, Sprite[] frames, float fps)
        {
            if (frames == null || frames.Length == 0 || _sr == null) return;

            // FPS가 0이면 멈춤 상태
            if (fps <= 0) return;

            _frameTimer += deltaTime;

            // 프레임 교체 타이밍 계산
            if (_frameTimer >= 1f / fps)
            {
                _frameTimer = 0f;
                // 다음 프레임으로 넘어가고, 마지막 프레임이면 다시 0으로
                _currentFrame = (_currentFrame + 1) % frames.Length;
                _sr.sprite = frames[_currentFrame];
            }
        }
    }
}