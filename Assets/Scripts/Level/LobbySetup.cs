using UnityEngine;
using Player;

namespace Level
{
    public class LobbySetup : MonoBehaviour
    {
        public bool runSetup = true;

        void Start()
        {
            if (!runSetup) return;
            CreateMap();
            SetupPlayer();
            CreateStations();
        }

        void CreateMap()
        {
            GameObject mapRoot = new GameObject("LobbyMap");
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
                fSr.size = new Vector2(20, 20);
            }
#endif
            CreateWall(mapRoot, "WallTop", new Vector2(0, 10.5f), new Vector2(22, 1));
            CreateWall(mapRoot, "WallBottom", new Vector2(0, -10.5f), new Vector2(22, 1));
            CreateWall(mapRoot, "WallLeft", new Vector2(-10.5f, 0), new Vector2(1, 22));
            CreateWall(mapRoot, "WallRight", new Vector2(10.5f, 0), new Vector2(1, 22));
        }

        void CreateWall(GameObject root, string n, Vector2 pos, Vector2 scale)
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
                GameObject topPart = new GameObject("VisualTop");
                topPart.transform.SetParent(w.transform);
                topPart.transform.localPosition = isVertical ? Vector3.zero : new Vector3(0, 0.5f, 0); 
                var srT = topPart.AddComponent<SpriteRenderer>();
                srT.sprite = topSprite;
                srT.drawMode = SpriteDrawMode.Tiled;
                srT.size = isVertical ? new Vector2(1f, scale.y) : new Vector2(scale.x, 1f);
                srT.sortingOrder = isVertical ? -5 : -1; 
                
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
            }
#endif
        }

        void SetupPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
            if (player == null) return;

            if (player.GetComponent<Player.PlayerStats>() == null)
                player.AddComponent<Player.PlayerStats>();

            var sf = player.GetComponent<SpriteRenderer>();
            if (sf != null) { sf.color = Color.white; sf.sortingOrder = 10; }
            var animator = player.GetComponent<Player.PlayerSpriteAnimator>();
            if (animator == null) animator = player.AddComponent<Player.PlayerSpriteAnimator>();
            if (animator.fps <= 0) animator.fps = 10f;

            var col = player.GetComponent<BoxCollider2D>();
            if (col == null) col = player.AddComponent<BoxCollider2D>();

            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) { rb.gravityScale = 0; rb.freezeRotation = true; }

            // Weapon configuring so Armory works
            Transform weaponPivot = player.transform.Find("WeaponPivot");
            if (weaponPivot == null)
            {
                GameObject pivot = new GameObject("WeaponPivot");
                pivot.transform.SetParent(player.transform);
                pivot.transform.localPosition = Vector3.zero;
                weaponPivot = pivot.transform;
            }

            Transform weaponInstance = weaponPivot.Find("WeaponInstance");
            if (weaponInstance == null)
            {
                GameObject weaponObj = new GameObject("WeaponInstance");
                weaponObj.transform.SetParent(weaponPivot);
                weaponObj.transform.localPosition = new Vector3(0.2f, 0, 0);
                weaponInstance = weaponObj.transform;
            }

            var pistol = weaponInstance.GetComponent<Weapons.WeaponPistol>();
            if (pistol == null) pistol = weaponInstance.gameObject.AddComponent<Weapons.WeaponPistol>();
            if (weaponInstance.GetComponent<Weapons.WeaponVisuals>() == null) weaponInstance.gameObject.AddComponent<Weapons.WeaponVisuals>();
            var wSr = weaponInstance.GetComponent<SpriteRenderer>();
            if (wSr == null) wSr = weaponInstance.gameObject.AddComponent<SpriteRenderer>();
            wSr.sortingOrder = 5;

#if UNITY_EDITOR
            if (wSr.sprite == null)
            {
                string wPath = "Assets/Sprites/weapon/weapons.png";
                var allWeapons = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(wPath);
                foreach (var asset in allWeapons) {
                    if (asset is Sprite s && s.name == "weapons_0") {
                        wSr.sprite = s;
                        break;
                    }
                }
            }
#endif

            // Unconditionally assign fresh data to avoid ghost nulls from previous runtime instances
            Weapons.WeaponData data = ScriptableObject.CreateInstance<Weapons.WeaponData>();
            data.Type = Weapons.WeaponData.WeaponType.Pistol;
            data.Damage = 10f;
            data.FireRate = 5f;
            data.ProjectileSpeed = 20f;
            data.IsAuto = true;
            pistol.weaponData = data;

            Transform firePoint = weaponInstance.Find("FirePoint");
            if (firePoint == null)
            {
                GameObject fpObj = new GameObject("FirePoint");
                fpObj.transform.SetParent(weaponInstance);
                firePoint = fpObj.transform;
            }
            firePoint.localPosition = new Vector3(0.8f, 0f, 0);
            pistol.firePoint = firePoint;

            if (pistol.projectilePrefab == null)
            {
                GameObject bulletPrefab = new GameObject("BulletPrefab");
                bulletPrefab.SetActive(false);
                bulletPrefab.AddComponent<Weapons.Projectile>();
                var bRb = bulletPrefab.AddComponent<Rigidbody2D>();
                bRb.bodyType = RigidbodyType2D.Dynamic;
                bRb.gravityScale = 0;
                bRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; 

                var bCol = bulletPrefab.AddComponent<BoxCollider2D>();
                bCol.isTrigger = true;
                bCol.size = new Vector2(0.4f, 0.4f); 

                var bSr = bulletPrefab.AddComponent<SpriteRenderer>();
                bSr.color = Color.white;
                bSr.sortingOrder = 7;
#if UNITY_EDITOR
                var normSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/bullet/bullet_player.png");
                if (normSprite != null) bSr.sprite = normSprite;
#endif
                pistol.projectilePrefab = bulletPrefab;
            }

            // Hook into controller
            var ctrl = player.GetComponent<Player.PlayerController>();
            if (ctrl != null)
            {
                ctrl.currentWeapon = pistol;
                ctrl.weaponPivot = weaponPivot;
            }

            if (player.GetComponent<PlayerInteractor>() == null)
            {
                player.AddComponent<PlayerInteractor>();
            }

            // Auto Camera Follow
            if (Camera.main != null)
            {
                var camFollow = Camera.main.gameObject.GetComponent<Core.CameraFollow>();
                if (camFollow == null) camFollow = Camera.main.gameObject.AddComponent<Core.CameraFollow>();
                camFollow.target = player.transform;
            }
        }

        void CreateStations()
        {
            Sprite playerSprite = null;
#if UNITY_EDITOR
            var all = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/player/player_idle.png");
            foreach (var a in all) if (a is Sprite s) { playerSprite = s; break; }
#endif
            
            // 1. Armory
            CreateStationObj<ArmoryStation>("Armory", new Vector2(-5, 4), playerSprite, Color.cyan);
            // 2. Supply Port
            CreateStationObj<SupplyPortStation>("SupplyPort", new Vector2(5, 4), playerSprite, Color.yellow);
            // 3. Elevator
            CreateStationObj<ElevatorStation>("Elevator", new Vector2(0, 8f), playerSprite, Color.green);
            
            // 4. Dummy
            CreateDummyStation(new Vector2(0, -4), playerSprite);
        }

        void CreateStationObj<T>(string name, Vector2 pos, Sprite sprite, Color color) where T : MonoBehaviour
        {
            GameObject obj = new GameObject("Station_" + name);
            obj.transform.position = pos;
            
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = 5;

            var col = obj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2f, 2f);

            obj.AddComponent<T>();
        }

        void CreateDummyStation(Vector2 pos, Sprite fallbackSprite)
        {
            GameObject dummy = new GameObject("DummyStation");
            dummy.transform.position = pos;
            dummy.tag = "Enemy";

            var sr = dummy.AddComponent<SpriteRenderer>();
            sr.sprite = fallbackSprite;
            sr.color = Color.red; // Red tint to distinguish Dummy
            sr.sortingOrder = 5;
            
            var col = dummy.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            var rb = dummy.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            var enemyBase = dummy.AddComponent<Enemy.DummyEnemy>();
            enemyBase.enabled = false; // Disable AI updates, it acts only as a sandbag
            enemyBase.isInvincible = true; // 더미 피통 무제한
        }
    }
}
