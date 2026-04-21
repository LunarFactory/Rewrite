using UnityEngine;
using Entity;
using Player;
using Enemy;

[CreateAssetMenu(fileName = "Effect_Laser", menuName = "Effects/Stack/Laser")]
public class LaserEffect : StatusEffectData
{
    [Header("Result Effect")]
    public float damagePercent = 2.5f;
    public int maxStack = 3;
    // EMP만의 특화 수치가 필요하다면 여기에 추가 (예: 기절 시 추가 데미지 등)
    public override void OnStackFull(BuffManager manager, EntityStats source)
    {
        EnemyStats _entity = manager.GetComponent<EnemyStats>();
        if (source is PlayerStats player)
        {
            _entity.TakeDamage(player, Mathf.RoundToInt(player.GetWeaponBaseAttackDamage() * damagePercent), Color.magenta);
        }
        else _entity.TakeDamage(source, Mathf.RoundToInt(source.AttackDamage.GetValue() * damagePercent), Color.magenta);
        CreateLaserVisual(source.transform.position, _entity.transform.position);
    }
    private void CreateLaserVisual(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("Laser_Line");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        // 머티리얼 및 색상 설정
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.red; // 신경망 느낌의 민트색
        lr.endColor = Color.white;
        lr.startWidth = 0.08f;
        lr.endWidth = 0.08f;
        lr.sortingOrder = 100; // 다른 오브젝트보다 앞에 보이게

        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        // 0.15초 뒤에 선 제거 (잔상 효과)
        Destroy(lineObj, 0.15f);
    }
    public override int GetMaxStack()
    {
        return maxStack;
    }
}