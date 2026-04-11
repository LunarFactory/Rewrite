#nullable enable
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UI;
using UnityEditor.Events;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EditorTools
{
    public class LoginUISetupTool : EditorWindow
    {
        private const string TITLE_SCENE = "Assets/Scenes/TitleScene.unity";
        private const string SIGNUP_SCENE = "Assets/Scenes/IdPwdCreateScene.unity";
        private const string RECOVER_SCENE = "Assets/Scenes/IdPwdRecoverScene.unity";

        [MenuItem("Tools/Setup/Step 1: Create New Auth Scenes Only")]
        public static void CreateAllScenes()
        {
            SetupSignupScene();
            SetupRecoverScene();
            AddScenesToBuildSettings();
            Debug.Log("Signup and Recovery Scenes Setup Completed!");
        }

        [MenuItem("Tools/Setup/Step 3: Add Panels for Single Scene Flow")]
        public static void AddSinglePanelFlow()
        {
            var scene = EditorSceneManager.OpenScene(TITLE_SCENE);

            GameObject rightContainer = GameObject.Find("RightContainer");
            if (rightContainer == null)
            {
                Debug.LogError("RightContainer not found in TitleScene!");
                return;
            }

            // 1. 기존 로그인 요소들을 Panel_Login 그룹으로 묶기
            Transform panelLoginTrans = rightContainer.transform.Find("Panel_Login");
            GameObject panelLogin;
            if (panelLoginTrans == null)
            {
                panelLogin = new GameObject("Panel_Login", typeof(RectTransform));
                panelLogin.transform.SetParent(rightContainer.transform, false);
                RectTransform rt = panelLogin.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = rt.offsetMax = Vector2.zero;

                List<Transform> childrenToMove = new List<Transform>();
                foreach (Transform child in rightContainer.transform)
                {
                    if (child != panelLogin.transform && !child.name.StartsWith("Panel_"))
                    {
                        childrenToMove.Add(child);
                    }
                }
                foreach (Transform child in childrenToMove)
                {
                    child.SetParent(panelLogin.transform, true);
                }
            }
            else
            {
                panelLogin = panelLoginTrans.gameObject;
            }

            // 2. Panel_Signup 생성 (없을 경우만)
            Transform panelSignupTrans = rightContainer.transform.Find("Panel_Signup");
            GameObject panelSignup = panelSignupTrans != null ? panelSignupTrans.gameObject : BuildSignupPanel(rightContainer.transform);
            panelSignup.SetActive(false);

            // 3. Panel_Recover 생성 (없을 경우만)
            Transform panelRecoverTrans = rightContainer.transform.Find("Panel_Recover");
            GameObject panelRecover = panelRecoverTrans != null ? panelRecoverTrans.gameObject : BuildRecoverPanel(rightContainer.transform);
            panelRecover.SetActive(false);

            // ── StatusText 확인 (없으면 추가) ─────────────────
            UnityEngine.UI.Text statusText = null;
            Transform statusTextTrans = rightContainer.transform.Find("StatusText");
            if (statusTextTrans != null) {
                statusText = statusTextTrans.GetComponent<UnityEngine.UI.Text>();
                statusText.transform.SetParent(rightContainer.transform, true); // Panel_Login에 옮겨졌을 최상단으로 꺼냄
                statusText.transform.SetAsLastSibling();
            } else {
                statusText = CreateText(rightContainer.transform, "StatusText", "", new Vector2(0, -210), 15, Color.red);
            }

            // ── AuthManager 확보 ────────────────────────────
            EnsureAuthManager();

            // ── 기존 LoginController 제거 ────────────────────────────────
            var oldController = Object.FindObjectOfType<LoginController>();
            if (oldController != null) DestroyImmediate(oldController);

            // ── TitleUIController 연결 ────────────────────────────────
            TitleUIController controller = Object.FindObjectOfType<TitleUIController>();
            if (controller == null)
            {
                GameObject controllerGo = new GameObject("TitleUIController");
                controller = controllerGo.AddComponent<TitleUIController>();
            }

            controller.panelLogin   = panelLogin;
            controller.panelSignup  = panelSignup;
            controller.panelRecover = panelRecover;
            controller.statusText   = statusText;

            // Login panel refs (기존 오브젝트 이름 사용)
            controller.loginIdInput  = FindInputInChildren(panelLogin.transform, "IDField", "ID", "Id");
            controller.loginPwInput  = FindInputInChildren(panelLogin.transform, "PassField", "PW", "Password");
            controller.loginBtn      = FindButtonInChildren(panelLogin.transform, "LoginBtn", "Login");
            controller.toSignupBtn   = FindButtonInChildren(panelLogin.transform, "SignupBtn", "Create", "생성");
            controller.toRecoverBtn  = FindButtonInChildren(panelLogin.transform, "RecoverButton", "Recover", "찾기");

            // 기존에 버튼들에 에디터로 연동되어있던 고정 이벤트(Persistent Event) 완전 제거 (Missing 오류 및 씬 로드 트리거 방지)
            ClearButtonPersistentEvents(controller.loginBtn);
            ClearButtonPersistentEvents(controller.toSignupBtn);
            ClearButtonPersistentEvents(controller.toRecoverBtn);
            
            // 붙어있는 SceneLoadTrigger 스크립트도 에디터에서 깨끗하게 제거
            RemoveComponent<SceneLoadTrigger>(controller.loginBtn);
            RemoveComponent<SceneLoadTrigger>(controller.toSignupBtn);
            RemoveComponent<SceneLoadTrigger>(controller.toRecoverBtn);

            // Signup panel refs
            controller.signupIdInput       = panelSignup.transform.Find("SignupID_Border/SignupID")?.GetComponent<UnityEngine.UI.InputField>();
            controller.signupPwInput       = panelSignup.transform.Find("SignupPW_Border/SignupPW")?.GetComponent<UnityEngine.UI.InputField>();
            controller.signupNicknameInput = panelSignup.transform.Find("SignupNick_Border/SignupNick")?.GetComponent<UnityEngine.UI.InputField>();
            controller.signupSubmitBtn     = panelSignup.transform.Find("SignupSubmitBtn")?.GetComponent<UnityEngine.UI.Button>();
            controller.signupBackBtn       = panelSignup.transform.Find("SignupBackBtn")?.GetComponent<UnityEngine.UI.Button>();

            // Recover panel refs
            controller.recoverNicknameInput = panelRecover.transform.Find("RecoverNick_Border/RecoverNick")?.GetComponent<UnityEngine.UI.InputField>();
            controller.recoverSubmitBtn     = panelRecover.transform.Find("RecoverSubmitBtn")?.GetComponent<UnityEngine.UI.Button>();
            controller.recoverBackBtn       = panelRecover.transform.Find("RecoverBackBtn")?.GetComponent<UnityEngine.UI.Button>();

            EditorUtility.SetDirty(controller);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Step 3] 패널이 추가되었습니다. 기존 TitleScene이 유지되며 단일 씬 흐름이 적용되었습니다.");
        }

        private static UnityEngine.UI.InputField FindInputInChildren(Transform parent, params string[] names)
        {
            var inputs = parent.GetComponentsInChildren<UnityEngine.UI.InputField>(true);
            foreach (var n in names)
            {
                var match = inputs.FirstOrDefault(i => i.name.Contains(n));
                if (match != null) return match;
            }
            return inputs.FirstOrDefault();
        }

        private static UnityEngine.UI.Button FindButtonInChildren(Transform parent, params string[] names)
        {
            var btns = parent.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            foreach (var n in names)
            {
                var match = btns.FirstOrDefault(b => b.name.Contains(n) || (b.GetComponentInChildren<UnityEngine.UI.Text>() != null && b.GetComponentInChildren<UnityEngine.UI.Text>().text.Contains(n)));
                if (match != null) return match;
            }
            return btns.FirstOrDefault();
        }

        private static void ClearButtonPersistentEvents(UnityEngine.UI.Button btn)
        {
            if (btn == null) return;
            while (btn.onClick.GetPersistentEventCount() > 0)
            {
                UnityEditor.Events.UnityEventTools.RemovePersistentListener(btn.onClick, 0);
            }
            EditorUtility.SetDirty(btn.gameObject);
        }

        private static void RemoveComponent<T>(UnityEngine.UI.Button btn) where T : Component
        {
            if (btn == null) return;
            var comp = btn.GetComponent<T>();
            if (comp != null) DestroyImmediate(comp);
        }

        private static void EnsureAuthManager()
        {
            if (FindObjectOfType<Auth.AuthManager>() == null)
            {
                var go = new GameObject("AuthManager");
                go.AddComponent<Auth.AuthManager>();
                Debug.Log("Added AuthManager to TitleScene.");
            }
        }

        // ── Panel 빌더: Login ─────────────────────────────────────────
        private static GameObject BuildLoginPanel(Transform parent)
        {
            GameObject panel = new GameObject("Panel_Login", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            panel.GetComponent<UnityEngine.UI.Image>().color = Color.clear;

            CreateText(panel.transform, "PanelTitle", "로그인", new Vector2(0, 140), 32, new Color(0.1f, 0.1f, 0.1f));

            CreateInputField(panel.transform, "LoginID",  "아이디",   new Vector2(0,  50));
            CreateInputField(panel.transform, "LoginPW",  "비밀번호", new Vector2(0, -10), true);

            CreateButton(panel.transform, "LoginBtn",    "로그인",          new Vector2(0, -80),  new Vector2(260, 48), new Color(0.13f, 0.13f, 0.16f), Color.white);
            CreateButton(panel.transform, "ToSignupBtn", "계정 생성",       new Vector2(0, -140), new Vector2(260, 38), Color.clear, new Color(0.4f, 0.4f, 0.4f));
            CreateButton(panel.transform, "ToRecoverBtn","아이디 / 비밀번호 찾기", new Vector2(0, -183), new Vector2(260, 30), Color.clear, new Color(0.5f, 0.5f, 0.5f));

            return panel;
        }

        // ── Panel 빌더: Signup ────────────────────────────────────────
        private static GameObject BuildSignupPanel(Transform parent)
        {
            GameObject panel = new GameObject("Panel_Signup", typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            
            // 기존 씬의 UI 요소 위치를 참고하여 적절한 위치 지정
            CreateText(panel.transform, "PanelTitle", "계정 생성", new Vector2(0, 150), 32, new Color(0.1f, 0.1f, 0.1f));

            CreateInputField(panel.transform, "SignupID",   "아이디",   new Vector2(0,  50));
            CreateInputField(panel.transform, "SignupPW",   "비밀번호", new Vector2(0,  -10), true);
            CreateInputField(panel.transform, "SignupNick", "닉네임",   new Vector2(0, -70));

            CreateButton(panel.transform, "SignupSubmitBtn", "계정 생성하기",   new Vector2(0, -140), new Vector2(260, 48), new Color(0.13f, 0.13f, 0.16f), Color.white);
            CreateButton(panel.transform, "SignupBackBtn",   "← 로그인 화면으로", new Vector2(0, -200), new Vector2(200, 32), Color.clear, new Color(0.4f, 0.4f, 0.4f));

            return panel;
        }

        // ── Panel 빌더: Recover ───────────────────────────────────────
        private static GameObject BuildRecoverPanel(Transform parent)
        {
            GameObject panel = new GameObject("Panel_Recover", typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            CreateText(panel.transform, "PanelTitle", "계정 찾기", new Vector2(0, 150), 32, new Color(0.1f, 0.1f, 0.1f));
            CreateText(panel.transform, "Hint", "가입 시 사용한 닉네임을 입력하세요.", new Vector2(0, 90), 14, new Color(0.5f, 0.5f, 0.5f));

            CreateInputField(panel.transform, "RecoverNick", "닉네임", new Vector2(0, 30));

            CreateButton(panel.transform, "RecoverSubmitBtn", "계정 찾기",       new Vector2(0, -50),  new Vector2(260, 48), new Color(0.13f, 0.13f, 0.16f), Color.white);
            CreateButton(panel.transform, "RecoverBackBtn",   "← 로그인 화면으로", new Vector2(0, -110), new Vector2(200, 32), Color.clear, new Color(0.4f, 0.4f, 0.4f));

            return panel;
        }

        [MenuItem("Tools/Setup/Step 2: Polish Existing TitleScene")]
        public static void PolishTitleScene()
        {
            var scene = EditorSceneManager.OpenScene(TITLE_SCENE);
            Canvas canvas = GetOrCreateCanvas();
            EnsureEventSystem();
            
            // 1. Remove 'READY' text
            foreach (var t in FindObjectsOfType<Text>())
            {
                if (t.name.ToLower().Contains("ready") || t.text.ToLower().Contains("ready"))
                {
                    DestroyImmediate(t.gameObject);
                }
            }

            // 2. Add or Update Recovery Button layout (with DUPLICATE CLEANUP)
            Button? signupBtn = FindButtonInScene("계정 생성", "Signup", "CreateAccount");
            if (signupBtn != null)
            {
                // Find ALL buttons that could be recovery buttons
                var allButtons = FindObjectsOfType<Button>();
                List<Button> recoveryCandidates = new List<Button>();
                string[] recoveryKeywords = { "찾기", "Recover", "Forgot", "아이디" };
                
                foreach (var b in allButtons)
                {
                    // CRITICAL: NEVER consider the signup button as a recovery candidate!
                    if (signupBtn != null && b == signupBtn) continue;

                    if (b.name == "RecoverButton")
                    {
                        recoveryCandidates.Add(b);
                        continue;
                    }
                    
                    Text t = b.GetComponentInChildren<Text>();
                    if (t != null)
                    {
                        foreach (var k in recoveryKeywords)
                        {
                            if (t.text.Contains(k))
                            {
                                recoveryCandidates.Add(b);
                                break;
                            }
                        }
                    }
                }

                // If zero, we create one. If many, keep the first one and DESTROY others.
                GameObject recoverBtnGo;
                Button recoverBtn;

                if (recoveryCandidates.Count == 0)
                {
                    Vector2 pos = signupBtn.GetComponent<RectTransform>().anchoredPosition;
                    pos.y -= (signupBtn.GetComponent<RectTransform>().sizeDelta.y + 10);
                    recoverBtn = CreateButton(signupBtn.transform.parent, "RecoverButton", "아이디 / 비밀번호 찾기", pos, signupBtn.GetComponent<RectTransform>().sizeDelta, Color.clear, Color.gray);
                    recoverBtnGo = recoverBtn.gameObject;
                }
                else
                {
                    recoverBtn = recoveryCandidates[0];
                    recoverBtnGo = recoverBtn.gameObject;
                    recoverBtnGo.name = "RecoverButton"; // Normalize name
                    
                    // Destroy excess duplicates
                    for (int i = 1; i < recoveryCandidates.Count; i++)
                    {
                        DestroyImmediate(recoveryCandidates[i].gameObject);
                    }
                }

                // ALWAYS SYNC LAYOUT: Match everything to signup button to prevent overlap
                RectTransform signupRect = signupBtn.GetComponent<RectTransform>();
                RectTransform recoverRect = recoverBtnGo.GetComponent<RectTransform>();
                recoverRect.anchorMin = signupRect.anchorMin;
                recoverRect.anchorMax = signupRect.anchorMax;
                recoverRect.pivot = signupRect.pivot;
                recoverRect.sizeDelta = signupRect.sizeDelta;
                
                // Position RecoverButton exactly below SignupBtn with a 30px gap
                // Formula: signupBtn center Y - signupBtn half-height - gap - recoverBtn half-height
                float gap = 30f;
                float signupCenterY = signupRect.anchoredPosition.y;
                float signupHalfH  = signupRect.sizeDelta.y * 0.5f;
                float recoverHalfH = recoverRect.sizeDelta.y * 0.5f;
                recoverRect.anchoredPosition = new Vector2(signupRect.anchoredPosition.x,
                    signupCenterY - signupHalfH - gap - recoverHalfH);

                // CRITICAL FIX: Unity UI - higher sibling index = drawn on top = raycast priority
                // SignupBtn MUST be the LAST sibling so it wins over RecoverButton
                signupBtn.transform.SetAsLastSibling();

                // Ensure it has navigation
                AddNavigationToButton(recoverBtn, "IdPwdRecoverScene");
                
                Debug.Log($"Signup sibling: {signupBtn.transform.GetSiblingIndex()}, Recover sibling: {recoverBtnGo.transform.GetSiblingIndex()}. Gap applied: {gap}px.");
            }

            // 3. STATIC BINDING & CLEANUP
            LoginController controller = FindObjectOfType<LoginController>();
            if (controller == null)
            {
                GameObject panel = GameObject.Find("LoginPanel") ?? canvas.gameObject;
                controller = panel.AddComponent<LoginController>();
                Debug.Log("Added missing LoginController to TitleScene.");
            }

            if (controller != null)
            {
                controller.loginIdInput = FindFieldInScene("ID", "Id", "아이디", "User");
                controller.loginPwInput = FindFieldInScene("Password", "Pw", "비밀번호", "Pass");
                controller.loginBtn = FindButtonInScene("로그인", "LoginBtn");
                
                // signupBtn already found above — reuse it directly
                if (signupBtn != null) 
                {
                    controller.toSignupBtn = signupBtn;
                    AddNavigationToButton(signupBtn, "IdPwdCreateScene");
                    Debug.Log($"Signup navigation assigned to: {signupBtn.name}");
                }

                // Find recover button by EXACT GameObject name only — never by keyword to avoid overlap
                GameObject recoverGo = GameObject.Find("RecoverButton");
                if (recoverGo != null)
                {
                    Button? exactRecoverBtn = recoverGo.GetComponent<Button>();
                    if (exactRecoverBtn != null)
                    {
                        controller.toRecoverBtn = exactRecoverBtn;
                        AddNavigationToButton(exactRecoverBtn, "IdPwdRecoverScene");
                        Debug.Log($"Recover navigation assigned to: {recoverGo.name}");
                    }
                }
                
                controller.statusText = FindObjectOfType<Text>();

                // CRITICAL: Disable Raycast Target for non-interactive images to prevent blocking clicks
                foreach (var img in canvas.GetComponentsInChildren<Image>())
                {
                    if (img.GetComponent<Button>() == null && img.GetComponent<InputField>() == null)
                    {
                        img.raycastTarget = false; 
                    }
                }

                EditorUtility.SetDirty(controller);
                Debug.Log("TitleScene References Bound and Raycast Blockers Cleaned!");
            }

            EditorSceneManager.SaveScene(scene);
            Debug.Log("TitleScene Polished Successfully!");
        }

        private static void SetupSignupScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Canvas canvas = GetOrCreateCanvas();
            EnsureEventSystem();
            
            GameObject go = new GameObject("SignupController");
            SignupController controller = go.AddComponent<SignupController>();

            GameObject panel = GetOrCreatePanel(canvas.transform, "MainPanel", new Color(0.96f, 0.96f, 0.96f, 1f));
            CreateText(panel.transform, "Signature", "RE-WRITE", new Vector2(-350, 100), 60, Color.black, TextAnchor.MiddleLeft);
            CreateText(panel.transform, "SubTitle", "계정 생성", new Vector2(-350, 40), 20, new Color(0.4f, 0.4f, 0.4f), TextAnchor.MiddleLeft);

            GameObject formBg = GetOrCreatePanel(panel.transform, "FormBg", new Color(1, 1, 1, 1));
            RectTransform formRect = formBg.GetComponent<RectTransform>();
            formRect.anchorMin = new Vector2(0.6f, 0.2f); formRect.anchorMax = new Vector2(0.9f, 0.8f);
            formRect.offsetMin = formRect.offsetMax = Vector2.zero;

            controller.signupIdInput = CreateInputField(formBg.transform, "SignUp_ID", "아이디", new Vector2(0, 90));
            controller.signupPwInput = CreateInputField(formBg.transform, "SignUp_PW", "비밀번호", new Vector2(0, 35), true);
            controller.signupNicknameInput = CreateInputField(formBg.transform, "SignUp_Nick", "닉네임", new Vector2(0, -20));
            controller.submitBtn = CreateButton(formBg.transform, "Submit_Btn", "계정 생성하기", new Vector2(0, -90), new Vector2(200, 45), Color.black, Color.white);
            
            // Restore Back Button with Explicit Navigation
            controller.backBtn = CreateButton(formBg.transform, "Back_Btn", "< 로그인 화면으로", new Vector2(0, -150), new Vector2(150, 30), Color.clear, Color.black);
            AddNavigationToButton(controller.backBtn, "TitleScene");
            
            // Adjust status text position to avoid overlap
            controller.statusText = CreateText(panel.transform, "StatusText", "", new Vector2(0, -220), 16, Color.red);

            EditorSceneManager.SaveScene(scene, SIGNUP_SCENE);
        }

        private static void SetupRecoverScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Canvas canvas = GetOrCreateCanvas();
            EnsureEventSystem();
            
            GameObject go = new GameObject("RecoverController");
            RecoverController controller = go.AddComponent<RecoverController>();

            GameObject panel = GetOrCreatePanel(canvas.transform, "MainPanel", new Color(0.96f, 0.96f, 0.96f, 1f));
            CreateText(panel.transform, "Signature", "RE-WRITE", new Vector2(-350, 100), 60, Color.black, TextAnchor.MiddleLeft);
            CreateText(panel.transform, "SubTitle", "계정 찾기", new Vector2(-350, 40), 20, new Color(0.4f, 0.4f, 0.4f), TextAnchor.MiddleLeft);

            GameObject formBg = GetOrCreatePanel(panel.transform, "FormBg", new Color(1, 1, 1, 1));
            RectTransform formRect = formBg.GetComponent<RectTransform>();
            formRect.anchorMin = new Vector2(0.6f, 0.3f); formRect.anchorMax = new Vector2(0.9f, 0.7f);
            formRect.offsetMin = formRect.offsetMax = Vector2.zero;

            controller.nicknameInput = CreateInputField(formBg.transform, "Recover_Nick", "닉네임", new Vector2(0, 30));
            controller.submitBtn = CreateButton(formBg.transform, "Submit_Btn", "계정 찾기", new Vector2(0, -30), new Vector2(200, 45), Color.black, Color.white);
            controller.backBtn = CreateButton(formBg.transform, "Back_Btn", "< 로그인 화면으로", new Vector2(0, -90), new Vector2(150, 30), Color.clear, Color.black);
            AddNavigationToButton(controller.backBtn, "TitleScene");
            controller.statusText = CreateText(panel.transform, "StatusText", "", new Vector2(0, -250), 16, Color.black);

            EditorSceneManager.SaveScene(scene, RECOVER_SCENE);
        }

        private static InputField? FindFieldInScene(params string[] keywords)
        {
            foreach (var input in FindObjectsOfType<InputField>())
            {
                foreach (var k in keywords)
                {
                    if (input.name.ToLower().Contains(k.ToLower())) return input;
                    Text p = input.placeholder as Text;
                    if (p != null && p.text.ToLower().Contains(k.ToLower())) return input;
                }
            }
            return null;
        }

        private static Button? FindButtonInScene(params string[] keywords)
        {
            foreach (var btn in FindObjectsOfType<Button>())
            {
                foreach (var k in keywords)
                {
                    if (btn.name.ToLower().Contains(k.ToLower())) return btn;
                    Text t = btn.GetComponentInChildren<Text>();
                    if (t != null && t.text.ToLower().Contains(k.ToLower())) return btn;
                }
            }
            return null;
        }

        private static void AddScenesToBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            string[] paths = { TITLE_SCENE, SIGNUP_SCENE, RECOVER_SCENE };
            foreach (var path in paths) if (scenes.All(s => s.path != path)) scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void AddNavigationToButton(Button btn, string sceneName)
        {
            SceneLoadTrigger trigger = btn.gameObject.GetComponent<SceneLoadTrigger>() ?? btn.gameObject.AddComponent<SceneLoadTrigger>();
            
            // Clear existing persistent listeners to avoid duplication
            while (btn.onClick.GetPersistentEventCount() > 0)
            {
                UnityEventTools.RemovePersistentListener(btn.onClick, 0);
            }

            // Explicitly add the LoadScene call
            UnityEventTools.AddStringPersistentListener(btn.onClick, trigger.LoadScene, sceneName);
            EditorUtility.SetDirty(btn.gameObject);
        }

        #region Helpers
        private static void EnsureEventSystem()
        {
            var es = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (es == null) es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem)).GetComponent<UnityEngine.EventSystems.EventSystem>();
            var oldModule = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (oldModule != null) DestroyImmediate(oldModule);
            if (es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null) es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        private static Canvas GetOrCreateCanvas()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject go = new GameObject("Canvas");
                canvas = go.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                go.AddComponent<CanvasScaler>();
                go.AddComponent<GraphicRaycaster>();
            }
            return canvas;
        }

        private static GameObject GetOrCreatePanel(Transform parent, string name, Color color)
        {
            Transform t = parent.Find(name);
            GameObject go = t ? t.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private static InputField CreateInputField(Transform parent, string name, string hint, Vector2 pos, bool isPw = false)
        {
            GameObject borderGo = new GameObject(name + "_Border", typeof(RectTransform), typeof(Image));
            borderGo.transform.SetParent(parent, false);
            RectTransform borderRect = borderGo.GetComponent<RectTransform>();
            borderRect.sizeDelta = new Vector2(252, 42); borderRect.anchoredPosition = pos;
            borderGo.GetComponent<Image>().color = new Color(0.85f, 0.85f, 0.85f, 1f);
            GameObject inputRoot = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
            inputRoot.transform.SetParent(borderGo.transform, false);
            RectTransform inputRect = inputRoot.GetComponent<RectTransform>();
            inputRect.sizeDelta = new Vector2(250, 40); inputRect.anchoredPosition = Vector2.zero;
            inputRoot.GetComponent<Image>().color = Color.white;
            GameObject placeholderGo = new GameObject("Placeholder", typeof(RectTransform), typeof(Text));
            placeholderGo.transform.SetParent(inputRoot.transform, false);
            Text pText = placeholderGo.GetComponent<Text>(); pText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            pText.text = hint; pText.color = new Color(0.7f, 0.7f, 0.7f, 1f); pText.alignment = TextAnchor.MiddleLeft;
            pText.rectTransform.anchorMin = Vector2.zero; pText.rectTransform.anchorMax = Vector2.one;
            pText.rectTransform.offsetMin = new Vector2(10, 0); pText.rectTransform.offsetMax = new Vector2(-10, 0);
            GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(inputRoot.transform, false);
            Text mText = textGo.GetComponent<Text>(); mText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            mText.color = Color.black; mText.alignment = TextAnchor.MiddleLeft;
            mText.rectTransform.anchorMin = Vector2.zero; mText.rectTransform.anchorMax = Vector2.one;
            mText.rectTransform.offsetMin = new Vector2(10, 0); mText.rectTransform.offsetMax = new Vector2(-10, 0);
            InputField input = inputRoot.GetComponent<InputField>(); input.textComponent = mText; input.placeholder = pText;
            if (isPw) input.contentType = InputField.ContentType.Password;
            return input;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2? size = null, Color? bgColor = null, Color? textColor = null)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = size ?? new Vector2(120, 40); rect.anchoredPosition = pos;
            go.GetComponent<Image>().color = bgColor ?? Color.white;
            GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            Text t = textGo.GetComponent<Text>(); t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.text = label; t.color = textColor ?? Color.black; t.alignment = TextAnchor.MiddleCenter;
            t.rectTransform.anchorMin = Vector2.zero; t.rectTransform.anchorMax = Vector2.one;
            return go.GetComponent<Button>();
        }

        private static Text CreateText(Transform parent, string name, string content, Vector2 pos, int fontSize = 18, Color? color = null, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text t = go.GetComponent<Text>(); t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.text = content; t.fontSize = fontSize; t.color = color ?? Color.black; t.alignment = anchor;
            t.rectTransform.anchoredPosition = pos; t.rectTransform.sizeDelta = new Vector2(500, 100);
            return t;
        }
        #endregion
    }
}
