using TMPro;
using Miyo.UI.MVVM;
using UnityEngine;

namespace Miyo.UI
{
    public class ChildHomeView : ViewBase<ChildHomeViewModel>
    {
        [SerializeField] private TMP_Text _childNameText;
        protected override void OnBind(ChildHomeViewModel vm)
        {
            vm.CurrentChild.Subscribe(child =>
            {
                if (child == null) return;
                _childNameText.text = child.Name;
            }).AddTo(Disposables);

        }
    }
}
