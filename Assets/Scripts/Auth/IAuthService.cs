using System;
using System.Threading.Tasks;

namespace Auth
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(string id, string password);
        Task<AuthResult> SignupAsync(string id, string password, string nickname);
        Task<AuthResult> RecoverAccountAsync(string nickname);
        void Logout();
        bool IsLoggedIn();
        string GetCurrentUserId();
    }

    [Serializable]
    public class AuthResult
    {
        public bool Success;
        public string Message;
        public string Token;
        public string UserId;

        public static AuthResult Succeeded(string userId, string token = "") => new AuthResult 
        { 
            Success = true, 
            Message = "Success", 
            UserId = userId, 
            Token = token 
        };

        public static AuthResult Failed(string message) => new AuthResult 
        { 
            Success = false, 
            Message = message 
        };
    }
}
