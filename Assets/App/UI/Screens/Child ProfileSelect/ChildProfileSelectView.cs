
using Cysharp.Threading.Tasks;
using Miyo.UI.MVVM;
using UnityEngine;
using UnityEngine.UI;

namespace Miyo.UI.Screens
{
    public class ChildProfileSelectView : ViewBase<ChildProfileSelectViewModel>
    {
        [SerializeField] private ChildSelectorView _childSelector;
        [SerializeField] private Button _parentPanelButton;
        [SerializeField] private PinEntryConfig _pinEntryConfig;

        protected override void OnBind(ChildProfileSelectViewModel vm)
        {
            _childSelector.OnChildSelected += OnChildSelected;

            vm.Children.Subscribe(children =>
            {
                if (children == null) return;
                var selectedId = vm.SelectedChild.Value?.Id;
                _childSelector.SetChildren(children, selectedId, false);
            }).AddTo(Disposables);

            _parentPanelButton.BindClick(() => vm.OnParentPanelClicked(_pinEntryConfig))
                .AddTo(Disposables);
        }

        private void OnChildSelected(string childId)
        {
            ViewModel.SelectChild(childId);
        }
    }
}
