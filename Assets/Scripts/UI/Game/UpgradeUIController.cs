using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Player;
using Core;

namespace UI
{
    public class UpgradeUIController : MonoBehaviour
    {
        public static UpgradeUIController Instance { get; private set; }

        private GameObject _uiCanvas;
        private GameObject _mainPanel;

        private Transform _cardsContainer;
        private TextMeshProUGUI _creditDisplay;

        private void Awake()
        {
            Instance = this;
        }

        public void Open()
        {
            Time.timeScale = 0f;
            Cursor.visible = true;
            var playerInput = FindAnyObjectByType<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.SwitchCurrentActionMap("UI");
            }

            if (_uiCanvas == null)
            {
                BuildUI();
            }

            _uiCanvas.SetActive(true);
            RefreshUI();
        }

        public void Close()
        {
            if (_uiCanvas != null) _uiCanvas.SetActive(false);
            var playerInput = FindAnyObjectByType<PlayerInput>();
            if (playerInput != null)
            {
                // 3. 다시 게임으로 돌아갈 때 "Player" 조작으로 복구합니다.
                playerInput.SwitchCurrentActionMap("Player");
            }

            Time.timeScale = 1f;
            Cursor.visible = false;
        }

        private void BuildUI()
        {
            // 이벤트 시스템이 씬에 없으면 클릭(상호작용)이 안 되므로 강제 주입
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                var esGo = new GameObject("EventSystem");
                DontDestroyOnLoad(esGo);
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            // 1. 캔버스 생성
            _uiCanvas = new GameObject("SupplyPortCanvas");
            DontDestroyOnLoad(_uiCanvas); // 로비 씬 내내 유지

            var canvas = _uiCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            var scaler = _uiCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            _uiCanvas.AddComponent<GraphicRaycaster>();

            // 2. 어두운 배경 (클릭 시 닫기 기능 생략)
            var bgGo = CreateUIObject("DimBackground", _uiCanvas.transform);
            StretchFull(bgGo);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.05f, 0.05f, 0.08f, 0.9f);

            // 3. 메인 상점 패널
            _mainPanel = CreateUIObject("ShopPanel", _uiCanvas.transform);
            var panelRect = _mainPanel.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(1000, 700);
            panelRect.anchoredPosition = Vector2.zero;

            var panelImg = _mainPanel.AddComponent<Image>();
            panelImg.color = new Color(0.1f, 0.1f, 0.12f, 1f);

            // 4. 상단 타이틀
            var titleTxt = CreateText("Title", _mainPanel.transform, new Vector2(0, 300), new Vector2(900, 60), "보급포트 : 모듈 업그레이드", 40, Color.white, FontStyles.Bold);
            titleTxt.alignment = TextAlignmentOptions.Center;

            // 5. 보유 크레딧 표시
            _creditDisplay = CreateText("CreditDisplay", _mainPanel.transform, new Vector2(0, 240), new Vector2(900, 40), "", 24, new Color(1f, 0.85f, 0f), FontStyles.Normal);
            _creditDisplay.alignment = TextAlignmentOptions.Center;

            // 6. 카드 컨테이너 (수평 정렬)
            var containerGo = CreateUIObject("CardsContainer", _mainPanel.transform);
            var containerRect = containerGo.GetComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(900, 400);
            containerRect.anchoredPosition = new Vector2(0, -30);

            var hl = containerGo.AddComponent<HorizontalLayoutGroup>();
            hl.childAlignment = TextAnchor.MiddleCenter;
            hl.spacing = 30;
            hl.childControlWidth = false;
            hl.childControlHeight = false;

            _cardsContainer = containerGo.transform;

            // 7. 하단 닫기 버튼
            CreateButton("CloseBtn", _mainPanel.transform, new Vector2(0, -300), new Vector2(240, 55), "닫기 (ESC)", Close);
        }

        private void RefreshUI()
        {
            if (UpgradeManager.Instance == null) return;

            // 크레딧 갱신
            int credits = UpgradeManager.Instance.GetCredits();
            if (_creditDisplay != null)
                _creditDisplay.text = $"보유 크레딧 : {credits} CR";

            // 기존 카드 삭제
            if (_cardsContainer != null)
            {
                for (int i = _cardsContainer.childCount - 1; i >= 0; i--)
                {
                    Destroy(_cardsContainer.GetChild(i).gameObject);
                }
            }

            // 업그레이드 카드 생성
            foreach (var data in UpgradeManager.Instance.allUpgrades)
            {
                BuildUpgradeCard(data, _cardsContainer);
            }
        }

        private void BuildUpgradeCard(PlayerUpgradeData data, Transform parent)
        {
            int level = UpgradeManager.Instance.GetLevel(data.id);
            int maxLevel = 3;

            var card = CreateUIObject("Card_" + data.id, parent);
            var cardRect = card.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(260, 360);

            var cardImg = card.AddComponent<Image>();
            cardImg.color = new Color(0.18f, 0.18f, 0.22f, 1f);

            // 카드 제목 (공격력 증가 등)
            var titleTmp = CreateText("Name", card.transform, new Vector2(0, 140), new Vector2(240, 40), data.upgradeName, 22, Color.white, FontStyles.Bold);
            titleTmp.alignment = TextAlignmentOptions.Center;

            // 현재 레벨 표시 (별)
            string stars = "";
            for (int i = 0; i < maxLevel; i++)
                stars += i < level ? "<color=#ffd700>★</color> " : "<color=#555555>★</color> ";

            var levelTmp = CreateText("Level", card.transform, new Vector2(0, 90), new Vector2(240, 40), stars.TrimEnd(), 28, Color.white, FontStyles.Normal);
            levelTmp.alignment = TextAlignmentOptions.Center;

            // 효과 설명
            string statName = data.statType switch
            {
                StatType.Damage => "공격력",
                StatType.AttackSpeed => "공격속도",
                StatType.MoveSpeed => "이동속도",
                _ => "스탯"
            };

            string descText = level < maxLevel
                ? $"다음 레벨 효과:\n<color=#aaffaa>{statName} +{data.statOffsets[level]}</color>"
                : "<color=#ffd700>최고 레벨에 도달했습니다.</color>";

            var descTmp = CreateText("Desc", card.transform, new Vector2(0, 0), new Vector2(220, 100), descText, 18, new Color(0.8f, 0.8f, 0.8f), FontStyles.Normal);
            descTmp.alignment = TextAlignmentOptions.Center;

            // 구매 버튼
            bool isMax = level >= maxLevel;
            int cost = isMax ? 0 : data.costs[level];
            bool canAfford = !isMax && UpgradeManager.Instance.GetCredits() >= cost;

            string btnLabel = isMax ? "MAX" : $"{cost} CR";
            Color btnColor = isMax ? new Color(0.4f, 0.4f, 0.4f) : (canAfford ? new Color(0.2f, 0.6f, 0.2f) : new Color(0.6f, 0.2f, 0.2f));

            var btn = CreateButton("BuyBtn", card.transform, new Vector2(0, -120), new Vector2(180, 50), btnLabel, () =>
            {
                if (!isMax && UpgradeManager.Instance.PurchaseUpgrade(data))
                {
                    RefreshUI(); // 구매 성공 시 UI 전체 즉시 갱신
                }
            });

            // 버튼 색상 변경
            btn.GetComponent<Image>().color = btnColor;
            btn.interactable = !isMax && canAfford;
        }

        #region 스크립트 UI 빌더 유틸리티

        private GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private TextMeshProUGUI CreateText(string name, Transform parent, Vector2 pos, Vector2 size, string text, int fontSize, Color color, FontStyles style)
        {
            var go = CreateUIObject(name, parent);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            var font = Resources.Load<TMP_FontAsset>("Fonts/Galmuri11");
            if (font == null) font = Resources.Load<TMP_FontAsset>("Fonts/Galmuri9");
            if (font != null) tmp.font = font;

            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.raycastTarget = false;
            return tmp;
        }

        private Button CreateButton(string name, Transform parent, Vector2 pos, Vector2 size, string text, UnityEngine.Events.UnityAction action)
        {
            var go = CreateUIObject(name, parent);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.25f, 1f);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(action);

            var txtGo = CreateUIObject("Text", go.transform);
            var txtRt = txtGo.GetComponent<RectTransform>();
            StretchFull(txtGo);

            var tmp = txtGo.AddComponent<TextMeshProUGUI>();
            var font = Resources.Load<TMP_FontAsset>("Fonts/Galmuri11");
            if (font == null) font = Resources.Load<TMP_FontAsset>("Fonts/Galmuri9");
            if (font != null) tmp.font = font;

            tmp.text = text;
            tmp.fontSize = (int)(size.y * 0.4f);
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            return btn;
        }

        private void StretchFull(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        #endregion

        private void Update()
        {
            if (_uiCanvas != null && _uiCanvas.activeSelf)
            {
                if (Keyboard.current != null)
                {
                    // 나가기
                    if (Keyboard.current.escapeKey.wasPressedThisFrame)
                    {
                        Close();
                    }
                }
            }
        }
    }
}