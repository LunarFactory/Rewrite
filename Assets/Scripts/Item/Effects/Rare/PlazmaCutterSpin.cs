using UnityEngine;
using Player;
using Entity;
using Weapon;
using Core;

public class PlazmaCutterSpin : MonoBehaviour
{
    private Projectile proj;
    private void Awake()
    {
        proj = transform.parent.gameObject.GetComponent<Projectile>();
    }
    private void Update()
    {
        transform.Rotate(0, 0, -1440f * Time.deltaTime * Mathf.Pow(1 + proj.CurrentSpeed / 100, 2), Space.Self);
    }
}