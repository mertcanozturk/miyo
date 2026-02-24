using TMPro;
using Miyo.UI.MVVM;
using UnityEngine;

namespace Miyo.UI
{
    public class ChildHomeView : ViewBase<ChildHomeViewModel>
    {
        [SerializeField] private TMP_Text _childNameText;
        [SerializeField] private UICollection<GameAppView> _gameAppViews;
        protected override void OnBind(ChildHomeViewModel vm)
        {
            vm.CurrentChild.Subscribe(child =>
            {
                if (child == null) return;
                _childNameText.text = child.Name;
            }).AddTo(Disposables);

            vm.Games.Subscribe(games =>
            {
                if (games == null) return;
                _gameAppViews.Count = games.Length;
                for (int i = 0; i < games.Length; i++)
                {
                    _gameAppViews[i].SetGame(games[i], vm.OnGamePlayButtonClicked);
                }
            }).AddTo(Disposables);

        }
    }
}
