using Miyo.UI.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Miyo.UI.Screens
{
    public class ParentRegisterView : ViewBase<ParentRegisterViewModel>
    {
        [Header("Input Fields")]
        [SerializeField] private AdvancedInputField _nameField;
        [SerializeField] private AdvancedInputField _emailField;
        [SerializeField] private AdvancedInputField _passwordField;
        [SerializeField] private AdvancedInputField _confirmPasswordField;

        [Header("Buttons")]

        [SerializeField] private Button _registerButton;
        [SerializeField] private Button _backButton;

        [Header("Feedback")]
        [SerializeField] private TMP_Text _errorText;
        [SerializeField] private GameObject _errorContainer;
        [SerializeField] private GameObject _loadingIndicator;

        protected override void OnBind(ParentRegisterViewModel vm)
        {
            // Input → ViewModel
            _nameField.OnInputChanged += value => vm.Name.Value = value;
            _emailField.OnInputChanged += value => vm.Email.Value = value;
            _passwordField.OnInputChanged += value => vm.Password.Value = value;
            _confirmPasswordField.OnInputChanged += value => vm.ConfirmPassword.Value = value;
            // ViewModel → UI
            vm.ErrorMessage.BindTo(_errorText).AddTo(Disposables);
            vm.IsErrorVisible.BindTo(_errorContainer).AddTo(Disposables);
            vm.CanSubmit.BindToInteractable(_registerButton).AddTo(Disposables);

            if (_loadingIndicator != null)
                vm.IsLoading.BindTo(_loadingIndicator).AddTo(Disposables);

            // Button actions
            _registerButton.BindClick(vm.OnRegisterClicked).AddTo(Disposables);
            _backButton.BindClick(vm.OnBackToLoginClicked).AddTo(Disposables);
        }
    }
}
