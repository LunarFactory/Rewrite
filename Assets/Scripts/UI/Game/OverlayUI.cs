using Core;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class OverlayUI : MonoBehaviour
    {
        private Canvas _canvas;

        // 폰트
        private TMP_FontAsset font;

        protected virtual GameObject BuildOrFindUI()
        {
            GameObject canvasGo = GameObject.Find("Overlay_Canvas");
            if (canvasGo == null)
            {
                canvasGo = new GameObject("Overlay_Canvas");
                canvasGo.transform.SetParent(this.transform);
                _canvas = canvasGo.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.sortingOrder = 999; // 최상단 노출

                var scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasGo.AddComponent<GraphicRaycaster>();
            }
            else
                _canvas = canvasGo.GetComponent<Canvas>();
            return canvasGo;
        }

        public void SetFont(TMP_FontAsset font)
        {
            this.font = font;
        }

        public TextMeshProUGUI CreateText(
            string name,
            Transform parent,
            Vector2 pos,
            Vector2 size,
            string text,
            int fontSize,
            Color color,
            FontStyles style
        )
        {
            var go = CreateUIObject(name, parent);
            CenterRect(go, size);
            go.GetComponent<RectTransform>().anchoredPosition = pos;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = font; // 한글 지원 폰트 적용
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false; // 텍스트가 버튼 클릭을 가로채지 않도록
            return tmp;
        }

        public void CreateButton(
            string name,
            Transform parent,
            Vector2 pos,
            Vector2 size,
            string text,
            UnityEngine.Events.UnityAction onClick
        )
        {
            var go = CreateUIObject(name, parent);
            CenterRect(go, size);
            go.GetComponent<RectTransform>().anchoredPosition = pos;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.25f, 0.9f); // 버튼 배경색

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            // 간단한 하이라이트 트랜지션
            var cb = btn.colors;
            cb.highlightedColor = new Color(0.4f, 0.4f, 0.45f, 1f);
            cb.pressedColor = new Color(0.1f, 0.1f, 0.15f, 1f);
            cb.colorMultiplier = 1f;
            btn.colors = cb;

            CreateText(
                "Text",
                go.transform,
                Vector2.zero,
                size,
                text,
                18,
                Color.white,
                FontStyles.Normal
            );

            // 아웃라인 컴포넌트 추가로 입체감
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.5f);
            outline.effectDistance = new Vector2(2, -2);
        }

        public GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        public void CenterRect(GameObject go, Vector2 size)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
        }

        public void FullStretch(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        protected virtual void ReturnToLobby()
        {
            Time.timeScale = 1f;
            GameManager.Instance.isGameOver = false;

            // 플레이어 상태 초기화 (인벤토리 클리어)
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.items.Clear();
            }

            // DontDestroyOnLoad 된 싱글톤 매니저들 정리
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.MainMenu);
            }

            // 플레이어 즉시 비활성화 (PlayerInteractor.Update()가 이벤트를 발생시키지 못하게)
            if (PlayerStats.LocalPlayer != null)
            {
                PlayerStats.LocalPlayer.gameObject.SetActive(false);
            }

            // GameUIController 즉시 비활성화 후 파괴
            // (SetActive(false)로 모든 콜백을 즉시 정지시킨 뒤 Destroy)
            if (PlayerUI.Instance != null)
            {
                PlayerUI.Instance.gameObject.SetActive(false);
                Destroy(PlayerUI.Instance.gameObject);
            }

            // 기존 HUD 캔버스 정리
            var existingHUD = GameObject.Find("GameHUD_Canvas");
            if (existingHUD != null)
                Destroy(existingHUD);

            // 오버레이 UI 자체도 파괴 (로비에서 새로 생성됨)
            Destroy(gameObject);

            SceneManager.LoadScene("LobbyScene");
        }

        private void GoToMainMenu()
        {
            ReturnToLobby();
        }

        private void QuitToDesktop()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
