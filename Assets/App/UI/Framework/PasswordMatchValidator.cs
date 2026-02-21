using UnityEngine;
using TMPro;
namespace Miyo.UI
{
    public class PasswordMatchValidator : MonoBehaviour, IStringFieldValidator
    {
        [SerializeField] private AdvancedInputField _passwordInputField;
        [SerializeField] private TMP_Text _errorText;
        [SerializeField] private string _errorMessage;

        void Awake()
        {
            _errorText.text = string.Empty;
            _passwordInputField.OnInputChanged += OnPasswordInputChanged;
        }

        private void OnPasswordInputChanged(string value)
        {
            Validate(value);
        }

        public bool Validate(string value)
        {
            if (value != _passwordInputField.Value)
            {
                _errorText.text = _errorMessage;
                return false;
            }
            _errorText.text = "";
            return true;
        }
    }
}
