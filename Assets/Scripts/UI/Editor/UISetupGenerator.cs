using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UI;
using System.Collections.Generic;

namespace UI.Editor
{
    public class UISetupGenerator
    {
        [MenuItem("Tools/Generate UI Flow Scenes")]
        public static void GenerateUIScenes()
        {
            string titleScenePath = "Assets/Scenes/TitleScene.unity";
            string lobbyScenePath = "Assets/Scenes/LobbyScene.unity";
            
            // Ensure Scenes directory exists
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            CreateTitleScene(titleScenePath);
            CreateLobbyScene(lobbyScenePath);

            AddScenesToBuildSettings(new string[] { titleScenePath, lobbyScenePath, "Assets/Scenes/SampleScene.unity" });
            
            Debug.Log("UI Flow Scenes Generated and Added to Build Settings!");
        }

        private static void CreateTitleScene(string path)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "TitleScene";

            GameObject canvasObj = SetupBaseSceneContext(out GameObject uiManagerObj);

            // Left Layout
            CreateLeftPanelLayout(canvasObj.transform);

            // Right Layout (Title: Login Form)
            GameObject rightContainer = CreateRightPanelContainer(canvasObj.transform);

            // ID Field
            GameObject idField = AddInputField("IDField", rightContainer.transform, "ID");
            idField.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);

            // Pass Field
            GameObject passField = AddInputField("PassField", rightContainer.transform, "Password");
            passField.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 40);
            passField.GetComponent<InputField>().inputType = InputField.InputType.Password;

            // Login Button (Black, style of "게임 시작")
            GameObject loginBtn = AddMenuButton("LoginBtn", rightContainer.transform, "로그인", "[ENTER]", true);
            loginBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -30);

            // Sign Up Button
            GameObject signupBtn = AddMenuButton("SignupBtn", rightContainer.transform, "계정 생성", "[NEW]", false);
            signupBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -90);

            // Status Text
            GameObject statusText = AddText("StatusText", rightContainer.transform, "Ready", 16, Color.red, TextAnchor.MiddleCenter);
            statusText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -150);

            // Login Controller
            LoginController controller = canvasObj.AddComponent<LoginController>();
            controller.idInput = idField.GetComponent<InputField>();
            controller.passwordInput = passField.GetComponent<InputField>();
            controller.loginButton = loginBtn.GetComponent<Button>();
            controller.signupButton = signupBtn.GetComponent<Button>();
            controller.statusText = statusText.GetComponent<Text>();

            EditorSceneManager.SaveScene(scene, path);
        }

        private static void CreateLobbyScene(string path)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "LobbyScene";

            GameObject canvasObj = SetupBaseSceneContext(out GameObject uiManagerObj, isLobby: true);

            // Left Layout
            CreateLeftPanelLayout(canvasObj.transform);

            // Right Layout (Lobby: Menus)
            GameObject rightContainer = CreateRightPanelContainer(canvasObj.transform);

            // Buttons
            GameObject startBtn = AddMenuButton("StartBtn", rightContainer.transform, "게임 시작", "[ENTER]", true);
            startBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);

            GameObject statsBtn = AddMenuButton("StatsBtn", rightContainer.transform, "업적 / 기록", "도감·통계", false);
            statsBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 40);

            GameObject settingsBtn = AddMenuButton("SettingsBtn", rightContainer.transform, "설정", "옵션", false);
            settingsBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -20);

            GameObject quitBtn = AddMenuButton("QuitBtn", rightContainer.transform, "종료", "[ESC]", false);
            quitBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -80);

            // Lobby Controller
            LobbyController controller = canvasObj.AddComponent<LobbyController>();
            // Since we don't have a welcomeText in this specific mockup UI, we can create a hidden one or place it at top right.
            GameObject welcome = AddText("WelcomeText", rightContainer.transform, "Welcome, User!", 14, Color.black, TextAnchor.MiddleRight);
            welcome.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 160);
            controller.welcomeText = welcome.GetComponent<Text>();

            controller.startButton    = startBtn.GetComponent<Button>();
            controller.statsButton    = statsBtn.GetComponent<Button>();
            controller.settingsButton = settingsBtn.GetComponent<Button>();
            controller.quitButton     = quitBtn.GetComponent<Button>();

            EditorSceneManager.SaveScene(scene, path);
        }

        private static GameObject SetupBaseSceneContext(out GameObject uiManagerObj, bool isLobby = false)
        {
            // 1. Camera
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.white;
            
            // UIManager
            if (!isLobby)
            {
                uiManagerObj = new GameObject("UIManager");
                uiManagerObj.AddComponent<UIManager>();
            }
            else
            {
                uiManagerObj = null;
            }

            // 2. Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // 3. EventSystem
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            return canvasObj;
        }

        private static void CreateLeftPanelLayout(Transform canvasTransform)
        {
            Color darkGray = new Color(0.15f, 0.15f, 0.15f);
            Color lightGray = new Color(0.6f, 0.6f, 0.6f);
            Color boxColor = new Color(0.95f, 0.95f, 0.95f);

            // Main Label Container
            GameObject leftContainer = new GameObject("LeftContainer");
            leftContainer.transform.SetParent(canvasTransform, false);
            RectTransform rt = leftContainer.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(100, 0); 
            rt.offsetMax = new Vector2(0, 0);

            // Text elements
            GameObject projText = AddText("ProjText", leftContainer.transform, "PROJECT", 18, lightGray, TextAnchor.MiddleLeft);
            RectTransform projRt = projText.GetComponent<RectTransform>();
            projRt.anchorMin = projRt.anchorMax = new Vector2(0, 0.5f);
            projRt.pivot = new Vector2(0, 0.5f);
            projRt.anchoredPosition = new Vector2(50, 200);
            projRt.sizeDelta = new Vector2(600, 40);

            GameObject titleText = AddText("TitleText", leftContainer.transform, "RE-WRITE", 100, darkGray, TextAnchor.MiddleLeft);
            titleText.GetComponent<Text>().fontStyle = FontStyle.Bold;
            RectTransform titleRt = titleText.GetComponent<RectTransform>();
            titleRt.anchorMin = titleRt.anchorMax = new Vector2(0, 0.5f);
            titleRt.pivot = new Vector2(0, 0.5f);
            titleRt.anchoredPosition = new Vector2(50, 110);
            titleRt.sizeDelta = new Vector2(600, 120);

            GameObject subText = AddText("SubText", leftContainer.transform, "리 라 이 트", 24, lightGray, TextAnchor.MiddleLeft);
            RectTransform subRt = subText.GetComponent<RectTransform>();
            subRt.anchorMin = subRt.anchorMax = new Vector2(0, 0.5f);
            subRt.pivot = new Vector2(0, 0.5f);
            subRt.anchoredPosition = new Vector2(50, 20);
            subRt.sizeDelta = new Vector2(600, 40);

            // --- Items below the title: position relative to title bottom ---
            // Title center Y = 110, height = 120  ->  title bottom y = 110 - 60 = 50
            // "리라이트" sub label sits at y=20 (height 40) -> bottom edge y = 0
            // We align below sub label bottom (y=0) and stack downward.
            float baseX = 50f;          // same left offset as title
            float curY  = -16f;         // small gap under sub-label bottom (y=0)
            float lineGap = 16f;
            float synopsisHeight = 160f;

            // Dividing line
            GameObject line = AddUIElement<Image>("Line", leftContainer.transform);
            line.GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f);
            RectTransform lineRt = line.GetComponent<RectTransform>();
            lineRt.anchorMin = lineRt.anchorMax = new Vector2(0, 0.5f);
            lineRt.pivot = new Vector2(0, 0.5f);
            lineRt.anchoredPosition = new Vector2(baseX, curY);
            lineRt.sizeDelta = new Vector2(600, 2);
            curY -= lineGap;

            // Synopsis Box
            GameObject synBox = AddUIElement<Image>("SynopsisBox", leftContainer.transform);
            synBox.GetComponent<Image>().color = boxColor;
            RectTransform synRt = synBox.GetComponent<RectTransform>();
            synRt.anchorMin = synRt.anchorMax = new Vector2(0, 0.5f);
            synRt.pivot = new Vector2(0, 0.5f);
            synRt.anchoredPosition = new Vector2(baseX, curY - synopsisHeight * 0.5f);
            synRt.sizeDelta = new Vector2(600, synopsisHeight);
            curY -= synopsisHeight + lineGap;

            // Add border outline to box
            Outline outline = synBox.AddComponent<Outline>();
            outline.effectColor = new Color(0.8f, 0.8f, 0.8f);
            outline.effectDistance = new Vector2(1, -1);

            string info = "난이도 자동 조정을 지원하는 AI 프레임워크를 위한 샘플 게임입니다.";
            GameObject synText = AddText("SynopsisText", synBox.transform, info, 18, lightGray, TextAnchor.UpperLeft);
            RectTransform sTxtRt = synText.GetComponent<RectTransform>();
            sTxtRt.anchorMin = new Vector2(0, 0);
            sTxtRt.anchorMax = new Vector2(1, 1);
            sTxtRt.offsetMin = new Vector2(20, 20);
            sTxtRt.offsetMax = new Vector2(-20, -20);


            // --- Bottom Footer section ---
            GameObject footerContainer = new GameObject("FooterContainer");
            footerContainer.transform.SetParent(canvasTransform, false);
            RectTransform footerRt = footerContainer.AddComponent<RectTransform>();
            footerRt.anchorMin = new Vector2(0, 0);
            footerRt.anchorMax = new Vector2(1, 0);
            footerRt.offsetMin = new Vector2(0, 0);
            footerRt.offsetMax = new Vector2(0, 40);

            GameObject footerLine = AddUIElement<Image>("FooterLine", footerContainer.transform);
            footerLine.GetComponent<Image>().color = new Color(0.9f, 0.9f, 0.9f);
            RectTransform flRt = footerLine.GetComponent<RectTransform>();
            flRt.anchorMin = new Vector2(0, 1);
            flRt.anchorMax = new Vector2(1, 1);
            flRt.offsetMin = new Vector2(0, -1);
            flRt.offsetMax = new Vector2(0, 0);

            GameObject fr = AddText("FootR", footerContainer.transform, "CORE_SYSTEM_V2.7.4", 14, lightGray, TextAnchor.MiddleRight);
            fr.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0);
            fr.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            fr.GetComponent<RectTransform>().offsetMax = new Vector2(-20, 0);
            fr.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 0);
        }

        private static GameObject CreateRightPanelContainer(Transform canvasTransform)
        {
            GameObject container = new GameObject("RightContainer");
            container.transform.SetParent(canvasTransform, false);
            RectTransform rt = container.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.6f, 0.1f);
            rt.anchorMax = new Vector2(0.95f, 0.9f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return container;
        }

        // --- Extracted specialized UI builders ---

        private static GameObject AddMenuButton(string name, Transform parent, string mainText, string rightSubText, bool isPrimary)
        {
            GameObject obj = AddUIElement<Image>(name, parent);
            Image bg = obj.GetComponent<Image>();
            
            Color blackCol = new Color(0.12f, 0.12f, 0.12f);
            Color whiteCol = new Color(0.96f, 0.96f, 0.96f);
            
            bg.color = isPrimary ? blackCol : whiteCol;

            if (!isPrimary)
            {
                Outline outline = obj.AddComponent<Outline>();
                outline.effectColor = new Color(0.8f, 0.8f, 0.8f);
                outline.effectDistance = new Vector2(1, -1);
            }

            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = bg;
            
            Color textCol = isPrimary ? Color.white : blackCol;
            Color subTextCol = isPrimary ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.6f, 0.6f, 0.6f);

            GameObject mainTObj = AddText("MainText", obj.transform, mainText, 22, textCol, TextAnchor.MiddleLeft);
            mainTObj.GetComponent<Text>().fontStyle = FontStyle.Bold;
            RectTransform mRt = mainTObj.GetComponent<RectTransform>();
            mRt.anchorMin = new Vector2(0, 0); mRt.anchorMax = new Vector2(1, 1);
            mRt.offsetMin = new Vector2(30, 0); mRt.offsetMax = new Vector2(-30, 0);

            GameObject subTObj = AddText("SubText", obj.transform, rightSubText, 16, subTextCol, TextAnchor.MiddleRight);
            RectTransform sRt = subTObj.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0, 0); sRt.anchorMax = new Vector2(1, 1);
            sRt.offsetMin = new Vector2(30, 0); sRt.offsetMax = new Vector2(-30, 0);

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400, 50);
            return obj;
        }

        private static void AddScenesToBuildSettings(string[] paths)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            foreach (string p in paths)
            {
                if (System.IO.File.Exists(p))
                {
                    scenes.Add(new EditorBuildSettingsScene(p, true));
                }
                else
                {
                    Debug.LogWarning($"Scene not found to add to build settings: {p}");
                }
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static GameObject AddUIElement<T>(string name, Transform parent) where T : Component
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            obj.AddComponent<T>();
            return obj;
        }

        private static void Stretch(GameObject obj)
        {
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static GameObject AddText(string name, Transform parent, string content, int fontSize, Color color, TextAnchor align)
        {
            GameObject obj = AddUIElement<Text>(name, parent);
            Text text = obj.GetComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = align;
            
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return obj;
        }

        private static GameObject AddInputField(string name, Transform parent, string placeholderTextStr)
        {
            GameObject obj = AddUIElement<InputField>(name, parent);
            
            GameObject bgObj = AddUIElement<Image>("Background", obj.transform);
            Stretch(bgObj);
            bgObj.GetComponent<Image>().color = new Color(0.96f, 0.96f, 0.96f);
            Outline outline = bgObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.8f, 0.8f, 0.8f);
            outline.effectDistance = new Vector2(1, -1);

            obj.GetComponent<InputField>().targetGraphic = bgObj.GetComponent<Image>();

            GameObject textObj = AddText("Text", obj.transform, "", 20, Color.black, TextAnchor.MiddleLeft);
            Stretch(textObj);
            textObj.GetComponent<RectTransform>().offsetMin = new Vector2(20, 0);

            GameObject placeholderObj = AddText("Placeholder", obj.transform, placeholderTextStr, 20, new Color(0.6f, 0.6f, 0.6f), TextAnchor.MiddleLeft);
            Stretch(placeholderObj);
            placeholderObj.GetComponent<RectTransform>().offsetMin = new Vector2(20, 0);

            InputField field = obj.GetComponent<InputField>();
            field.textComponent = textObj.GetComponent<Text>();
            field.placeholder = placeholderObj.GetComponent<Text>();

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400, 50);
            return obj;
        }
    }
}
