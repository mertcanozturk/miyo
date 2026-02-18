using Miyo.UI.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Miyo.UI.Screens
{
    public class ParentLoginView : ViewBase<ParentLoginViewModel>
    {
        [Header("Input Fields")]
        [SerializeField] private AdvancedInputField _emailField;
        [SerializeField] private AdvancedInputField _passwordField;

        [Header("Buttons")]
        [SerializeField] private Button _loginButton;
        [SerializeField] private Button _registerButton;

        [Header("Feedback")]
        [SerializeField] private TMP_Text _errorText;
        [SerializeField] private GameObject _errorContainer;
        [SerializeField] private GameObject _loadingIndicator;

        protected override void OnBind(ParentLoginViewModel vm)
        {
            // Input → ViewModel
            _emailField.OnInputChanged += value => vm.Email.Value = value;
            _passwordField.OnInputChanged += value => vm.Password.Value = value;

            // ViewModel → UI
            vm.ErrorMessage.BindTo(_errorText).AddTo(Disposables);
            vm.IsErrorVisible.BindTo(_errorContainer).AddTo(Disposables);
            vm.CanSubmit.BindToInteractable(_loginButton).AddTo(Disposables);

            if (_loadingIndicator != null)
                vm.IsLoading.BindTo(_loadingIndicator).AddTo(Disposables);

            // Button actions
            _loginButton.BindClick(vm.OnLoginClicked).AddTo(Disposables);
            _registerButton.BindClick(vm.OnRegisterClicked).AddTo(Disposables);
        }
    }
}
