using UnityEngine;

namespace Level
{
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("Outline Settings")]
        private static Material _outlineMaterial;
        public Color outlineColor = Color.white;
        private SpriteRenderer[] outlineSRs = new SpriteRenderer[4];
        private bool isInitialized = false;

        public void ShowOutline(bool show)
        {
            if (!isInitialized) InitOutline();

            foreach (var sr in outlineSRs)
            {
                if (sr != null) sr.enabled = show;
            }
        }

        private void InitOutline()
        {
            SpriteRenderer mainSR = GetComponent<SpriteRenderer>();
            if (mainSR == null) return;

            if (_outlineMaterial == null)
            {
                // 1. 시도: 이름으로 찾기
                Shader shader = Shader.Find("Custom/FlatColor");
                _outlineMaterial = new Material(shader);
            }
            if (GetComponent<UnityEngine.Rendering.SortingGroup>() == null)
            {
                var group = gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>();
                group.sortingLayerID = mainSR.sortingLayerID;
                group.sortingOrder = 10;
            }

            // 16 PPU 기준 1픽셀 이동 값 (1 / 16 = 0.0625)
            float p = 1f / mainSR.sprite.pixelsPerUnit;
            float zOffset = 0.01f;
            Vector3[] offsets = {
                new Vector3(0, p, zOffset),
                new Vector3(0, -p, zOffset),
                new Vector3(-p, 0, zOffset),
                new Vector3(p, 0, zOffset)
            };

            for (int i = 0; i < 4; i++)
            {
                GameObject go = new GameObject("Outline_Ghost");
                go.transform.SetParent(transform);
                go.transform.localPosition = offsets[i];
                go.transform.localScale = Vector3.one;

                // [중요] 하이어라키 창에서 이 자식 오브젝트를 숨깁니다.
                go.hideFlags = HideFlags.HideInHierarchy;

                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sortingLayerID = mainSR.sortingLayerID;
                sr.sprite = mainSR.sprite;

                if (_outlineMaterial != null)
                {
                    _outlineMaterial.SetColor("_Color", Color.white);
                    sr.material = _outlineMaterial;
                }

                sr.sortingOrder = -1;
                sr.spriteSortPoint = mainSR.spriteSortPoint;
                sr.enabled = false;

                outlineSRs[i] = sr;
            }
            isInitialized = true;
        }

        // [자식용] 상호작용 로직은 자식마다 다르므로 '추상 메서드'로 만듭니다.
        public abstract string GetInteractPrompt();
        public abstract void OnInteract(GameObject interactEntity);
    }
}
