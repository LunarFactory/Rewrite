using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
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

            // 5. Crosshair
            if (FindAnyObjectByType<Player.Crosshair>() == null)
            {
                GameObject ch = new GameObject("Crosshair");
                ch.AddComponent<Player.Crosshair>();
            }

            // 6. Update WaveManager
            WaveManager waveManager = FindAnyObjectByType<WaveManager>();
            if (waveManager != null)
            {
                var field = typeof(WaveManager).GetField("dummyEnemyPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field == null) { }
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
            if (sr != null)
            {
                sr.color = Color.white; // Ensure it's not blue anymore
#if UNITY_EDITOR
                Sprite idle1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/player_idle1.png");
                Sprite idle2 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/player_idle2.png");
                Sprite run1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/player_run1.png");
                Sprite run2 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/player_run2.png");
                Sprite run3 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/player_run3.png");
                Sprite run4 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/player_run4.png");
                Sprite runU1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/player_run_upside1.png");
                Sprite runU2 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/player_run_upside2.png");
                Sprite runU3 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/player_run_upside3.png");
                Sprite runU4 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/player_run_upside4.png");

                if (idle1 != null) sr.sprite = idle1;

                var animator = player.GetComponent<Player.PlayerSpriteAnimator>();
                if (animator == null) animator = player.AddComponent<Player.PlayerSpriteAnimator>();
                
                animator.idleSprites = new Sprite[] { idle1, idle2 };
                animator.runSprites = new Sprite[] { run1, run2, run3, run4 };
                animator.runUpsideSprites = new Sprite[] { runU1, runU2, runU3, runU4 };
                animator.fps = 10f;
#else
                if (sr.sprite == null)
                {
                    sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 4f);
                    sr.color = Color.blue;
                }
#endif
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
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;
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

                    // Create FirePoint at player center (origin)
                    GameObject firePoint = new GameObject("FirePoint");
                    firePoint.transform.SetParent(weaponPivot);
                    firePoint.transform.localPosition = Vector3.zero;

                    var field2 = typeof(WeaponBase).GetField("firePoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field2 != null) field2.SetValue(pistol, firePoint.transform);

                    // Create Projectile Prefab
                    GameObject bullet = new GameObject("BulletPrefab");
                    bullet.SetActive(false);
                    var bSr = bullet.AddComponent<SpriteRenderer>();

                    // Load bullet_normal.png sprite
#if UNITY_EDITOR
                    Sprite bulletSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/bullet_normal.png");
                    if (bulletSprite != null)
                    {
                        // Set PPU and Point filter mode at runtime import
                        UnityEditor.TextureImporter bImporter = UnityEditor.AssetImporter.GetAtPath("Assets/Sprites/bullet_normal.png") as UnityEditor.TextureImporter;
                        if (bImporter != null)
                        {
                            bool bChanged = false;
                            if (bImporter.spritePixelsPerUnit != 16f) { bImporter.spritePixelsPerUnit = 16f; bChanged = true; }
                            if (bImporter.filterMode != FilterMode.Point) { bImporter.filterMode = FilterMode.Point; bChanged = true; }
                            if (bImporter.textureCompression != UnityEditor.TextureImporterCompression.Uncompressed) { bImporter.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed; bChanged = true; }
                            if (bImporter.spriteImportMode != UnityEditor.SpriteImportMode.Single) { bImporter.spriteImportMode = UnityEditor.SpriteImportMode.Single; bChanged = true; }
                            UnityEditor.TextureImporterSettings bTexSettings = new UnityEditor.TextureImporterSettings();
                            bImporter.ReadTextureSettings(bTexSettings);
                            if (bTexSettings.spriteAlignment != (int)SpriteAlignment.Center) { bTexSettings.spriteAlignment = (int)SpriteAlignment.Center; bChanged = true; }
                            if (bChanged) { bImporter.SetTextureSettings(bTexSettings); bImporter.SaveAndReimport(); bulletSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/bullet_normal.png"); }
                        }
                        bSr.sprite = bulletSprite;
                    }
                    else
                    {
                        bSr.sprite = CreateCircleSprite();
                        bSr.color = Color.red;
                    }
#else
                    bSr.sprite = CreateCircleSprite();
                    bSr.color = Color.red;
#endif

                    var bCol = bullet.AddComponent<BoxCollider2D>();
                    bCol.isTrigger = true;
                    var bRb = bullet.AddComponent<Rigidbody2D>();
                    bRb.gravityScale = 0;
                    bRb.interpolation = RigidbodyInterpolation2D.Interpolate;
                    bullet.AddComponent<Projectile>();

                    var field3 = typeof(WeaponBase).GetField("projectilePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field3 != null) field3.SetValue(pistol, bullet);
                    
                    // Hook into controller
                    var ctrl = player.GetComponent<Player.PlayerController>();
                    var field4 = typeof(Player.PlayerController).GetField("currentWeapon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field4 != null) field4.SetValue(ctrl, pistol);

                    // Assign weaponPivot (public field) so HandleAiming has a pivot reference
                    ctrl.weaponPivot = weaponPivot;
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
