using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Miyo.UI
{
    public class AdvancedInputField : MonoBehaviour
    {
        [Header("Input Field")]
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Image _icon;


        [Header("Validation")]
        [SerializeField] private GameObject _validationError;
        [SerializeField] private GameObject _validationSuccess;
        [SerializeField] private bool _autoValidate = true;
        [SerializeField] private bool _showValidation = true;
        [SerializeField] private bool _optional = false;
        private IStringFieldValidator _validator;

        public string Value
        {
            get => _value;
        }

        public bool IsValid
        {
            get => _validationError.activeSelf;
        }

        public event Action<bool> OnValidationChanged;
        public event Action<string> OnInputChanged;

        private string _value;
        private bool _isValid;

        void Awake()
        {
            _inputField.onValueChanged.AddListener(OnValueChanged);
            _validator = GetComponent<IStringFieldValidator>();
            _validationError.SetActive(false);
            _validationSuccess.SetActive(false);
            _isValid = _optional;
            _value = "";
        }

        void OnValueChanged(string value)
        {
            _value = value;

            bool isValid;

            if (_autoValidate && _validator != null)
            {
                isValid = _validator.Validate(value);
            }
            else
            {
                isValid = true;
            }

            bool changed = isValid != _isValid;
            _isValid = isValid;

            OnInputChanged?.Invoke(value);
            OnValidationChanged?.Invoke(_isValid);
            UpdateValidationUI();
        }

        public void Validate()
        {
            bool isValid = _validator.Validate(_value);
            if (isValid != _isValid)
            {
                _isValid = isValid;
                OnValidationChanged?.Invoke(_isValid);
            }
            UpdateValidationUI();
        }

        private void UpdateValidationUI()
        {
            _validationError.SetActive(!_isValid && _showValidation);
            _validationSuccess.SetActive(_isValid && _showValidation);
        }

    }

}
