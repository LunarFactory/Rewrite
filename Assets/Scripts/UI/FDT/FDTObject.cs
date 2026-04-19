using UnityEngine;
using TMPro;

public class FDTObject : MonoBehaviour
{
    private TextMeshPro _textMesh;
    private Vector2 _velocity;
    private float _lifeTime;
    private float _maxLifeTime;
    private Color _initialColor;

    // 중력 강도를 인스펙터에서 조절할 수 있게 밖으로 뺐습니다.
    [SerializeField] private float gravity = 15f;

    private void Awake()
    {
        _textMesh = GetComponent<TextMeshPro>();
    }

    public void Initialize(int damage, float duration, Color color)
    {
        _textMesh.text = damage.ToString();
        _initialColor = color;
        _textMesh.color = color;

        // 만약 duration이 0으로 들어오면 1초로 강제 설정 (방어 코드)
        _maxLifeTime = duration <= 0 ? 1f : duration;
        _lifeTime = 0f;

        // 초기 속도 설정
        float angle = Random.Range(-10f, 10f);
        float speed = Random.Range(3f, 5f);
        _velocity = Quaternion.Euler(0, 0, angle) * Vector2.up * speed;
    }

    private void Update()
    {
        _lifeTime += Time.deltaTime;

        // 1. 중력 및 이동
        _velocity.y -= gravity * Time.deltaTime;
        // 공기 저항: 좌우 이동 속도를 서서히 줄임
        _velocity.x = Mathf.Lerp(_velocity.x, 0, Time.deltaTime * 4f);

        transform.Translate(_velocity * Time.deltaTime, Space.World);

        // 2. 투명도 조절 (0~1 사이의 비율 계산)
        float progress = _lifeTime / _maxLifeTime;

        // 처음에는 선명하다가 절반이 지나면 급격히 사라짐
        float alpha = Mathf.Lerp(1f, 0f, progress);
        float scale = Mathf.Lerp(1.2f, 0.5f, progress); // 커졌다가 작아지면서 소멸
        transform.localScale = new Vector3(scale, scale, 1);
        _textMesh.color = new Color(_initialColor.r, _initialColor.g, _initialColor.b, alpha);

        // 3. 파괴 체크 (가장 중요!)
        if (_lifeTime >= _maxLifeTime)
        {
            Destroy(gameObject);
        }
    }
}