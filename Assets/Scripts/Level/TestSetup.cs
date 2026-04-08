using UnityEngine;
using Enemy;
using Weapons;
using Core;

namespace Level
{
    public class TestSetup : MonoBehaviour
    {
        public bool runSetup = true;
        
        public static GameObject EnemyPrefab;

        void Start()
        {
            if (!runSetup) return;

            // 1. Camera Follow
            if (Camera.main != null && Camera.main.GetComponent<CameraFollow>() == null)
            {
                var follow = Camera.main.gameObject.AddComponent<CameraFollow>();
                GameObject player = GameObject.Find("Player");
                if (player != null) follow.target = player.transform;
            }

            // 2. Map (Floor and Walls)
            CreateMap();

            // 3. Projectile & Enemy Prefab creation
            CreatePrefabs();

            // 4. Player setup
            SetupPlayer();
            
            // 5. Update WaveManager
            WaveManager waveManager = FindAnyObjectByType<WaveManager>();
            if (waveManager != null)
            {
                // Reflection to inject enemy prefab into WaveManager (since it's not exposed yet)
                var field = typeof(WaveManager).GetField("dummyEnemyPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field == null)
                {
                    // If we didn't add the field, we will modify WaveManager directly later.
                }
            }
        }

        void CreateMap()
        {
            GameObject mapRoot = new GameObject("MapRoot");
            
            // Floor
            GameObject floor = new GameObject("Floor");
            floor.transform.SetParent(mapRoot.transform);
            floor.transform.localScale = new Vector3(30, 30, 1);
            var fSr = floor.AddComponent<SpriteRenderer>();
            fSr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 4f);
            fSr.color = new Color(0.2f, 0.3f, 0.2f);
            fSr.sortingOrder = -10;

            // Walls
            CreateWall(mapRoot, "WallTop", new Vector2(0, 15.5f), new Vector2(32, 1), Color.black);
            CreateWall(mapRoot, "WallBottom", new Vector2(0, -15.5f), new Vector2(32, 1), Color.black);
            CreateWall(mapRoot, "WallLeft", new Vector2(-15.5f, 0), new Vector2(1, 32), Color.black);
            CreateWall(mapRoot, "WallRight", new Vector2(15.5f, 0), new Vector2(1, 32), Color.black);
        }

        void CreateWall(GameObject root, string n, Vector2 pos, Vector2 scale, Color color)
        {
            GameObject w = new GameObject(n);
            w.transform.SetParent(root.transform);
            w.transform.position = pos;
            w.transform.localScale = scale;
            w.tag = "Obstacle";

            var sr = w.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 4f);
            sr.color = color;
            sr.sortingOrder = -5;

            var col = w.AddComponent<BoxCollider2D>();
        }

        void CreatePrefabs()
        {
            // Enemy Prefab
            EnemyPrefab = new GameObject("DummyEnemyPrefab");
            EnemyPrefab.SetActive(false);
            EnemyPrefab.tag = "Enemy";

            var eSr = EnemyPrefab.AddComponent<SpriteRenderer>();
            eSr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 4f);
            eSr.color = Color.green;

            var eCol = EnemyPrefab.AddComponent<BoxCollider2D>();
            eCol.size = Vector2.one;

            var eRb = EnemyPrefab.AddComponent<Rigidbody2D>();
            eRb.gravityScale = 0;
            eRb.freezeRotation = true;

            EnemyPrefab.AddComponent<DummyEnemy>();
        }

        void SetupPlayer()
        {
            GameObject player = GameObject.Find("Player");
            if (player == null) return;

            player.tag = "Player";
            
            // Stats
            if (player.GetComponent<Player.PlayerStats>() == null)
            {
                player.AddComponent<Player.PlayerStats>();
            }

            // Visuals
            var sr = player.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite == null)
            {
                sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 4f);
                sr.color = Color.blue;
            }

            var col = player.GetComponent<BoxCollider2D>();
            if (col == null) 
            {
                col = player.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1f, 1f);
            }

            // RB setup
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0;
                rb.freezeRotation = true;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

            // Weapon Setup
            Transform weaponPivot = player.transform.Find("WeaponPivot");
            if (weaponPivot != null)
            {
                if (weaponPivot.GetComponent<WeaponPistol>() == null)
                {
                    var pistol = weaponPivot.gameObject.AddComponent<WeaponPistol>();
                    
                    // Create runtime WeaponData
                    WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
                    data.Type = WeaponData.WeaponType.Pistol;
                    data.Damage = 10;
                    data.FireRate = 5f;
                    data.ProjectileSpeed = 15f;
                    data.IsAuto = true;

                    var field = typeof(WeaponBase).GetField("weaponData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null) field.SetValue(pistol, data);

                    // Create FirePoint
                    GameObject firePoint = new GameObject("FirePoint");
                    firePoint.transform.SetParent(weaponPivot);
                    firePoint.transform.localPosition = new Vector3(0.5f, 0, 0);

                    var field2 = typeof(WeaponBase).GetField("firePoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field2 != null) field2.SetValue(pistol, firePoint.transform);

                    // Create Projectile Prefab
                    GameObject bullet = new GameObject("BulletPrefab");
                    bullet.SetActive(false); 
                    var bSr = bullet.AddComponent<SpriteRenderer>();
                    bSr.sprite = CreateCircleSprite();
                    bSr.color = Color.red;
                    bullet.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                    
                    var bCol = bullet.AddComponent<BoxCollider2D>();
                    bCol.isTrigger = true;

                    bullet.AddComponent<Rigidbody2D>().gravityScale = 0;
                    bullet.AddComponent<Projectile>();

                    var field3 = typeof(WeaponBase).GetField("projectilePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field3 != null) field3.SetValue(pistol, bullet);
                    
                    // Hook into controller
                    var ctrl = player.GetComponent<Player.PlayerController>();
                    var field4 = typeof(Player.PlayerController).GetField("currentWeapon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field4 != null) field4.SetValue(ctrl, pistol);
                }
            }
        }

        Sprite CreateCircleSprite()
        {
            int radius = 16;
            int diam = radius * 2;
            Texture2D tex = new Texture2D(diam, diam, TextureFormat.ARGB32, false);
            Color[] colorData = new Color[diam * diam];
            float rSquared = radius * radius;
            
            for (int x = 0; x < diam; x++) {
                for (int y = 0; y < diam; y++) {
                    float dx = x - radius;
                    float dy = y - radius;
                    if (dx * dx + dy * dy < rSquared) 
                        colorData[x + y * diam] = Color.white;
                    else 
                        colorData[x + y * diam] = Color.clear;
                }
            }
            tex.SetPixels(colorData);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, diam, diam), new Vector2(0.5f, 0.5f), (float)diam);
        }
    }
}
