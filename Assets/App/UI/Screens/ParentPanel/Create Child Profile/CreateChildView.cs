using Miyo.UI.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Miyo.UI.Screens
{
    public class CreateChildView : ViewBase<CreateChildViewModel>
    {
        [Header("Input Fields")]
        [SerializeField] private AdvancedInputField _nameField;
        [SerializeField] private DateTimeInputField _birthDateField;

        [Header("Time Limits")]
        [SerializeField] private Slider _weekdayLimitSlider;
        [SerializeField] private TMP_Text _weekdayLimitText;
        [SerializeField] private Slider _weekendLimitSlider;
        [SerializeField] private TMP_Text _weekendLimitText;

        [Header("Buttons")]
        [SerializeField] private Button _createButton;

        [Header("Feedback")]
        [SerializeField] private GameObject _loadingIndicator;

        protected override void OnBind(CreateChildViewModel vm)
        {
            // Input → ViewModel
            _nameField.OnInputChanged += value => vm.ChildName.Value = value;

            // DateTime two-way binding
            vm.BirthDate.BindTwoWay(_birthDateField).AddTo(Disposables);

            // Slider min/max setup
            _weekdayLimitSlider.minValue = CreateChildViewModel.MinLimit;
            _weekdayLimitSlider.maxValue = CreateChildViewModel.MaxLimit;
            _weekdayLimitSlider.wholeNumbers = true;
            _weekendLimitSlider.minValue = CreateChildViewModel.MinLimit;
            _weekendLimitSlider.maxValue = CreateChildViewModel.MaxLimit;
            _weekendLimitSlider.wholeNumbers = true;

            // Slider two-way bindings
            vm.WeekdayLimit.BindTwoWay(_weekdayLimitSlider).AddTo(Disposables);
            vm.WeekendLimit.BindTwoWay(_weekendLimitSlider).AddTo(Disposables);

            // ViewModel → UI
            vm.WeekdayLimitText.BindTo(_weekdayLimitText).AddTo(Disposables);
            vm.WeekendLimitText.BindTo(_weekendLimitText).AddTo(Disposables);
            vm.CanSubmit.BindToInteractable(_createButton).AddTo(Disposables);

            if (_loadingIndicator != null)
                vm.IsLoading.BindTo(_loadingIndicator).AddTo(Disposables);

            // Button actions
            _createButton.BindClick(vm.OnCreateClicked).AddTo(Disposables);
        }
    }
}
