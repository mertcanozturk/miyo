using System;
using Miyo.Core;
using Miyo.Services.DateTimePicker;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Miyo.UI
{
    public class DateTimeInputField : MonoBehaviour, IPointerClickHandler
    {
        [Header("Input Field")]
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Image _icon;
        [SerializeField] private string _placeholder = "Select...";

        [Header("Picker Settings")]
        [SerializeField] private DateTimePickerMode _pickerMode = DateTimePickerMode.DateOnly;
        [SerializeField] private string _dateFormat = "dd/MM/yyyy";
        [SerializeField] private string _timeFormat = "HH:mm";
        [SerializeField] private string _dateTimeFormat = "dd/MM/yyyy HH:mm";

        [Header("Validation")]
        [SerializeField] private GameObject _validationError;
        [SerializeField] private GameObject _validationSuccess;
        [SerializeField] private bool _showValidation = true;
        [SerializeField] private bool _optional = false;

        public DateTime? Value => _value;
        public bool IsValid => _isValid;
        public DateTimePickerMode PickerMode
        {
            get => _pickerMode;
            set => _pickerMode = value;
        }

        public event Action<DateTime?> OnValueChanged;
        public event Action<bool> OnValidationChanged;

        private DateTime? _value;
        private bool _isValid;
        private DateTime? _minDate;
        private DateTime? _maxDate;
        private INativeDateTimePicker _picker;
        private bool _pickerOpen;

        private void Awake()
        {
            _inputField.readOnly = true;
            _inputField.onSelect.AddListener(OnInputSelected);

            if (_inputField.placeholder is TMP_Text placeholderText)
                placeholderText.text = _placeholder;

            _isValid = _optional;
            UpdateDisplay();
            UpdateValidationUI();
        }

        private void OnDestroy()
        {
            _inputField.onSelect.RemoveListener(OnInputSelected);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OpenPicker();
        }

        private void OnInputSelected(string _)
        {
            OpenPicker();
        }

        public void SetMinDate(DateTime? min)
        {
            _minDate = min;
        }

        public void SetMaxDate(DateTime? max)
        {
            _maxDate = max;
        }

        public void SetValue(DateTime? value)
        {
            if (_value == value) return;
            _value = value;
            UpdateDisplay();
            Validate();
            OnValueChanged?.Invoke(_value);
        }

        public void SetValueWithoutNotify(DateTime? value)
        {
            _value = value;
            UpdateDisplay();
            UpdateValidationUI();
        }

        public void Clear()
        {
            SetValue(null);
        }

        public void Validate()
        {
            bool wasValid = _isValid;
            _isValid = _optional ? !_value.HasValue || IsWithinRange(_value.Value)
                                 : _value.HasValue && IsWithinRange(_value.Value);

            UpdateValidationUI();

            if (wasValid != _isValid)
                OnValidationChanged?.Invoke(_isValid);
        }

        private bool IsWithinRange(DateTime dt)
        {
            if (_minDate.HasValue && dt < _minDate.Value) return false;
            if (_maxDate.HasValue && dt > _maxDate.Value) return false;
            return true;
        }

        private async void OpenPicker()
        {
            if (_pickerOpen) return;
            _pickerOpen = true;

            try
            {
                _picker ??= ServiceLocator.Get<INativeDateTimePicker>();

                var initial = _value ?? DateTime.Now;
                var result = await _picker.ShowPicker(_pickerMode, initial, _minDate, _maxDate);

                if (!result.WasCancelled)
                    SetValue(result.SelectedDateTime);
            }
            finally
            {
                _pickerOpen = false;
                _inputField.DeactivateInputField();
            }
        }

        private void UpdateDisplay()
        {
            if (_inputField == null) return;

            if (!_value.HasValue)
            {
                _inputField.SetTextWithoutNotify("");
                return;
            }

            var formatted = _pickerMode switch
            {
                DateTimePickerMode.DateOnly => _value.Value.ToString(_dateFormat),
                DateTimePickerMode.TimeOnly => _value.Value.ToString(_timeFormat),
                DateTimePickerMode.DateTime => _value.Value.ToString(_dateTimeFormat),
                _ => _value.Value.ToString(_dateFormat)
            };

            _inputField.SetTextWithoutNotify(formatted);
        }

        private void UpdateValidationUI()
        {
            if (_validationError != null)
                _validationError.SetActive(!_isValid && _showValidation && _value.HasValue);
            if (_validationSuccess != null)
                _validationSuccess.SetActive(_isValid && _showValidation && _value.HasValue);
        }
    }
}
