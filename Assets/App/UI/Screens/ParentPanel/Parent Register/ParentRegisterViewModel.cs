using Cysharp.Threading.Tasks;
using Miyo.Core;
using Miyo.Services.Auth;
using Miyo.UI.MVVM;
using UnityEngine;

namespace Miyo.UI.Screens
{
    public class ParentRegisterViewModel : ViewModelBase
    {
        public ReactiveProperty<string> Name { get; } = new("");
        public ReactiveProperty<string> Email { get; } = new("");
        public ReactiveProperty<string> Password { get; } = new("");
        public ReactiveProperty<string> ErrorMessage { get; } = new("");
        public ReactiveProperty<bool> IsErrorVisible { get; } = new(false);
        public ReactiveProperty<bool> CanSubmit { get; } = new(false);
        public ReactiveProperty<bool> IsLoading { get; } = new(false);
        public ReactiveProperty<string> ConfirmPassword { get; } = new("");

        protected override void Initialize()
        {
            Name.Subscribe(_ => ValidateForm(), invokeImmediately: false).AddTo(Disposables);
            Email.Subscribe(_ => ValidateForm(), invokeImmediately: false).AddTo(Disposables);
            Password.Subscribe(_ => ValidateForm(), invokeImmediately: false).AddTo(Disposables);
            ConfirmPassword.Subscribe(_ => ValidateForm(), invokeImmediately: false).AddTo(Disposables);
        }

        private void ValidateForm()
        {
            bool hasName = !string.IsNullOrEmpty(Name.Value);
            bool hasEmail = !string.IsNullOrEmpty(Email.Value);
            bool hasPin = !string.IsNullOrEmpty(Password.Value);
            bool isPinValid = hasPin && Password.Value.Length == 6 && IsAllDigits(Password.Value);
            bool pinsMatch = hasPin && ConfirmPassword.Value == Password.Value;

            CanSubmit.Value = hasName && hasEmail && isPinValid && pinsMatch;

            if (hasPin && !isPinValid)
            {
                ErrorMessage.Value = "PIN 6 haneli rakam olmalıdır.";
                IsErrorVisible.Value = true;
            }
            else if (hasPin && !pinsMatch)
            {
                ErrorMessage.Value = "PIN'ler eşleşmiyor.";
                IsErrorVisible.Value = true;
            }
            else
            {
                IsErrorVisible.Value = false;
            }
        }

        private static bool IsAllDigits(string value)
        {
            foreach (char c in value)
                if (!char.IsDigit(c)) return false;
            return true;
        }

        public async void OnRegisterClicked()
        {
            if (!CanSubmit.Value || IsLoading.Value)
            {
                Debug.Log($"[Register] Blocked — CanSubmit: {CanSubmit.Value}, IsLoading: {IsLoading.Value}");
                return;
            }

            Debug.Log($"[Register] Starting registration for: {Email.Value}");
            IsLoading.Value = true;
            IsErrorVisible.Value = false;

            try
            {
                var authService = ServiceLocator.Get<IAuthService>();
                var result = await authService.Register(Name.Value, Email.Value, Password.Value);

                if (result.Success)
                {
                    Debug.Log("[Register] Success! Navigating...");
                    var nav = ServiceLocator.Get<INavigationService>();
                    await AuthNavigationHelper.NavigateAfterAuth(authService.PlayerId, nav);
                }
                else
                {
                    Debug.LogWarning($"[Register] Failed: {result.ErrorMessage}");
                    ErrorMessage.Value = result.ErrorMessage;
                    IsErrorVisible.Value = true;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Register] Exception: {ex}");
                ErrorMessage.Value = $"Beklenmeyen hata: {ex.Message}";
                IsErrorVisible.Value = true;
            }

            IsLoading.Value = false;
        }

        public void OnBackToLoginClicked()
        {
            var nav = ServiceLocator.Get<INavigationService>();
            nav.NavigateTo<ParentLoginViewModel>().Forget();
        }
    }
}
