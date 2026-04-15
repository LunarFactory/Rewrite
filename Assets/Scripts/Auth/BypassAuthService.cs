using System.Threading.Tasks;
using UnityEngine;

namespace Auth
{
    public class BypassAuthService : IAuthService
    {
        private string currentUserId;

        public async Task<AuthResult> LoginAsync(string id, string password)
        {
            // Simulate network latency
            await Task.Delay(500);

            // Bypass check: Any non-empty input is accepted
            if (string.IsNullOrEmpty(id))
            {
                return AuthResult.Failed("ID cannot be empty.");
            }

            currentUserId = id;
            Debug.Log($"[BypassAuth] Logged in as: {id}");
            return AuthResult.Succeeded(id, "bypass_token_" + id);
        }

        public async Task<AuthResult> SignupAsync(string id, string password, string email)
        {
            await Task.Delay(500);
            Debug.Log($"[BypassAuth] Signed up as ID: {id}, Email: {email}");
            return AuthResult.Succeeded(id, "bypass_token_" + id);
        }

        public async Task<AuthResult> RecoverAccountAsync(string email)
        {
            await Task.Delay(500);
            if (string.IsNullOrEmpty(email))
            {
                return AuthResult.Failed("Email cannot be empty.");
            }

            Debug.Log($"[BypassAuth] Account recovery requested for Email: {email}");
            
            // In Bypass mode, we just return a simulated account info
            string recoveredId = $"user_{email}";
            string recoveredPw = "demo1234";
            
            return AuthResult.Succeeded(recoveredId, $"Your ID: {recoveredId}\nPassword: {recoveredPw}");
        }

        public void Logout()
        {
            currentUserId = null;
        }

        public bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(currentUserId);
        }

        public string GetCurrentUserId()
        {
            return currentUserId;
        }
    }
}
