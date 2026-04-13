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
        public static GameObject BulletPrefab;

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

            // 6. Game HUD
            if (FindAnyObjectByType<UI.GameHUD>() == null)
            {
                GameObject hudGo = new GameObject("GameHUD");
                hudGo.AddComponent<UI.GameHUD>();
            }

            // 7. Update WaveManager
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
            eSr.color = Color.white;
            
#if UNITY_EDITOR
            string dronePath = "Assets/Sprites/enemy/drone.png";
            Sprite droneSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(dronePath);
            if (droneSprite != null)
            {
                eSr.sprite = droneSprite;
            }
            else
            {
                eSr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 4f);
                eSr.color = Color.green;
            }
#else
            eSr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 4f);
            eSr.color = Color.green;
#endif

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
                // We let PlayerSpriteAnimator handle the sprites from its internal sheets.
                // Just ensure color is white and it uses correct sorting.
                sr.color = Color.white;
                sr.sortingOrder = 10;
                
                var animator = player.GetComponent<Player.PlayerSpriteAnimator>();
                if (animator == null) animator = player.AddComponent<Player.PlayerSpriteAnimator>();
                
                // If it's a fresh component, we might want default FPS, 
                // but let's not overwrite the sheets configured in the Inspector or Scene.
                if (animator.fps <= 0) animator.fps = 10f;
            }

            var col = player.GetComponent<BoxCollider2D>();
            if (col == null) 
            {
                col = player.AddComponent<BoxCollider2D>();
            }
            col.size = new Vector2(0.7f, 1.1f); // 상하 충돌 판정 확장

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
                // Create a dedicated object for the weapon visuals and logic, offset from the pivot
                Transform weaponInstance = weaponPivot.Find("WeaponInstance");
                if (weaponInstance == null)
                {
                    GameObject weaponObj = new GameObject("WeaponInstance");
                    weaponObj.transform.SetParent(weaponPivot);
                    // Initial position (WeaponVisuals will manage this thereafter)
                    weaponObj.transform.localPosition = new Vector3(0.2f, 0, 0);
                    weaponInstance = weaponObj.transform;
                }

                var pistol = weaponInstance.GetComponent<WeaponPistol>();
                if (pistol == null)
                {
                    pistol = weaponInstance.gameObject.AddComponent<WeaponPistol>();
                }
                
                // Ensure Visuals component is present
                if (weaponInstance.GetComponent<WeaponVisuals>() == null)
                    weaponInstance.gameObject.AddComponent<WeaponVisuals>();

                // Ensure SpriteRenderer is present and loaded
                var weaponSr = weaponInstance.GetComponent<SpriteRenderer>();
                if (weaponSr == null) weaponSr = weaponInstance.gameObject.AddComponent<SpriteRenderer>();
                weaponSr.sortingOrder = 5;

#if UNITY_EDITOR
                if (weaponSr.sprite == null)
                {
                    string wPath = "Assets/Sprites/weapon/weapons.png";
                    var allWeapons = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(wPath);
                    foreach (var asset in allWeapons) {
                        if (asset is Sprite s && s.name == "weapons_0") {
                            weaponSr.sprite = s;
                            break;
                        }
                    }
                }
#endif

                // Configure or Find FirePoint (Tip of the gun)
                Transform firePoint = weaponInstance.Find("FirePoint");
                if (firePoint == null)
                {
                    GameObject fpObj = new GameObject("FirePoint");
                    fpObj.transform.SetParent(weaponInstance);
                    firePoint = fpObj.transform;
                }
                // Updated position for Right-facing muzzle (relative to handle pivot at 0, 0.4)
                firePoint.localPosition = new Vector3(0.8f, 0f, 0);

                // Reset/Verify Fields (using reflection to ensure they are set even if private)
                var fieldData = typeof(WeaponBase).GetField("weaponData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (fieldData != null && fieldData.GetValue(pistol) == null)
                {
                    WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
                    data.Type = WeaponData.WeaponType.Pistol;
                    data.Damage = 10;
                    data.FireRate = 5f;
                    data.ProjectileSpeed = 15f;
                    data.IsAuto = true;
                    fieldData.SetValue(pistol, data);
                }

                var fieldFP = typeof(WeaponBase).GetField("firePoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (fieldFP != null) fieldFP.SetValue(pistol, firePoint);

                // Ensure Projectile Prefab is configured correctly
                var prefabField = typeof(WeaponBase).GetField("projectilePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                GameObject bulletPrefab = (GameObject)prefabField?.GetValue(pistol);

                if (bulletPrefab == null)
                {
                    bulletPrefab = new GameObject("BulletPrefab");
                    bulletPrefab.SetActive(false);
                    bulletPrefab.AddComponent<Projectile>();
                    prefabField?.SetValue(pistol, bulletPrefab);
                }
                
                // Expose it globally so enemies can use it if they don't have their own
                BulletPrefab = bulletPrefab;

                // Ensure components are correctly configured
                var bRb = bulletPrefab.GetComponent<Rigidbody2D>();
                if (bRb == null) bRb = bulletPrefab.AddComponent<Rigidbody2D>();
                bRb.bodyType = RigidbodyType2D.Dynamic;
                bRb.gravityScale = 0;
                bRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; 
                bRb.interpolation = RigidbodyInterpolation2D.Interpolate;

                var bCol = bulletPrefab.GetComponent<BoxCollider2D>();
                if (bCol == null) bCol = bulletPrefab.AddComponent<BoxCollider2D>();
                bCol.isTrigger = true;
                bCol.size = new Vector2(0.4f, 0.4f); 

                var bSr = bulletPrefab.GetComponent<SpriteRenderer>();
                if (bSr == null) bSr = bulletPrefab.AddComponent<SpriteRenderer>();
                bSr.sortingOrder = 15; 
                
#if UNITY_EDITOR
                string bPath = "Assets/Sprites/bullet/bullet_player.png";
                Sprite bulletSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bPath);
                if (bulletSprite == null)
                {
                    var allAssets = AssetDatabase.LoadAllAssetsAtPath(bPath);
                    foreach (var asset in allAssets) {
                        if (asset is Sprite s) {
                            bulletSprite = s;
                            break;
                        }
                    }
                }

                if (bulletSprite != null)
                {
                    bSr.sprite = bulletSprite;
                    bSr.color = Color.white;
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
                
                // Hook into controller
                var ctrl = player.GetComponent<Player.PlayerController>();
                if (ctrl != null)
                {
                    var field4 = typeof(Player.PlayerController).GetField("currentWeapon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field4 != null) field4.SetValue(ctrl, pistol);
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
