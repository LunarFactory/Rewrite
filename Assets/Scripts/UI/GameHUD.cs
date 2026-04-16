using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GameHUD : MonoBehaviour
    {
        public static GameHUD Instance { get; private set; }

        [Header("References (auto-found if null)")]
        [SerializeField] private Player.PlayerStats   playerStats;
        [SerializeField] private Player.PlayerStealth playerStealth;

        private Canvas hudCanvas;
        private Slider hpSlider;
        private Text   hpText;
        private Slider          stealthSlider;
        private RectTransform   stealthSliderRT;
        private Text boltText;

        private const float STEALTH_BASE_WIDTH   = 160f;
        private const float STEALTH_MAX_WIDTH    = 260f;
        private const float STEALTH_BASE_SECONDS = 3f;
        private const float STEALTH_MAX_SECONDS  = 5f;

        private const int        MAX_ITEM_SLOTS = 10;
        private List<Image>      itemSlotImages = new List<Image>();
        private List<Sprite>     storedItems    = new List<Sprite>();

        private GameObject bossHPPanel;
        private Slider     bossSlider;
        private Text       bossNameText;
        private Text       bossSubtitleText;

        private GameObject wavePromptPanel;
        private Text       wavePromptText;

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
            RefreshBolts();
        }

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

        private void BuildTopLeftHUD()
        {
            GameObject panel = CreateUIObject("TopLeft_Panel", hudCanvas.transform);
            RectTransform panelRT = panel.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0, 1);
            panelRT.anchorMax = new Vector2(0, 1);
            panelRT.pivot = new Vector2(0, 1);
            panelRT.anchoredPosition = new Vector2(20, -20);
            panelRT.sizeDelta = new Vector2(340, 70);

            GameObject hpLabel = CreateUIObject("HP_Label", panel.transform);
            SetRect(hpLabel, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 0), new Vector2(40, 22));
            Text hpLabelText = hpLabel.AddComponent<Text>();
            StyleLabel(hpLabelText, "HP", 14, TextAnchor.MiddleLeft, new Color(0.8f, 0.8f, 0.8f));

            GameObject hpNumGo = CreateUIObject("HP_Num", panel.transform);
            SetRect(hpNumGo, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(44, 0), new Vector2(200, 22));
            hpText = hpNumGo.AddComponent<Text>();
            StyleLabel(hpText, "100 / 100", 14, TextAnchor.MiddleLeft, Color.white);
            hpText.fontStyle = FontStyle.Bold;

            hpSlider = BuildSlider(panel.transform, new Vector2(0, -24), new Vector2(320, 12), new Color(0.18f, 0.55f, 0.25f), new Color(0.2f, 0.2f, 0.2f));
            BuildStealthSlider(panel.transform);

            GameObject boltGo = CreateUIObject("Bolt_Display", panel.transform);
            SetRect(boltGo, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, -65), new Vector2(200, 20));
            boltText = boltGo.AddComponent<Text>();
            StyleLabel(boltText, "BOLTS: 0", 16, TextAnchor.MiddleLeft, new Color(1f, 0.9f, 0.2f));
            boltText.fontStyle = FontStyle.Bold;
        }

        private void BuildStealthSlider(Transform parent)
        {
            float initWidth = StealthDurationToWidth(STEALTH_BASE_SECONDS);
            stealthSlider = BuildSlider(parent, new Vector2(0, -44), new Vector2(initWidth, 10), new Color(0.35f, 0.72f, 0.95f, 0.9f), new Color(0.2f, 0.2f, 0.2f));
            stealthSliderRT = stealthSlider.GetComponent<RectTransform>();
        }

        private void BuildRightItemSlots()
        {
            itemSlotImages.Clear();
            const float slotSize = 52f;
            const float slotSpacing = 6f;
            float totalH = MAX_ITEM_SLOTS * (slotSize + slotSpacing);

            GameObject container = CreateUIObject("ItemSlots_Container", hudCanvas.transform);
            RectTransform cRT = container.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(1, 0.5f);
            cRT.anchorMax = new Vector2(1, 0.5f);
            cRT.pivot = new Vector2(1, 0.5f);
            cRT.anchoredPosition = new Vector2(-20, 0);
            cRT.sizeDelta = new Vector2(slotSize + 16f, totalH);

            for (int i = 0; i < MAX_ITEM_SLOTS; i++)
            {
                float yPos = totalH * 0.5f - i * (slotSize + slotSpacing) - slotSize * 0.5f;
                GameObject slotGo = CreateUIObject($"ItemSlot_{i}", container.transform);
                RectTransform sRT = slotGo.GetComponent<RectTransform>();
                sRT.anchorMin = sRT.anchorMax = new Vector2(0.5f, 0.5f);
                sRT.pivot = new Vector2(0.5f, 0.5f);
                sRT.anchoredPosition = new Vector2(0, yPos);
                sRT.sizeDelta = new Vector2(slotSize, slotSize);

                Image bgImg = slotGo.AddComponent<Image>();
                bgImg.color = new Color(0.12f, 0.12f, 0.12f, 0.75f);
                Outline outline = slotGo.AddComponent<Outline>();
                outline.effectColor = new Color(0.35f, 0.35f, 0.35f, 0.8f);
                outline.effectDistance = new Vector2(1.5f, -1.5f);

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

        private void BuildBossHPPanel()
        {
            bossHPPanel = CreateUIObject("BossHP_Panel", hudCanvas.transform);
            RectTransform pRT = bossHPPanel.GetComponent<RectTransform>();
            pRT.anchorMin = new Vector2(0.5f, 0);
            pRT.anchorMax = new Vector2(0.5f, 0);
            pRT.pivot = new Vector2(0.5f, 0);
            pRT.anchoredPosition = new Vector2(0, 30);
            pRT.sizeDelta = new Vector2(500, 80);

            bossSubtitleText = CreateText("BossSubtitle", bossHPPanel.transform, new Vector2(0, 60), new Vector2(400, 20), "F2 BOSS", 11, TextAnchor.MiddleCenter, new Color(0.7f, 0.7f, 0.7f));
            bossNameText = CreateText("BossName", bossHPPanel.transform, new Vector2(0, 36), new Vector2(460, 32), "보스", 22, TextAnchor.MiddleCenter, Color.white);
            bossNameText.fontStyle = FontStyle.Bold;

            bossSlider = BuildSlider(bossHPPanel.transform, new Vector2(0, 10), new Vector2(460, 14), new Color(0.7f, 0.18f, 0.18f), new Color(0.2f, 0.2f, 0.2f));

            CreateText("BossHP_Label", bossHPPanel.transform, new Vector2(0, -6), new Vector2(200, 16), "Boss HP", 10, TextAnchor.MiddleCenter, new Color(0.55f, 0.55f, 0.55f));

            bossHPPanel.SetActive(false);
            BuildWavePromptPanel();
        }

        private void BuildWavePromptPanel()
        {
            wavePromptPanel = CreateUIObject("WavePrompt_Panel", hudCanvas.transform);
            RectTransform pRT = wavePromptPanel.GetComponent<RectTransform>();
            pRT.anchorMin = new Vector2(1, 0);
            pRT.anchorMax = new Vector2(1, 0);
            pRT.pivot = new Vector2(1, 0);
            pRT.anchoredPosition = new Vector2(-20, 20);
            pRT.sizeDelta = new Vector2(400, 50);

            wavePromptText = CreateText("WavePrompt_Text", wavePromptPanel.transform, Vector2.zero, new Vector2(400, 50), "Press [T] to Continue", 18, TextAnchor.LowerRight, new Color(1f, 0.8f, 0f));
            wavePromptText.fontStyle = FontStyle.Bold;

            Outline outline = wavePromptText.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1f, -1f);

            wavePromptPanel.SetActive(false);
        }

        private void RefreshHP()
        {
            if (playerStats == null || hpSlider == null) return;
            hpSlider.value = Mathf.Clamp01(playerStats.currentHealth / playerStats.MaxHealth);
            if (hpText != null) hpText.text = $"{Mathf.CeilToInt(playerStats.currentHealth)} / {Mathf.CeilToInt(playerStats.MaxHealth)}";
        }

        private void RefreshStealth()
        {
            if (playerStealth == null || stealthSlider == null) return;
            stealthSlider.value = playerStealth.StealthRatio;
            Image fill = stealthSlider.fillRect?.GetComponent<Image>();
            if (fill != null)
            {
                if (playerStealth.IsStealthActive) fill.color = new Color(0.35f, 0.72f, 0.95f, 0.9f);
                else if (playerStealth.IsRecharging) fill.color = new Color(0.3f, 0.3f, 0.35f, 0.8f);
                else fill.color = new Color(0.55f, 0.85f, 1f, 0.95f);
            }
            if (stealthSliderRT != null)
            {
                float targetWidth = StealthDurationToWidth(playerStealth.MaxDuration);
                if (!Mathf.Approximately(stealthSliderRT.sizeDelta.x, targetWidth)) stealthSliderRT.sizeDelta = new Vector2(targetWidth, stealthSliderRT.sizeDelta.y);
            }
        }

        private void RefreshBolts()
        {
            if (playerStats == null || boltText == null) return;
            boltText.text = $"BOLTS: {playerStats.Bolts}";
        }

        public void ShowBossHP(string bossName, string subtitle = "")
        {
            if (bossHPPanel == null) return;
            bossHPPanel.SetActive(true);
            if (bossNameText != null) bossNameText.text = bossName;
            if (bossSubtitleText != null) { bossSubtitleText.text = subtitle; bossSubtitleText.gameObject.SetActive(!string.IsNullOrEmpty(subtitle)); }
            if (bossSlider != null) bossSlider.value = 1f;
        }

        public void HideBossHP()
        {
            if (bossHPPanel != null) bossHPPanel.SetActive(false);
        }

        public void SetBossHP(float ratio)
        {
            if (bossSlider != null) bossSlider.value = Mathf.Clamp01(ratio);
        }

        public void ShowWavePrompt(string message)
        {
            if (wavePromptPanel == null) return;
            wavePromptPanel.SetActive(true);
            if (wavePromptText != null) wavePromptText.text = message;
        }

        public void HideWavePrompt()
        {
            if (wavePromptPanel != null) wavePromptPanel.SetActive(false);
        }

        public void AddItem(Sprite icon)
        {
            if (storedItems.Count >= MAX_ITEM_SLOTS) return;
            storedItems.Add(icon);
            RefreshItemSlots();
        }

        public void ClearItems()
        {
            storedItems.Clear();
            RefreshItemSlots();
        }

        private void RefreshItemSlots()
        {
            for (int i = 0; i < itemSlotImages.Count; i++)
            {
                if (i < storedItems.Count && storedItems[i] != null)
                {
                    itemSlotImages[i].sprite = storedItems[i];
                    itemSlotImages[i].color = Color.white;
                }
                else itemSlotImages[i].color = Color.clear;
            }
        }

        private static float StealthDurationToWidth(float duration)
        {
            float t = Mathf.InverseLerp(STEALTH_BASE_SECONDS, STEALTH_MAX_SECONDS, duration);
            return Mathf.Lerp(STEALTH_BASE_WIDTH, STEALTH_MAX_WIDTH, t);
        }

        private Slider BuildSlider(Transform parent, Vector2 anchoredPos, Vector2 size, Color fillColor, Color bgColor)
        {
            GameObject sliderGo = CreateUIObject("Slider", parent);
            Slider slider = sliderGo.AddComponent<Slider>();
            slider.minValue = 0f; slider.maxValue = 1f; slider.value = 1f;
            RectTransform sRT = sliderGo.GetComponent<RectTransform>();
            sRT.anchorMin = sRT.anchorMax = new Vector2(0, 1);
            sRT.pivot = new Vector2(0, 1);
            sRT.anchoredPosition = anchoredPos;
            sRT.sizeDelta = size;
            GameObject bgGo = CreateUIObject("Background", sliderGo.transform);
            Image bgImg = bgGo.AddComponent<Image>();
            bgImg.color = bgColor;
            RectTransform bgRT = bgGo.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one; bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
            GameObject fillAreaGo = CreateUIObject("Fill Area", sliderGo.transform);
            RectTransform faRT = fillAreaGo.GetComponent<RectTransform>();
            faRT.anchorMin = Vector2.zero; faRT.anchorMax = Vector2.one; faRT.offsetMin = faRT.offsetMax = Vector2.zero;
            GameObject fillGo = CreateUIObject("Fill", fillAreaGo.transform);
            Image fillImg = fillGo.AddComponent<Image>();
            fillImg.color = fillColor;
            RectTransform fillRT = fillGo.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one; fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
            slider.fillRect = fillRT; slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private Text CreateText(string name, Transform parent, Vector2 anchoredPos, Vector2 size, string content, int fontSize, TextAnchor anchor, Color color)
        {
            GameObject go = CreateUIObject(name, parent);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            Text t = go.AddComponent<Text>();
            StyleLabel(t, content, fontSize, anchor, color);
            return t;
        }

        private void StyleLabel(Text t, string content, int fontSize, TextAnchor anchor, Color color)
        {
            t.text = content; t.fontSize = fontSize; t.alignment = anchor; t.color = color;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private void SetRect(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot; rt.anchoredPosition = anchoredPos; rt.sizeDelta = sizeDelta;
        }

        private GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }
    }
}
