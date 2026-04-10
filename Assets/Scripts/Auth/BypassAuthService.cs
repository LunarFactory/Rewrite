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

        public async Task<AuthResult> SignupAsync(string id, string password)
        {
            await Task.Delay(500);
            Debug.Log($"[BypassAuth] Signed up as: {id}");
            return AuthResult.Succeeded(id, "bypass_token_" + id);
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
