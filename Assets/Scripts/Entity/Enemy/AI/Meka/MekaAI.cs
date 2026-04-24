using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.Rendering;

namespace Enemy
{
    public class MekaAI : EnemyAI
    {
        private enum BossState
        {
            Rest,
            Aim,
            LaserFire,
        }

        [Header("Modules")]
        [SerializeField]
        private MekaRailgun railgunModule;

        [SerializeField]
        private MekaSawModule sawModule;

        [SerializeField]
        private MekaAntiAccessLine antiAccessLineModule;

        [Header("Timers")]
        public float restDuration = 3f;
        public float aimDuration = 2f;
        public float laserDuration = 3f; // 3초간 지짐
        public float sawFireInterval = 1.5f;

        private BossState _currentState = BossState.Rest;
        private float _stateTimer;
        private float _sawTimer;

        private Queue<(float time, Vector2 pos)> _playerPosHistory =
            new Queue<(float time, Vector2 pos)>();
        private Vector2 _targetPos;

        protected override void Awake()
        {
            base.Awake();
            rb.bodyType = RigidbodyType2D.Static;
            Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/boss/m3_k4");
            Sprite marker = Resources.Load<Sprite>("Sprites/boss/boss_turret_marker");
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.sprite = sprites[0];
            sr.sortingOrder = 0;

            var railgun = new GameObject("MekaRailgun");
            railgun.transform.SetParent(gameObject.transform);
            railgun.transform.position = gameObject.transform.position;
            railgunModule = railgun.AddComponent<MekaRailgun>();
            var saw = new GameObject("MekaSaw");
            saw.transform.SetParent(gameObject.transform);
            saw.transform.position = gameObject.transform.position;
            sawModule = saw.AddComponent<MekaSawModule>();
            var antiAccessLine = new GameObject("MekaAntiAccessLine");
            antiAccessLine.transform.SetParent(gameObject.transform);
            antiAccessLine.transform.position = gameObject.transform.position;
            antiAccessLineModule = antiAccessLine.AddComponent<MekaAntiAccessLine>();
            railgunModule.Init(sprites[6..14], marker);
            sawModule.Init(sprites[1..5]);
            antiAccessLineModule.Init(sprites[5]);
            playerStat = PlayerStats.LocalPlayer;
            SetState(BossState.Rest);
        }

        protected override void ExecuteBehavior()
        {
            if (playerStat == null || stats.isStunned)
                return;

            // 1. 플레이어 위치 기록 (현재 시간과 좌표 저장)
            _playerPosHistory.Enqueue((Time.time, playerStat.transform.position));

            // 2. 1초보다 오래된 데이터는 꺼내서 타겟으로 삼고 삭제
            while (_playerPosHistory.Count > 0 && _playerPosHistory.Peek().time < Time.time - 0.5f)
            {
                _targetPos = _playerPosHistory.Dequeue().pos;
            }

            _stateTimer -= Time.deltaTime;
            _sawTimer -= Time.deltaTime;
            HandleSawLogic();

            switch (_currentState)
            {
                case BossState.Rest:
                    railgunModule.SetLaser(false);
                    if (_stateTimer <= 0)
                        SetState(BossState.Aim);
                    break;

                case BossState.Aim:
                    // 1초 전 위치(_targetPos)를 조준하고 마커 표시
                    railgunModule.UpdateLaser(_targetPos, stats, false);
                    railgunModule.SetLaser(true, true);
                    if (_stateTimer <= 0)
                        SetState(BossState.LaserFire);
                    break;

                case BossState.LaserFire:
                    // 1초 전 위치를 지지면서 데미지 판정
                    railgunModule.UpdateLaser(_targetPos, stats, true);
                    railgunModule.SetLaser(true, false);
                    if (_stateTimer <= 0)
                        SetState(BossState.Rest);
                    break;
            }
        }

        private void HandleSawLogic()
        {
            // 톱날은 항상 플레이어의 반대 방향을 바라봄
            sawModule.LookAwayFromPlayer(playerStat.transform.position);

            if (_sawTimer <= 0)
            {
                sawModule.FirePlasma(stats);
                _sawTimer = sawFireInterval;
            }
        }

        private void SetState(BossState newState)
        {
            _currentState = newState;
            switch (_currentState)
            {
                case BossState.Rest:
                    _stateTimer = restDuration;
                    railgunModule.SetLaser(false);
                    break;
                case BossState.Aim:
                    _stateTimer = aimDuration;
                    railgunModule.SetLaser(true, true); // 조준 레이저(연함)
                    break;
                case BossState.LaserFire:
                    _stateTimer = laserDuration;
                    railgunModule.SetLaser(true, false); // 공격 레이저(진함)
                    railgunModule.FireRailgunEffect();
                    break;
            }
        }
    }
}
