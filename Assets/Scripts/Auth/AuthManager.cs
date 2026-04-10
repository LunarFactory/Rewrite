using UnityEngine;
using System.Threading.Tasks;

namespace Auth
{
    public class AuthManager : MonoBehaviour
    {
        public static AuthManager Instance { get; private set; }

        private IAuthService authService;
        public bool IsLoggedIn => authService != null && authService.IsLoggedIn();
        public string CurrentUserId => authService?.GetCurrentUserId();

        [SerializeField] private bool useBypass = true; // For development

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeService();
        }

        private void InitializeService()
        {
            if (useBypass)
            {
                authService = new BypassAuthService();
            }
            else
            {
                // Future: Initialize CognitoAuthService
                // authService = new CognitoAuthService();
                authService = new BypassAuthService(); // Fallback
            }
        }

        public async Task<AuthResult> Login(string id, string password)
        {
            var result = await authService.LoginAsync(id, password);
            if (result.Success)
            {
                PlayerPrefs.SetString("LastSessionID", result.UserId);
                PlayerPrefs.Save();
            }
            return result;
        }

        public async Task<AuthResult> Signup(string id, string password)
        {
            return await authService.SignupAsync(id, password);
        }

        public void Logout()
        {
            authService.Logout();
            PlayerPrefs.DeleteKey("LastSessionID");
        }
    }
}
