using System;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.CloudSave;
using UnityEngine;

namespace Miyo.Services.Auth
{
    public class AuthService : IAuthService
    {
        private IAuthenticationService _auth;
        private string _cachedEmail;
        private string _cachedPinHash;

        public bool IsLoggedIn => _auth?.IsSignedIn ?? false;
        public string CurrentUserEmail => _cachedEmail;
        public string PlayerId => _auth?.PlayerId;
        public string PlayerName => _auth?.PlayerName;

        public async UniTask InitializeAsync()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
                Debug.Log("[AuthService] Unity Services initialized.");
            }

            _auth = AuthenticationService.Instance;

            // Restore existing session if token exists
            if (_auth.SessionTokenExists && !_auth.IsSignedIn)
            {
                try
                {
                    await _auth.SignInAnonymouslyAsync();
                    await LoadUserDataFromCloud();
                    Debug.Log($"[AuthService] Session restored for player: {_auth.PlayerId}");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[AuthService] Session restore failed: {ex.Message}");
                }
            }
        }

        public async UniTask<AuthResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return AuthResult.Failed("E-posta ve şifre boş olamaz.");

            var username = EmailToUsername(email);
            var complexPassword = PinToPassword(password);

            try
            {
                await _auth.SignInWithUsernamePasswordAsync(username, complexPassword);
                _cachedEmail = email;
                await LoadPinHashFromCloud();
                Debug.Log($"[AuthService] Login successful. PlayerId: {_auth.PlayerId}");
                return AuthResult.Successful();
            }
            catch (AuthenticationException ex)
            {
                Debug.LogWarning($"[AuthService] Login failed: {ex.Message}");
                return AuthResult.Failed(ParseAuthError(ex));
            }
            catch (RequestFailedException ex)
            {
                Debug.LogWarning($"[AuthService] Login request failed: {ex.Message}");
                return AuthResult.Failed("Bağlantı hatası. Lütfen internet bağlantınızı kontrol edin.");
            }
        }

        public async UniTask<AuthResult> Register(string name, string email, string password)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return AuthResult.Failed("Ad, e-posta ve şifre boş olamaz.");

            if (password.Length != 6)
                return AuthResult.Failed("PIN 6 haneli olmalıdır.");

            var username = EmailToUsername(email);
            var complexPassword = PinToPassword(password);

            try
            {
                await _auth.SignUpWithUsernamePasswordAsync(username, complexPassword);
                _cachedEmail = email;
                Debug.Log($"[AuthService] Registration successful. PlayerId: {_auth.PlayerId}");

                // Save real email and PIN hash to cloud save for session restore & PIN verification
                await SaveEmailToCloud(email);
                await SavePinHashToCloud(password);

                try
                {
                    await _auth.UpdatePlayerNameAsync(name);
                    Debug.Log($"[AuthService] Player name set to: {name}");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[AuthService] Failed to set player name: {ex.Message}");
                }

                return AuthResult.Successful();
            }
            catch (AuthenticationException ex)
            {
                Debug.LogWarning($"[AuthService] Registration failed: {ex.Message}");
                return AuthResult.Failed(ParseAuthError(ex));
            }
            catch (RequestFailedException ex)
            {
                Debug.LogWarning($"[AuthService] Registration request failed: {ex.Message}");
                return AuthResult.Failed("Bağlantı hatası. Lütfen internet bağlantınızı kontrol edin.");
            }
        }

        public async UniTask<bool> VerifyPinAsync(string pin)
        {
            if (string.IsNullOrEmpty(_cachedPinHash))
            {
                await LoadPinHashFromCloud();
            }

            if (string.IsNullOrEmpty(_cachedPinHash))
            {
                Debug.LogWarning("[AuthService] No PIN hash found. Cannot verify.");
                return false;
            }

            return HashPin(pin) == _cachedPinHash;
        }

        public void Logout()
        {
            _cachedEmail = null;
            _cachedPinHash = null;
            _auth.SignOut();
            _auth.ClearSessionToken();
            Debug.Log("[AuthService] User logged out and session cleared.");
        }

        private static string ParseAuthError(AuthenticationException ex)
        {
            var errorCode = ex.ErrorCode;

            if (errorCode == AuthenticationErrorCodes.InvalidParameters)
                return "Geçersiz e-posta veya şifre formatı.";

            if (errorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
                return "Bu e-posta adresi zaten kayıtlı.";

            return $"Kimlik doğrulama hatası: {ex.Message}";
        }

        // --- Helpers for Workarounds ---
        
        /// <summary>
        /// Converts an email into a deterministic 20-character string valid for UGS Username limitations.
        /// UGS limits username to 20 chars, but emails are longer.
        /// </summary>
        private static string EmailToUsername(string email)
        {
            using var md5 = MD5.Create();
            var inputBytes = Encoding.UTF8.GetBytes(email.Trim().ToLowerInvariant());
            var hashBytes = md5.ComputeHash(inputBytes);
            // Convert to hex string and take first 20 characters
            var sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
                sb.Append(hashBytes[i].ToString("x2"));
            
            return sb.ToString().Substring(0, 20);
        }

        /// <summary>
        /// Converts a simple PIN into a complex password that satisfies UGS requirements.
        /// UGS requires: Min 8 chars, 1 uppercase, 1 lowercase, 1 digit, 1 symbol.
        /// </summary>
        private static string PinToPassword(string pin)
        {
            // We append a static suffix to guarantee requirements are met:
            // "Aa1!" provides Uppercase (A), Lowercase (a), Digit (1), Symbol (!)
            // "Miyo" just pads it further in case the pin is short.
            return $"{pin}Aa1!Miyo";
        }

        private async UniTask SaveEmailToCloud(string email)
        {
            try
            {
                var data = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "RealEmail", email }
                };
                await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not save real email to cloud: {e.Message}");
            }
        }

        private async UniTask LoadUserDataFromCloud()
        {
            try
            {
                var keys = new System.Collections.Generic.HashSet<string> { "RealEmail", "PinHash" };
                var result = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

                if (result.TryGetValue("RealEmail", out var emailValue))
                    _cachedEmail = emailValue.Value.GetAs<string>();

                if (result.TryGetValue("PinHash", out var pinValue))
                    _cachedPinHash = pinValue.Value.GetAs<string>();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AuthService] Could not load user data from cloud: {e.Message}");
            }
        }

        private async UniTask LoadPinHashFromCloud()
        {
            try
            {
                var keys = new System.Collections.Generic.HashSet<string> { "PinHash" };
                var result = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

                if (result.TryGetValue("PinHash", out var value))
                    _cachedPinHash = value.Value.GetAs<string>();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AuthService] Could not load PIN hash from cloud: {e.Message}");
            }
        }

        private async UniTask SavePinHashToCloud(string pin)
        {
            try
            {
                var hash = HashPin(pin);
                _cachedPinHash = hash;

                var data = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "PinHash", hash }
                };
                await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AuthService] Could not save PIN hash to cloud: {e.Message}");
            }
        }

        private static string HashPin(string pin)
        {
            using var sha256 = SHA256.Create();
            var inputBytes = Encoding.UTF8.GetBytes(pin);
            var hashBytes = sha256.ComputeHash(inputBytes);
            var sb = new StringBuilder(64);
            for (int i = 0; i < hashBytes.Length; i++)
                sb.Append(hashBytes[i].ToString("x2"));
            return sb.ToString();
        }
    }
}
