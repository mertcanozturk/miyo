using Cysharp.Threading.Tasks;
using Miyo.Core;
using Miyo.UI.MVVM;

namespace Miyo.UI.Screens
{
    public class ParentRegisterViewModel : ViewModelBase
    {
        public ReactiveProperty<string> Email { get; } = new("");
        public ReactiveProperty<string> Password { get; } = new("");
        public ReactiveProperty<string> ErrorMessage { get; } = new("");
        public ReactiveProperty<bool> IsErrorVisible { get; } = new(false);
        public ReactiveProperty<bool> CanSubmit { get; } = new(false);
        public ReactiveProperty<bool> IsLoading { get; } = new(false);

        protected override void Initialize()
        {
            Email.Subscribe(_ => ValidateForm(), invokeImmediately: false).AddTo(Disposables);
            Password.Subscribe(_ => ValidateForm(), invokeImmediately: false).AddTo(Disposables);
        }

        private void ValidateForm()
        {
            bool hasEmail = !string.IsNullOrEmpty(Email.Value);
            bool hasPassword = !string.IsNullOrEmpty(Password.Value);

            CanSubmit.Value = hasEmail && hasPassword;

            if (hasPassword)
            {
                ErrorMessage.Value = "Şifre en az 4 karakterli olmalıdır.";
                IsErrorVisible.Value = true;
            }
            else
            {
                IsErrorVisible.Value = false;
            }
        }

        public async void OnRegisterClicked()
        {
            if (!CanSubmit.Value || IsLoading.Value) return;

            IsLoading.Value = true;
            IsErrorVisible.Value = false;

            // TODO: Gerçek auth service entegrasyonu
            // var authService = ServiceLocator.Get<IAuthService>();
            // var result = await authService.Register(Email.Value, Password.Value);

            await Cysharp.Threading.Tasks.UniTask.Delay(1000); // Simülasyon

            bool success = false; // Placeholder
            if (success)
            {
                var nav = ServiceLocator.Get<INavigationService>();
                await nav.ClearAndNavigateTo<ParentLoginViewModel>();
            }
            else
            {
                ErrorMessage.Value = "Kayıt işlemi başarısız oldu.";
                IsErrorVisible.Value = true;
            }

            IsLoading.Value = false;
        }

        public void OnBackToLoginClicked()
        {
            var nav = ServiceLocator.Get<INavigationService>();
            nav.GoBack().Forget();
        }
    }
}
