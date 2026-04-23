using System.Collections.Generic;
using Core; // EntityStatus와 Buff 시스템 참조
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class PlayerUI : MonoBehaviour
    {
        public static PlayerUI Instance { get; private set; }

        [Header("References")]
        private Player.PlayerStats playerStats;
        private Player.PlayerStealth playerStealth;
        private Player.PlayerInteractor playerInteractor;
        private BuffManager playerBuffManager; // 추가

        private Canvas hudCanvas;

        // HP & Stealth
        private Slider hpSlider;
        private TextMeshProUGUI hpText;
        private Slider stealthSlider;
        private RectTransform stealthSliderRT;

        // 아이템 슬롯 (우측)
        private const int MAX_ITEM_SLOTS = 10;
        private List<Image> itemSlotImages = new List<Image>();

        // 버프 아이콘 (추가)
        private List<Image> buffIconImages = new List<Image>();
        private GameObject buffContainer;

        private GameObject interactRoot; // 텍스트를 담고 있는 부모 오브젝트
        private TextMeshProUGUI interactText; // 실제 TMP 컴포넌트

        // 보스 HP
        private GameObject bossHPPanel;
        private Slider bossSlider;
        private TextMeshProUGUI bossNameText;
        private TextMeshProUGUI bossSubtitleText;

        // 폰트
        private TMP_FontAsset font;

        private PlayerInput playerInput;

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                // [수정] Awake에서 미리 Canvas를 만들어야 OnEnable에서 에러가 안 납니다.
                EnsureCanvasExists();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        #region Canvas 생성

        private void BuildCanvas()
        {
            // 기존에 이미 같은 이름의 Canvas가 있다면 찾아서 연결 (중복 생성 방지)
            GameObject existingCanvas = GameObject.Find("GameHUD_Canvas");
            if (existingCanvas != null)
            {
                hudCanvas = existingCanvas.GetComponent<Canvas>();
                return;
            }
            GameObject canvasGo = new GameObject("GameHUD_Canvas");
            hudCanvas = canvasGo.AddComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudCanvas.sortingOrder = 100;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();
            canvasGo.transform.SetParent(gameObject.transform, false);
        }

        private void EnsureCanvasExists()
        {
            // Canvas가 없거나, 파괴되었다면 새로 생성
            if (hudCanvas == null)
            {
                BuildCanvas();
            }
        }
        #endregion

        private void BuildInteract()
        {
            if (interactRoot == null)
            {
                interactRoot = new GameObject("InteractPrompt");
                interactText = interactRoot.AddComponent<TextMeshProUGUI>();
                interactText.color = Color.yellow;
                interactText.font = font;
                interactText.fontSize = 36;
                interactText.textWrappingMode = TextWrappingModes.NoWrap;
                interactText.verticalAlignment = VerticalAlignmentOptions.Middle;
                interactText.horizontalAlignment = HorizontalAlignmentOptions.Center;
                interactRoot.transform.SetParent(hudCanvas.transform, false);

                // 이동 후 스케일이 꼬일 수 있으니 (1,1,1)로 초기화
                interactRoot.transform.localScale = Vector3.one;
            }
        }

        public void SetPlayerInput(PlayerInput input)
        {
            playerInput = input;
        }

        private void Start()
        {
            // 인벤토리 매니저 이벤트 구독 (아이템 획득 시 HUD 갱신)
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnItemAdded += SyncInventory;
            }
            if (Player.PlayerStats.LocalPlayer != null)
            {
                BindPlayer(Player.PlayerStats.LocalPlayer);
            }
            DontDestroyOnLoad(gameObject);
            // 참조 자동 할당

            BuildCanvas();
            BuildTopLeftHUD();
            BuildBuffContainer(); // 버프 UI 생성 추가
            BuildRightItemSlots();
            BuildBossHPPanel();
            BuildInteract();
        }

        private void OnEnable()
        {
            // 씬 로드 이벤트 구독
            SceneManager.sceneLoaded += OnSceneLoaded;
            Player.PlayerStats.OnPlayerReady += BindPlayer;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Player.PlayerStats.OnPlayerReady -= BindPlayer;
        }

        // 씬이 로드될 때마다 실행됨
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 씬이 바뀌었으므로 즉시 플레이어를 다시 찾아서 연결
            if (Player.PlayerStats.LocalPlayer != null)
            {
                BindPlayer(Player.PlayerStats.LocalPlayer);
            }
        }

        private void BindPlayer(Player.PlayerStats stats)
        {
            playerStats = stats;
            playerStealth = stats.GetComponent<Player.PlayerStealth>();
            playerBuffManager = stats.GetComponent<BuffManager>();
            playerInteractor = stats.GetComponent<Player.PlayerInteractor>();
            playerInteractor.OnInteractableChanged += UpdateInteractUI;

            RebuildUIReferences();
        }

        public void SetFont(TMP_FontAsset font)
        {
            this.font = font;
        }

        private void RebuildUIReferences()
        {
            // 기존 리스트가 파괴된 객체들을 들고 있다면 비워줍니다.
            buffIconImages.Clear();
            itemSlotImages.Clear();

            // 다시 슬롯들을 생성하는 로직 호출
            BuildBuffContainer();
            BuildRightItemSlots();

            SyncInventory();
            RefreshHP();
            RefreshStealth();
            RefreshBuffs();
        }

        private void Update()
        {
            if (playerStats == null)
            {
                if (Player.PlayerStats.LocalPlayer != null)
                {
                    BindPlayer(Player.PlayerStats.LocalPlayer);
                }
                return; // 찾을 때까지는 아래 로직 실행 안 함
            }
            RefreshHP();
            RefreshStealth();
            RefreshBuffs(); // 버프 아이콘 갱신 추가
            HandleInteractUIPosition();
        }

        private void HandleInteractUIPosition()
        {
            // 플레이어나 텍스트 객체가 없으면 계산 안 함
            if (
                playerStats == null
                || interactRoot == null
                || !interactRoot.activeSelf
                || Camera.main == null
            )
            {
                return;
            }

            // [핵심 로직]
            // 1. 플레이어 머리 위 월드 좌표 (예: 발밑 0,0에서 위로 1.5f)
            Vector3 playerHeadPos = playerStats.transform.position + Vector3.up * 1.5f;

            // 2. 카메라가 바라보는 스크린 좌표로 변환
            Vector3 screenPos = Camera.main.WorldToScreenPoint(playerHeadPos);

            // 3. 카메라 앞에 있을 때만 표시
            if (screenPos.z > 0)
            {
                // 4. [가장 중요] Z값을 0으로 고정하여 캔버스 평면에 붙입니다.
                // 그리고 이 값을 일반 transform.position에 넣으면
                // Screen Space - Overlay 모드에서는 정확하게 꽂힙니다.
                interactRoot.transform.position = new Vector3(screenPos.x, screenPos.y, 0);
            }
            else
            {
                // 카메라 뒤에 있으면 안 보이게 화면 밖으로 치움
                interactRoot.transform.position = new Vector3(-10000, -10000, 0);
            }
        }

        #region HP & Stealth 로직 (소문자 변수 반영)

        private void RefreshHP()
        {
            if (playerStats == null || hpSlider == null)
                return;

            // [수정] (float)를 붙여서 강제로 소수점 계산을 하게 만듭니다.
            // 또한 maxHealth가 0일 경우 발생하는 오류(DivideByZero)를 방지하기 위해 0.001f를 더하거나 체크합니다.
            float max = Mathf.Max(playerStats.maxHealth, 0.001f);
            float ratio = Mathf.Clamp01((float)playerStats.currentHealth / max);

            hpSlider.value = ratio;

            if (hpText != null)
            {
                // 수치는 CeilToInt로 깔끔하게 정수로 표시
                hpText.text =
                    $"{Mathf.CeilToInt(playerStats.currentHealth)} / {Mathf.CeilToInt(playerStats.maxHealth)}";
            }
        }

        private void RefreshStealth()
        {
            if (playerStealth == null || stealthSlider == null)
                return;

            // 1. 게이지 비율 반영 (이미 PlayerStealth에서 계산된 값 사용)
            stealthSlider.value = playerStealth.StealthRatio;

            // 2. 상태별 색상 변경
            Image fill = stealthSlider.fillRect?.GetComponent<Image>();
            if (fill != null)
            {
                if (playerStealth.IsStealthActive)
                    fill.color = Color.cyan; // 사용 중
                else if (playerStealth.IsRecharging)
                    fill.color = Color.gray; // 충전 중
                else
                    fill.color = Color.green; // 완충
            }

            // 3. 슬라이더 폭 갱신 (상수 없이 실시간 계산)
            if (stealthSliderRT != null)
            {
                float targetWidth = StealthDurationToWidth(playerStealth.MaxDuration);

                if (!Mathf.Approximately(stealthSliderRT.sizeDelta.x, targetWidth))
                {
                    stealthSliderRT.sizeDelta = new Vector2(
                        targetWidth,
                        stealthSliderRT.sizeDelta.y
                    );
                }
            }
        }

        #endregion

        #region 버프 아이콘 로직 (신규)

        private void BuildBuffContainer()
        {
            // 만약 hudCanvas가 없다면 여기서 한 번 더 체크 (최종 방어선)
            EnsureCanvasExists();

            // 기존에 이미 Container가 있다면 지우고 새로 만듭니다 (중복 방지)
            if (buffContainer != null)
                Destroy(buffContainer);
            buffContainer = CreateUIObject("BuffIcons_Container", hudCanvas.transform);
            RectTransform rt = buffContainer.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(20, -100); // HP바 아래쪽
            rt.sizeDelta = new Vector2(400, 40);

            // 미리 5개 정도 아이콘 슬롯 생성
            for (int i = 0; i < 5; i++)
            {
                GameObject iconGo = CreateUIObject($"BuffIcon_{i}", buffContainer.transform);
                RectTransform iRT = iconGo.GetComponent<RectTransform>();
                iRT.anchoredPosition = new Vector2(i * 45, 0);
                iRT.sizeDelta = new Vector2(40, 40);

                Image img = iconGo.AddComponent<Image>();
                img.color = Color.clear;
                buffIconImages.Add(img);
            }
        }

        private void RefreshBuffs()
        {
            if (playerBuffManager == null)
                return;

            // BuffManager의 활성화된 효과 리스트를 순회하며 아이콘 표시
            var activeEffects = playerBuffManager._activeEffects;

            for (int i = 0; i < buffIconImages.Count; i++)
            {
                // [수정] 이 줄을 추가하여 파괴된 UI 객체에 접근하는 것을 방지합니다.
                if (buffIconImages[i] == null)
                    continue;
                if (i < activeEffects.Count)
                {
                    buffIconImages[i].sprite = activeEffects[i].Data.icon;
                    buffIconImages[i].color = Color.white;
                    // 남은 시간에 따른 투명도나 게이지 연출 추가 가능
                }
                else
                {
                    buffIconImages[i].color = Color.clear;
                }
            }
        }

        #endregion

        #region 인벤토리 동기화

        private void SyncInventory()
        {
            if (InventoryManager.Instance == null)
                return;

            var items = InventoryManager.Instance.items;
            for (int i = 0; i < itemSlotImages.Count; i++)
            {
                if (i < items.Count)
                {
                    itemSlotImages[i].sprite = items[i].icon;
                    itemSlotImages[i].color = Color.white;
                }
                else
                {
                    itemSlotImages[i].color = Color.clear;
                }
            }
        }

        #endregion
        // ────────────────────────────────────────────────────────────
        #region 좌상단 HUD (HP + 스텔스)

        private void BuildTopLeftHUD()
        {
            // 패널 루트
            GameObject panel = CreateUIObject("TopLeft_Panel", hudCanvas.transform);
            RectTransform panelRT = panel.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0, 1);
            panelRT.anchorMax = new Vector2(0, 1);
            panelRT.pivot = new Vector2(0, 1);
            panelRT.anchoredPosition = new Vector2(20, -20);
            panelRT.sizeDelta = new Vector2(340, 70);

            // HP 라벨
            GameObject hpLabel = CreateUIObject("HP_Label", panel.transform);
            SetRect(
                hpLabel,
                new Vector2(0, 1),
                new Vector2(0, 1),
                new Vector2(0, 1),
                new Vector2(0, 0),
                new Vector2(40, 22)
            );
            TextMeshProUGUI hpLabelText = hpLabel.AddComponent<TextMeshProUGUI>();
            StyleLabel(
                hpLabelText,
                "HP",
                14,
                TextAlignmentOptions.Left,
                new Color(0.8f, 0.8f, 0.8f)
            );

            // HP 수치 텍스트
            GameObject hpNumGo = CreateUIObject("HP_Num", panel.transform);
            SetRect(
                hpNumGo,
                new Vector2(0, 1),
                new Vector2(0, 1),
                new Vector2(0, 1),
                new Vector2(44, 0),
                new Vector2(200, 22)
            );
            hpText = hpNumGo.AddComponent<TextMeshProUGUI>();
            StyleLabel(hpText, "100 / 100", 14, TextAlignmentOptions.Left, Color.white);
            hpText.fontStyle = FontStyles.Bold;
            hpText.font = font;

            // HP 슬라이더
            hpSlider = BuildSlider(
                panel.transform,
                new Vector2(0, -24),
                new Vector2(320, 12),
                new Color(0.18f, 0.55f, 0.25f),
                new Color(0.2f, 0.2f, 0.2f)
            );

            // 스텔스 슬라이더
            BuildStealthSlider(panel.transform);
        }

        private void BuildStealthSlider(Transform parent)
        {
            // 슬라이더 배경 + 전경은 BuildSlider 가 만들어 줌
            // 초기 폭은 기본 지속 시간(3 s) 기준
            float initWidth = StealthDurationToWidth(playerStealth.MaxDuration);
            stealthSlider = BuildSlider(
                parent,
                new Vector2(0, -44),
                new Vector2(initWidth, 10),
                new Color(0.35f, 0.72f, 0.95f, 0.9f),
                new Color(0.2f, 0.2f, 0.2f)
            );
            stealthSliderRT = stealthSlider.GetComponent<RectTransform>();
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        #region 우측 아이템 슬롯

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

                // 슬롯 번호
                GameObject numGo = CreateUIObject($"SlotNum_{i}", slotGo.transform);
                SetRect(
                    numGo,
                    new Vector2(0, 1),
                    new Vector2(0, 1),
                    new Vector2(0, 1),
                    new Vector2(3, -2),
                    new Vector2(20, 15)
                );
                TextMeshProUGUI numText = numGo.AddComponent<TextMeshProUGUI>();
                numText.font = font;
                StyleLabel(
                    numText,
                    (i + 1).ToString(),
                    10,
                    TextAlignmentOptions.TopLeft,
                    new Color(0.55f, 0.55f, 0.55f)
                );

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
            pRT.anchorMin = new Vector2(0.5f, 0);
            pRT.anchorMax = new Vector2(0.5f, 0);
            pRT.pivot = new Vector2(0.5f, 0);
            pRT.anchoredPosition = new Vector2(0, 30);
            pRT.sizeDelta = new Vector2(500, 80);

            bossSubtitleText = CreateText(
                "BossSubtitle",
                bossHPPanel.transform,
                new Vector2(0, 60),
                new Vector2(400, 20),
                "F2 BOSS",
                11,
                TextAlignmentOptions.Center,
                new Color(0.7f, 0.7f, 0.7f)
            );

            bossNameText = CreateText(
                "BossName",
                bossHPPanel.transform,
                new Vector2(0, 36),
                new Vector2(460, 32),
                "보스",
                22,
                TextAlignmentOptions.Center,
                Color.white
            );
            bossNameText.fontStyle = FontStyles.Bold;
            bossNameText.font = font;

            bossSlider = BuildSlider(
                bossHPPanel.transform,
                new Vector2(0, 10),
                new Vector2(460, 14),
                new Color(0.7f, 0.18f, 0.18f),
                new Color(0.2f, 0.2f, 0.2f)
            );

            CreateText(
                "BossHP_Label",
                bossHPPanel.transform,
                new Vector2(0, -6),
                new Vector2(200, 16),
                "Boss HP",
                10,
                TextAlignmentOptions.Center,
                new Color(0.55f, 0.55f, 0.55f)
            );

            bossHPPanel.SetActive(false);
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        #region 내부 유틸

        /// <summary>지속 시간(초) → 슬라이더 픽셀 폭 변환</summary>
        /// <summary>
        /// 현재 스텔스 지속 시간에 비례하여 UI 바의 길이를 계산합니다.
        /// </summary>
        private float StealthDurationToWidth(float currentMax)
        {
            if (playerStealth == null)
                return 160f; // 기본 폭

            // [중요]
            // playerStealth.MaxDuration: 현재 레벨에서의 최대 시간 (예: 3초)
            // playerStealth.AbsoluteMaxDuration: 업그레이드 끝판왕 시간 (예: 5초)

            // 3초일 때 폭을 160, 5초일 때 폭을 260으로 잡고 싶다면 아래처럼 비례식을 씁니다.
            // 만약 이것도 싫으시면 그냥 currentMax * 50f 같은 식으로 단순하게 가셔도 됩니다.
            float ratio = currentMax / playerStealth.AbsoluteMaxDuration;
            return Mathf.Lerp(100f, 260f, ratio);
        }

        private Slider BuildSlider(
            Transform parent,
            Vector2 anchoredPos,
            Vector2 size,
            Color fillColor,
            Color bgColor
        )
        {
            GameObject sliderGo = CreateUIObject("Slider", parent);
            Slider slider = sliderGo.AddComponent<Slider>();
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            RectTransform sRT = sliderGo.GetComponent<RectTransform>();
            sRT.anchorMin = sRT.anchorMax = new Vector2(0, 1);
            sRT.pivot = new Vector2(0, 1);
            sRT.anchoredPosition = anchoredPos;
            sRT.sizeDelta = size;

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

            slider.fillRect = fillRT;
            slider.direction = Slider.Direction.LeftToRight;

            return slider;
        }

        private TextMeshProUGUI CreateText(
            string name,
            Transform parent,
            Vector2 anchoredPos,
            Vector2 size,
            string content,
            int fontSize,
            TextAlignmentOptions anchor,
            Color color
        )
        {
            GameObject go = CreateUIObject(name, parent);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
            StyleLabel(t, content, fontSize, anchor, color);
            return t;
        }

        private void StyleLabel(
            TextMeshProUGUI t,
            string content,
            int fontSize,
            TextAlignmentOptions anchor,
            Color color
        )
        {
            t.text = content;
            t.fontSize = fontSize;
            t.alignment = anchor;
            t.color = color;
        }

        private void SetRect(
            GameObject go,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPos,
            Vector2 sizeDelta
        )
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
        }

        private GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        #region 상호작용 UI 로직

        private void UpdateInteractUI(Level.IInteractable target)
        {
            if (target == null)
            {
                interactRoot.SetActive(false);
            }
            else
            {
                interactRoot.SetActive(true);
                interactText.text = $"[E] {target.GetInteractPrompt()}";
            }
        }

        #endregion
    }
}
