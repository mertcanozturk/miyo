using Cysharp.Threading.Tasks;
using Miyo.Core;
using Miyo.Services.Auth;
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

            var authService = ServiceLocator.Get<IAuthService>();
            var result = await authService.Login(Email.Value, Password.Value);

            if (result.Success)
            {
                var nav = ServiceLocator.Get<INavigationService>();
                await AuthNavigationHelper.NavigateAfterAuth(authService.PlayerId, nav);
            }
            else
            {
                ErrorMessage.Value = result.ErrorMessage;
                IsErrorVisible.Value = true;
            }

            IsLoading.Value = false;
        }

        public void OnRegisterClicked()
        {
            var nav = ServiceLocator.Get<INavigationService>();
            nav.GoBack().Forget();
        }
    }
}
