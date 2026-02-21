using Cysharp.Threading.Tasks;

namespace Miyo.Services.Auth
{
    public interface IAuthService
    {
        bool IsLoggedIn { get; }
        string CurrentUserEmail { get; }
        string PlayerId { get; }
        string PlayerName { get; }

        UniTask InitializeAsync();
        UniTask<AuthResult> Login(string email, string password);
        UniTask<AuthResult> Register(string name, string email, string password);
        UniTask<bool> VerifyPinAsync(string pin);
        void Logout();
    }
}
