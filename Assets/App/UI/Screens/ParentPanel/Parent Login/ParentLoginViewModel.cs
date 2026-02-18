using Cysharp.Threading.Tasks;
using Miyo.Core;
using Miyo.UI.MVVM;

namespace Miyo.UI.Screens
{
    public class ParentLoginViewModel : ViewModelBase
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
            CanSubmit.Value = !string.IsNullOrEmpty(Email.Value) && !string.IsNullOrEmpty(Password.Value);
            IsErrorVisible.Value = false;
        }

        public async void OnLoginClicked()
        {
            if (!CanSubmit.Value || IsLoading.Value) return;

            IsLoading.Value = true;
            IsErrorVisible.Value = false;

            // TODO: Gerçek auth service entegrasyonu
            // var authService = ServiceLocator.Get<IAuthService>();
            // var result = await authService.Login(Email.Value, Password.Value);

            await Cysharp.Threading.Tasks.UniTask.Delay(1000); // Simülasyon

            bool success = false; // Placeholder
            if (success)
            {
                var nav = ServiceLocator.Get<INavigationService>();
                // await nav.ClearAndNavigateTo<HomeViewModel>();
            }
            else
            {
                ErrorMessage.Value = "E-posta veya şifre hatalı.";
                IsErrorVisible.Value = true;
            }

            IsLoading.Value = false;
        }

        public void OnRegisterClicked()
        {
            var nav = ServiceLocator.Get<INavigationService>();
            nav.NavigateTo<ParentRegisterViewModel>().Forget();
        }
    }
}
