using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    /// <summary>
    /// Renders a pixel-art crosshair that follows the mouse cursor.
    /// Hides the system cursor while active.
    /// </summary>
    public class Crosshair : MonoBehaviour
    {
        [Header("Crosshair Settings")]
        [SerializeField] private float size = 0.06f;
        [SerializeField] private float thickness = 0.4f; // 선의 굵기 조절 (기존 0.15f -> 0.4f 로 굵게 설정)
        [SerializeField] private Color color = new Color(1f, 0.2f, 0.2f, 1f);
        [SerializeField] private float lineLength = 5f;
        [SerializeField] private float gapSize = 2f;

        private LineRenderer[] lines;  // 4 arms of the crosshair
        private SpriteRenderer centerDot;

        private void Awake()
        {
            Cursor.visible = false;
            BuildCrosshair();
        }

        private void OnDestroy()
        {
            Cursor.visible = true;
        }

        private void LateUpdate()
        {
            if (Mouse.current == null || Camera.main == null) return;

            Vector3 mouseScreen = Mouse.current.position.ReadValue();
            mouseScreen.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mouseScreen);
            worldPos.z = 0f;
            transform.position = worldPos;
        }

        private void BuildCrosshair()
        {
            // Center dot
            GameObject dotObj = new GameObject("Dot");
            dotObj.transform.SetParent(transform, false);
            centerDot = dotObj.AddComponent<SpriteRenderer>();
            centerDot.sprite = MakeDotSprite();
            centerDot.color = color;
            centerDot.sortingOrder = 100;
            dotObj.transform.localScale = Vector3.one * size * 0.5f;

            // 4 arms (Up, Down, Left, Right)
            Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            lines = new LineRenderer[4];
            for (int i = 0; i < 4; i++)
            {
                GameObject lineObj = new GameObject("Line_" + i);
                lineObj.transform.SetParent(transform, false);
                var lr = lineObj.AddComponent<LineRenderer>();
                lr.useWorldSpace = false;
                lr.positionCount = 2;
                lr.startWidth = size * thickness;
                lr.endWidth = size * thickness;
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = color;
                lr.endColor = color;
                lr.sortingOrder = 100;

                float start = gapSize * size;
                float end = (gapSize + lineLength) * size;
                lr.SetPosition(0, dirs[i] * start);
                lr.SetPosition(1, dirs[i] * end);

                lines[i] = lr;
            }
        }

        private Sprite MakeDotSprite()
        {
            Texture2D tex = new Texture2D(4, 4, TextureFormat.ARGB32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 4f);
        }
    }
}
