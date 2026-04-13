using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 인게임 HUD 전체를 런타임에 생성하고 관리합니다.
    ///
    /// 구성요소:
    ///   - 좌상단: HP 바 (수치 표시) + 스텔스 슬라이더
    ///   - 우측: 최근 획득 아이템 슬롯 10개 (순서대로 누적)
    ///   - 하단 중앙: 보스 HP 바 (보스 웨이브에서만 활성화)
    ///
    /// 사용방법:
    ///   GameHUD.Instance 로 싱글톤 접근
    ///   ShowBossHP / HideBossHP / SetBossHP 로 보스 HUD 조작
    ///   AddItem / ClearItems 로 아이템 슬롯 조작
    ///   UpgradeStealthBar(newDuration) 로 게이지 폭 확장
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        // ── 싱글톤 ──────────────────────────────────────────────────
        public static GameHUD Instance { get; private set; }

        // ── 외부 레퍼런스 ────────────────────────────────────────────
        [Header("References (auto-found if null)")]
        [SerializeField] private Player.PlayerStats   playerStats;
        [SerializeField] private Player.PlayerStealth playerStealth;

        // ── UI 루트 ──────────────────────────────────────────────────
        private Canvas hudCanvas;

        // HP
        private Slider hpSlider;
        private Text   hpText;

        // 스텔스 슬라이더
        private Slider          stealthSlider;
        private RectTransform   stealthSliderRT;

        /// <summary>슬라이더가 기준 지속 시간(3 s)일 때의 픽셀 폭 (1920×1080 기준)</summary>
        private const float STEALTH_BASE_WIDTH   = 160f;
        /// <summary>최대 지속 시간(5 s)일 때의 픽셀 폭 (1920×1080 기준)</summary>
        private const float STEALTH_MAX_WIDTH    = 260f;
        private const float STEALTH_BASE_SECONDS = 3f;
        private const float STEALTH_MAX_SECONDS  = 5f;

        // 아이템 슬롯 (우측)
        private const int        MAX_ITEM_SLOTS = 10;
        private List<Image>      itemSlotImages = new List<Image>();
        private List<Sprite>     storedItems    = new List<Sprite>();

        // 보스 HP
        private GameObject bossHPPanel;
        private Slider     bossSlider;
        private Text       bossNameText;
        private Text       bossSubtitleText;

        // ────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            if (playerStats   == null) playerStats   = FindAnyObjectByType<Player.PlayerStats>();
            if (playerStealth == null) playerStealth = FindAnyObjectByType<Player.PlayerStealth>();

            BuildCanvas();
            BuildTopLeftHUD();
            BuildRightItemSlots();
            BuildBossHPPanel();
        }

        private void Update()
        {
            RefreshHP();
            RefreshStealth();
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        #region Canvas 생성

        private void BuildCanvas()
        {
            GameObject canvasGo = new GameObject("GameHUD_Canvas");
            hudCanvas = canvasGo.AddComponent<Canvas>();
            hudCanvas.renderMode  = RenderMode.ScreenSpaceOverlay;
            hudCanvas.sortingOrder = 100;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight  = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        #region 좌상단 HUD (HP + 스텔스)

        private void BuildTopLeftHUD()
        {
            // 패널 루트
            GameObject panel = CreateUIObject("TopLeft_Panel", hudCanvas.transform);
            RectTransform panelRT = panel.GetComponent<RectTransform>();
            panelRT.anchorMin        = new Vector2(0, 1);
            panelRT.anchorMax        = new Vector2(0, 1);
            panelRT.pivot            = new Vector2(0, 1);
            panelRT.anchoredPosition = new Vector2(20, -20);
            panelRT.sizeDelta        = new Vector2(340, 70);

            // HP 라벨
            GameObject hpLabel = CreateUIObject("HP_Label", panel.transform);
            SetRect(hpLabel, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
                    new Vector2(0, 0), new Vector2(40, 22));
            Text hpLabelText = hpLabel.AddComponent<Text>();
            StyleLabel(hpLabelText, "HP", 14, TextAnchor.MiddleLeft, new Color(0.8f, 0.8f, 0.8f));

            // HP 수치 텍스트
            GameObject hpNumGo = CreateUIObject("HP_Num", panel.transform);
            SetRect(hpNumGo, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
                    new Vector2(44, 0), new Vector2(200, 22));
            hpText = hpNumGo.AddComponent<Text>();
            StyleLabel(hpText, "100 / 100", 14, TextAnchor.MiddleLeft, Color.white);
            hpText.fontStyle = FontStyle.Bold;

            // HP 슬라이더
            hpSlider = BuildSlider(panel.transform, new Vector2(0, -24), new Vector2(320, 12),
                                   new Color(0.18f, 0.55f, 0.25f), new Color(0.2f, 0.2f, 0.2f));

            // 스텔스 슬라이더
            BuildStealthSlider(panel.transform);
        }

        private void BuildStealthSlider(Transform parent)
        {
            // 슬라이더 배경 + 전경은 BuildSlider 가 만들어 줌
            // 초기 폭은 기본 지속 시간(3 s) 기준
            float initWidth = StealthDurationToWidth(STEALTH_BASE_SECONDS);
            stealthSlider   = BuildSlider(parent, new Vector2(0, -44), new Vector2(initWidth, 10),
                                          new Color(0.35f, 0.72f, 0.95f, 0.9f), new Color(0.2f, 0.2f, 0.2f));
            stealthSliderRT = stealthSlider.GetComponent<RectTransform>();
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        #region 우측 아이템 슬롯

        private void BuildRightItemSlots()
        {
            itemSlotImages.Clear();

            const float slotSize    = 52f;
            const float slotSpacing = 6f;
            float totalH            = MAX_ITEM_SLOTS * (slotSize + slotSpacing);

            GameObject container = CreateUIObject("ItemSlots_Container", hudCanvas.transform);
            RectTransform cRT = container.GetComponent<RectTransform>();
            cRT.anchorMin        = new Vector2(1, 0.5f);
            cRT.anchorMax        = new Vector2(1, 0.5f);
            cRT.pivot            = new Vector2(1, 0.5f);
            cRT.anchoredPosition = new Vector2(-20, 0);
            cRT.sizeDelta        = new Vector2(slotSize + 16f, totalH);

            for (int i = 0; i < MAX_ITEM_SLOTS; i++)
            {
                float yPos = totalH * 0.5f - i * (slotSize + slotSpacing) - slotSize * 0.5f;

                GameObject slotGo = CreateUIObject($"ItemSlot_{i}", container.transform);
                RectTransform sRT = slotGo.GetComponent<RectTransform>();
                sRT.anchorMin        = sRT.anchorMax = new Vector2(0.5f, 0.5f);
                sRT.pivot            = new Vector2(0.5f, 0.5f);
                sRT.anchoredPosition = new Vector2(0, yPos);
                sRT.sizeDelta        = new Vector2(slotSize, slotSize);

                Image bgImg = slotGo.AddComponent<Image>();
                bgImg.color = new Color(0.12f, 0.12f, 0.12f, 0.75f);

                Outline outline = slotGo.AddComponent<Outline>();
                outline.effectColor    = new Color(0.35f, 0.35f, 0.35f, 0.8f);
                outline.effectDistance = new Vector2(1.5f, -1.5f);

                // 슬롯 번호
                GameObject numGo = CreateUIObject($"SlotNum_{i}", slotGo.transform);
                SetRect(numGo, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
                        new Vector2(3, -2), new Vector2(20, 15));
                Text numText = numGo.AddComponent<Text>();
                StyleLabel(numText, (i + 1).ToString(), 10, TextAnchor.UpperLeft, new Color(0.55f, 0.55f, 0.55f));

                // 아이템 아이콘
                GameObject iconGo = CreateUIObject($"ItemIcon_{i}", slotGo.transform);
                RectTransform iRT = iconGo.GetComponent<RectTransform>();
                iRT.anchorMin = Vector2.zero;
                iRT.anchorMax = Vector2.one;
                iRT.offsetMin = new Vector2(6, 6);
                iRT.offsetMax = new Vector2(-6, -6);
                Image iconImg = iconGo.AddComponent<Image>();
                iconImg.color = Color.clear;
                itemSlotImages.Add(iconImg);
            }
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        #region 보스 HP 패널

        private void BuildBossHPPanel()
        {
            bossHPPanel = CreateUIObject("BossHP_Panel", hudCanvas.transform);
            RectTransform pRT = bossHPPanel.GetComponent<RectTransform>();
            pRT.anchorMin        = new Vector2(0.5f, 0);
            pRT.anchorMax        = new Vector2(0.5f, 0);
            pRT.pivot            = new Vector2(0.5f, 0);
            pRT.anchoredPosition = new Vector2(0, 30);
            pRT.sizeDelta        = new Vector2(500, 80);

            bossSubtitleText = CreateText("BossSubtitle", bossHPPanel.transform,
                new Vector2(0, 60), new Vector2(400, 20), "F2 BOSS",
                11, TextAnchor.MiddleCenter, new Color(0.7f, 0.7f, 0.7f));

            bossNameText = CreateText("BossName", bossHPPanel.transform,
                new Vector2(0, 36), new Vector2(460, 32), "보스",
                22, TextAnchor.MiddleCenter, Color.white);
            bossNameText.fontStyle = FontStyle.Bold;

            bossSlider = BuildSlider(bossHPPanel.transform, new Vector2(0, 10), new Vector2(460, 14),
                                     new Color(0.7f, 0.18f, 0.18f), new Color(0.2f, 0.2f, 0.2f));

            CreateText("BossHP_Label", bossHPPanel.transform,
                new Vector2(0, -6), new Vector2(200, 16), "Boss HP",
                10, TextAnchor.MiddleCenter, new Color(0.55f, 0.55f, 0.55f));

            bossHPPanel.SetActive(false);
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        #region Refresh (Update)

        private void RefreshHP()
        {
            if (playerStats == null || hpSlider == null) return;
            float ratio = Mathf.Clamp01(playerStats.currentHealth / playerStats.MaxHealth);
            hpSlider.value = ratio;
            if (hpText != null)
                hpText.text = $"{Mathf.CeilToInt(playerStats.currentHealth)} / {Mathf.CeilToInt(playerStats.MaxHealth)}";
        }

        private void RefreshStealth()
        {
            if (playerStealth == null || stealthSlider == null) return;

            // 슬라이더 값 = 잔여 게이지 비율
            stealthSlider.value = playerStealth.StealthRatio;

            // 색상 피드백
            // 충전 중: 회색, 활성 중: 파랑, 풀충전: 밝은 파랑
            Image fill = stealthSlider.fillRect?.GetComponent<Image>();
            if (fill != null)
            {
                if (playerStealth.IsStealthActive)
                    fill.color = new Color(0.35f, 0.72f, 0.95f, 0.9f);   // 사용 중 – 파랑
                else if (playerStealth.IsRecharging)
                    fill.color = new Color(0.3f, 0.3f, 0.35f, 0.8f);     // 충전 중 – 회색
                else
                    fill.color = new Color(0.55f, 0.85f, 1f, 0.95f);     // 풀충전 – 밝은 파랑
            }

            // maxDuration 이 변경됐을 때 슬라이더 폭 자동 갱신
            if (stealthSliderRT != null)
            {
                float targetWidth = StealthDurationToWidth(playerStealth.MaxDuration);
                if (!Mathf.Approximately(stealthSliderRT.sizeDelta.x, targetWidth))
                    stealthSliderRT.sizeDelta = new Vector2(targetWidth, stealthSliderRT.sizeDelta.y);
            }
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        #region 공개 API

        /// <summary>보스 HP 바를 활성화하고 정보를 설정합니다.</summary>
        public void ShowBossHP(string bossName, string subtitle = "BOSS")
        {
            if (bossHPPanel == null) return;
            bossHPPanel.SetActive(true);
            if (bossNameText     != null) bossNameText.text     = bossName;
            if (bossSubtitleText != null) bossSubtitleText.text = subtitle;
            if (bossSlider       != null) bossSlider.value      = 1f;
        }

        /// <summary>보스 HP 바를 숨깁니다.</summary>
        public void HideBossHP()
        {
            if (bossHPPanel != null) bossHPPanel.SetActive(false);
        }

        /// <summary>보스 체력 비율(0~1)을 업데이트합니다.</summary>
        public void SetBossHP(float ratio)
        {
            if (bossSlider != null) bossSlider.value = Mathf.Clamp01(ratio);
        }

        /// <summary>우측 아이템 슬롯에 아이콘을 추가합니다. (최대 10개)</summary>
        public void AddItem(Sprite icon)
        {
            if (storedItems.Count >= MAX_ITEM_SLOTS) return;
            storedItems.Add(icon);
            RefreshItemSlots();
        }

        /// <summary>아이템 슬롯을 초기화합니다.</summary>
        public void ClearItems()
        {
            storedItems.Clear();
            RefreshItemSlots();
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        #region 내부 유틸

        private void RefreshItemSlots()
        {
            for (int i = 0; i < itemSlotImages.Count; i++)
            {
                if (i < storedItems.Count && storedItems[i] != null)
                {
                    itemSlotImages[i].sprite = storedItems[i];
                    itemSlotImages[i].color  = Color.white;
                }
                else
                {
                    itemSlotImages[i].color = Color.clear;
                }
            }
        }

        /// <summary>지속 시간(초) → 슬라이더 픽셀 폭 변환</summary>
        private static float StealthDurationToWidth(float duration)
        {
            float t = Mathf.InverseLerp(STEALTH_BASE_SECONDS, STEALTH_MAX_SECONDS, duration);
            return Mathf.Lerp(STEALTH_BASE_WIDTH, STEALTH_MAX_WIDTH, t);
        }

        private Slider BuildSlider(Transform parent, Vector2 anchoredPos, Vector2 size,
                                   Color fillColor, Color bgColor)
        {
            GameObject sliderGo = CreateUIObject("Slider", parent);
            Slider slider = sliderGo.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value    = 1f;
            RectTransform sRT = sliderGo.GetComponent<RectTransform>();
            sRT.anchorMin        = sRT.anchorMax = new Vector2(0, 1);
            sRT.pivot            = new Vector2(0, 1);
            sRT.anchoredPosition = anchoredPos;
            sRT.sizeDelta        = size;

            // 배경
            GameObject bgGo = CreateUIObject("Background", sliderGo.transform);
            Image bgImg = bgGo.AddComponent<Image>();
            bgImg.color = bgColor;
            RectTransform bgRT = bgGo.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
            slider.targetGraphic = bgImg;

            // Fill Area
            GameObject fillAreaGo = CreateUIObject("Fill Area", sliderGo.transform);
            RectTransform faRT = fillAreaGo.GetComponent<RectTransform>();
            faRT.anchorMin = Vector2.zero;
            faRT.anchorMax = Vector2.one;
            faRT.offsetMin = faRT.offsetMax = Vector2.zero;

            // Fill
            GameObject fillGo = CreateUIObject("Fill", fillAreaGo.transform);
            Image fillImg = fillGo.AddComponent<Image>();
            fillImg.color = fillColor;
            RectTransform fillRT = fillGo.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;

            slider.fillRect  = fillRT;
            slider.direction = Slider.Direction.LeftToRight;

            return slider;
        }

        private Text CreateText(string name, Transform parent, Vector2 anchoredPos, Vector2 size,
                                string content, int fontSize, TextAnchor anchor, Color color)
        {
            GameObject go = CreateUIObject(name, parent);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot            = new Vector2(0.5f, 0);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = size;
            Text t = go.AddComponent<Text>();
            StyleLabel(t, content, fontSize, anchor, color);
            return t;
        }

        private void StyleLabel(Text t, string content, int fontSize, TextAnchor anchor, Color color)
        {
            t.text      = content;
            t.fontSize  = fontSize;
            t.alignment = anchor;
            t.color     = color;
            t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private void SetRect(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
                             Vector2 anchoredPos, Vector2 sizeDelta)
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin        = anchorMin;
            rt.anchorMax        = anchorMax;
            rt.pivot            = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = sizeDelta;
        }

        private GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        #endregion
    }
}
