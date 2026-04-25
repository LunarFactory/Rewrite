using Enemy;
using UnityEngine;

public class MekaRailgun : MonoBehaviour
{
    [SerializeField]
    private Sprite[] sprites8Way;

    [SerializeField]
    private LineRenderer laserLine;

    [SerializeField]
    private SpriteRenderer sr;

    private Vector2 _currentDir;
    private SpriteRenderer _markerSR;
    private float _damageAccumulator = 0f; // 소수점 데미지 누적용

    public void Init(Sprite[] sprites, Sprite markerSprite)
    {
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError("Railgun에 전달된 스프라이트가 없습니다!");
            return;
        }
        sprites8Way = sprites;

        if (laserLine == null)
            laserLine = gameObject.AddComponent<LineRenderer>();
        laserLine.material = new Material(Shader.Find("Sprites/Default"));
        laserLine.useWorldSpace = true;

        if (sr == null)
            sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 3;
        sr.sprite = sprites8Way[0]; // 초기 이미지 설정
        GameObject markerGo = new GameObject("TargetMarker");
        markerGo.transform.SetParent(this.transform);
        _markerSR = markerGo.AddComponent<SpriteRenderer>();
        _markerSR.sprite = markerSprite; // MekaAI에서 전달받은 마커 스프라이트
        _markerSR.color = new Color(1, 0, 0, 0.5f);
        _markerSR.sortingOrder = 4;
        _markerSR.enabled = false;
    }

    public void TrackPlayer(Vector2 targetPos)
    {
        _currentDir = (targetPos - (Vector2)transform.position).normalized;
        Update8WaySprite(_currentDir);
    }

    private void Update8WaySprite(Vector2 dir)
    {
        if (sprites8Way == null || sprites8Way.Length == 0)
            return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0)
            angle += 360;

        int index = Mathf.RoundToInt(angle / 45f) % 8;
        sr.sprite = sprites8Way[index];
    }

    public void SetLaser(bool active, bool isAiming = false)
    {
        if (laserLine == null)
            return;
        laserLine.enabled = active;
        _markerSR.enabled = active; // 레이저 켜질 때 마커도 같이 켜짐

        if (active)
        {
            laserLine.startWidth = isAiming ? 0.05f : 0.4f;
            laserLine.endWidth = isAiming ? 0.05f : 0.4f;
            Color col = isAiming ? new Color(0.5f, 1, 0.85f, 0.3f) : Color.aquamarine;
            laserLine.startColor = col;
            laserLine.endColor = col;

            // 조준 중엔 마커가 깜빡이게 하거나 색 조절 가능
            _markerSR.color = isAiming ? new Color(1, 0, 0, 0.4f) : new Color(1, 0, 0, 0.8f);
        }
    }

    // 발사 중 실시간 위치 및 데미지 처리
    public void UpdateLaser(Vector2 targetPos, EnemyStats stats, bool isFireMode)
    {
        if (laserLine == null || !laserLine.enabled)
            return;

        // 1. 방향 및 이미지 갱신
        _currentDir = (targetPos - (Vector2)transform.position).normalized;
        Update8WaySprite(_currentDir);

        // 2. 마커를 정확히 목표 지점에 표시
        if (_markerSR != null)
        {
            _markerSR.enabled = true;
            _markerSR.transform.position = targetPos;
        }

        Vector2 origin = transform.position;

        // 3. 레이저 끝점을 목표 지점(targetPos)으로 고정
        // 이제 레이저가 목표를 넘어서서 뻗어나가지 않습니다.
        Vector2 endPoint = targetPos;

        // 4. 데미지 판정 (Raycast 거리를 목표 지점까지로 제한)
        if (isFireMode)
        {
            // 보스와 목표 지점 사이의 실제 거리 계산
            float distToTarget = Vector2.Distance(origin, targetPos);
            int layerMask = LayerMask.GetMask("Obstacle", "Player");

            // 사거리(distance)를 딱 distToTarget만큼만 줍니다.
            // 이렇게 하면 목표 지점 뒤에 있는 플레이어는 맞지 않습니다.
            RaycastHit2D hit = Physics2D.Raycast(origin, _currentDir, distToTarget, layerMask);

            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                if (hit.collider.TryGetComponent(out Player.PlayerStats pStats))
                {
                    // 소수점 데미지 누적 처리
                    _damageAccumulator += stats.AttackDamage.GetValue() * Time.deltaTime;
                    if (_damageAccumulator >= 1f)
                    {
                        int dmg = Mathf.FloorToInt(_damageAccumulator);
                        pStats.TakeDamage(stats, dmg);
                        _damageAccumulator -= dmg;
                    }
                }
            }
        }

        // 5. 레이저 그리기
        laserLine.SetPosition(0, origin);
        laserLine.SetPosition(1, endPoint);
    }

    public void FireRailgunEffect()
    {
        // 발사 시점 이펙트나 카메라 쉐이크 로직 추가 가능
    }
}
