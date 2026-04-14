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
            //CreateMap();

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
            floor.transform.localScale = Vector3.one;
            var fSr = floor.AddComponent<SpriteRenderer>();
            fSr.color = Color.white;
            fSr.sortingOrder = -10;

#if UNITY_EDITOR
            Sprite flrSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/tileset/floor.png");
            if (flrSprite != null) {
                fSr.sprite = flrSprite;
                fSr.drawMode = SpriteDrawMode.Tiled;
                fSr.size = new Vector2(30, 30);
            } else {
                fSr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 4f);
                fSr.color = new Color(0.2f, 0.3f, 0.2f);
                floor.transform.localScale = new Vector3(30, 30, 1);
            }
#else
            fSr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 4f);
            fSr.color = new Color(0.2f, 0.3f, 0.2f);
            floor.transform.localScale = new Vector3(30, 30, 1);
#endif

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
            w.transform.localScale = Vector3.one;
            w.tag = "Obstacle";

            var col = w.AddComponent<BoxCollider2D>();
            col.size = scale;

#if UNITY_EDITOR
            string wPath = "Assets/Sprites/tileset/wall.png";
            var allSprites = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(wPath);
            Sprite topSprite = null;
            Sprite botSprite = null;
            foreach (var asset in allSprites) {
                if (asset is Sprite s) {
                    if (s.name == "wall_5") topSprite = s;
                    if (s.name == "wall_13") botSprite = s;
                }
            }

            if (topSprite != null && botSprite != null) {
                bool isVertical = scale.y > scale.x;

                // Visual Top (Upper wall segment)
                GameObject topPart = new GameObject("VisualTop");
                topPart.transform.SetParent(w.transform);
                topPart.transform.localPosition = isVertical ? Vector3.zero : new Vector3(0, 0.5f, 0); 
                var srT = topPart.AddComponent<SpriteRenderer>();
                srT.sprite = topSprite;
                srT.drawMode = SpriteDrawMode.Tiled;
                srT.size = isVertical ? new Vector2(1f, scale.y) : new Vector2(scale.x, 1f);
                srT.sortingOrder = isVertical ? -5 : -1; // Top parts slightly higher sorting

                // Visual Bottom (Lower wall segment)
                if (!isVertical) {
                    GameObject botPart = new GameObject("VisualBottom");
                    botPart.transform.SetParent(w.transform);
                    botPart.transform.localPosition = new Vector3(0, -0.5f, 0); 
                    var srB = botPart.AddComponent<SpriteRenderer>();
                    srB.sprite = botSprite;
                    srB.drawMode = SpriteDrawMode.Tiled;
                    srB.size = new Vector2(scale.x, 1f);
                    srB.sortingOrder = -2;
                }
            } else {
                var sr = w.AddComponent<SpriteRenderer>();
                sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 4f);
                sr.color = color;
                sr.sortingOrder = -5;
                w.transform.localScale = scale;
                col.size = Vector2.one;
            }
#else
            var sr = w.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 4f);
            sr.color = color;
            sr.sortingOrder = -5;
            w.transform.localScale = scale;
            col.size = Vector2.one;
#endif
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
            var allDrones = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(dronePath);
            System.Collections.Generic.List<Sprite> droneFrames = new System.Collections.Generic.List<Sprite>();
            foreach (var asset in allDrones) {
                if (asset is Sprite s) droneFrames.Add(s);
            }

            if (droneFrames.Count > 0)
            {
                eSr.sprite = droneFrames[0];
                var anim = EnemyPrefab.AddComponent<Enemy.EnemySpriteAnimator>();
                anim.frames = droneFrames.ToArray();
                anim.fps = 10f;
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
            //col.size = new Vector2(0.7f, 1.1f); // 상하 충돌 판정 확장

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
            if (weaponPivot == null)
            {
                GameObject pivot = new GameObject("WeaponPivot");
                pivot.transform.SetParent(player.transform);
                pivot.transform.localPosition = Vector3.zero;
                weaponPivot = pivot.transform;
            }

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

            Weapons.WeaponData.WeaponType targetType = Weapons.WeaponData.WeaponType.Pistol;
            if (Core.RunManager.Instance != null)
            {
                targetType = Core.RunManager.Instance.CurrentWeaponType;
            }

            Weapons.WeaponBase oldPistol = weaponInstance.GetComponent<Weapons.WeaponBase>();
            if (oldPistol != null) DestroyImmediate(oldPistol); // Forcibly clean up any mismatched previous components

            Weapons.WeaponBase pistol = null;
            switch (targetType)
            {
                case Weapons.WeaponData.WeaponType.Pistol: pistol = weaponInstance.gameObject.AddComponent<Weapons.WeaponPistol>(); break;
                case Weapons.WeaponData.WeaponType.AssaultRifle: pistol = weaponInstance.gameObject.AddComponent<Weapons.WeaponAssaultRifle>(); break;
                case Weapons.WeaponData.WeaponType.Shotgun: pistol = weaponInstance.gameObject.AddComponent<Weapons.WeaponShotgun>(); break;
                case Weapons.WeaponData.WeaponType.Sniper: pistol = weaponInstance.gameObject.AddComponent<Weapons.WeaponSniper>(); break;
            }
            
            if (weaponInstance.GetComponent<Weapons.WeaponVisuals>() == null)
                weaponInstance.gameObject.AddComponent<Weapons.WeaponVisuals>();

            var weaponSr = weaponInstance.GetComponent<SpriteRenderer>();
            if (weaponSr == null) weaponSr = weaponInstance.gameObject.AddComponent<SpriteRenderer>();
            weaponSr.sortingOrder = 5;

#if UNITY_EDITOR
            // Unconditionally update visual instead of checking if null
            string wPath = "Assets/Sprites/weapon/weapons.png";
            var allWeapons = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(wPath);
            int spriteIdx = (int)targetType;
            foreach (var asset in allWeapons) {
                if (asset is Sprite s && s.name == "weapons_" + spriteIdx) {
                    weaponSr.sprite = s;
                    break;
                }
            }
#endif

            Transform firePoint = weaponInstance.Find("FirePoint");
            if (firePoint == null)
            {
                GameObject fpObj = new GameObject("FirePoint");
                fpObj.transform.SetParent(weaponInstance);
                firePoint = fpObj.transform;
            }
            // Updated position for Right-facing muzzle (relative to handle pivot at 0, 0.4)
            firePoint.localPosition = new Vector3(0.8f, 0f, 0);

            // Unconditionally assign fresh data to avoid ghost nulls from previous runtime instances
            Weapons.WeaponData data = ScriptableObject.CreateInstance<Weapons.WeaponData>();
            data.Type = targetType;
            
            switch (targetType)
            {
                case Weapons.WeaponData.WeaponType.Pistol:
                    data.Damage = 10f; data.FireRate = 5f; data.ProjectileSpeed = 20f; data.IsAuto = true;
                    data.NumberOfPellets = 1; data.SpreadAngle = 0f; data.PierceCount = 0; break;
                case Weapons.WeaponData.WeaponType.AssaultRifle:
                    data.Damage = 5f; data.FireRate = 12f; data.ProjectileSpeed = 25f; data.IsAuto = true;
                    data.NumberOfPellets = 1; data.SpreadAngle = 5f; data.PierceCount = 0; break;
                case Weapons.WeaponData.WeaponType.Sniper:
                    data.Damage = 40f; data.FireRate = 1f; data.ProjectileSpeed = 40f; data.IsAuto = false;
                    data.NumberOfPellets = 1; data.SpreadAngle = 0f; data.PierceCount = 3; break;
                case Weapons.WeaponData.WeaponType.Shotgun:
                    data.Damage = 8f; data.FireRate = 1.5f; data.ProjectileSpeed = 30f; data.IsAuto = false;
                    data.NumberOfPellets = 5; data.SpreadAngle = 30f; data.PierceCount = 0; break;
            }
            
            pistol.weaponData = data;

                pistol.firePoint = firePoint;

                // Ensure Projectile Prefab is configured correctly
                GameObject bulletPrefab = pistol.projectilePrefab;

                if (bulletPrefab == null)
                {
                    bulletPrefab = new GameObject("BulletPrefab");
                    bulletPrefab.SetActive(false);
                    bulletPrefab.AddComponent<Weapons.Projectile>();
                    pistol.projectilePrefab = bulletPrefab;
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
                    ctrl.currentWeapon = pistol;
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
