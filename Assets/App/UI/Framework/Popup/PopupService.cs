using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Miyo.Core;
using UnityEngine;

namespace Miyo.UI.MVVM
{
    public class PopupService : MonoBehaviour, IPopupService
    {
        [SerializeField] private ScreenRegistry _popupRegistry;
        [SerializeField] private Transform _popupContainer;
        [SerializeField] private GameObject _dimBackground;

        private readonly struct PopupInstance
        {
            public readonly GameObject GameObject;
            public readonly ViewModelBase ViewModel;

            public PopupInstance(GameObject gameObject, ViewModelBase viewModel)
            {
                GameObject = gameObject;
                ViewModel = viewModel;
            }
        }

        private readonly Stack<PopupInstance> _popupStack = new();

        public bool HasActivePopup => _popupStack.Count > 0;

        private void Awake()
        {
            ServiceLocator.Register<IPopupService>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<IPopupService>();
        }

        public async UniTask ShowPopup<TViewModel>(Action<TViewModel> configure = null)
            where TViewModel : ViewModelBase, new()
        {
            await ShowPopupInternal<TViewModel>(configure);
        }

        public async UniTask<TResult> ShowPopup<TViewModel, TResult>(Action<TViewModel> configure = null)
            where TViewModel : PopupViewModel<TResult>, new()
        {
            var (_, viewModel) = await ShowPopupInternal<TViewModel>(configure);
            return await viewModel.Result;
        }

        private async UniTask<(PopupInstance instance, TViewModel viewModel)> ShowPopupInternal<TViewModel>(
            Action<TViewModel> configure)
            where TViewModel : ViewModelBase, new()
        {
            var screenId = ViewModelIdHelper.GetId<TViewModel>();

            var prefab = _popupRegistry.GetPrefab(screenId);
            if (prefab == null)
                throw new InvalidOperationException($"Popup prefab not found: {screenId}");

            var go = Instantiate(prefab, _popupContainer);
            var view = go.GetComponent<PopupViewBase<TViewModel>>();

            if (view == null)
            {
                Debug.LogError($"[PopupService] PopupViewBase<{typeof(TViewModel).Name}> not found on prefab: {screenId}");
                Destroy(go);
                throw new InvalidOperationException($"PopupViewBase<{typeof(TViewModel).Name}> not found");
            }

            var viewModel = new TViewModel();
            configure?.Invoke(viewModel);
            view.Bind(viewModel);

            view.OnCloseRequested += () => ClosePopup(go);

            var instance = new PopupInstance(go, viewModel);
            _popupStack.Push(instance);
            UpdateDimBackground();

            go.SetActive(true);
            await view.AnimateIn();

            return (instance, viewModel);
        }

        public void CloseTop()
        {
            if (_popupStack.Count == 0) return;
            var top = _popupStack.Pop();
            DestroyPopup(top);
            UpdateDimBackground();
        }

        public void CloseAll()
        {
            while (_popupStack.Count > 0)
            {
                var popup = _popupStack.Pop();
                DestroyPopup(popup);
            }
            UpdateDimBackground();
        }

        private void ClosePopup(GameObject go)
        {
            if (_popupStack.Count == 0) return;

            var top = _popupStack.Peek();
            if (top.GameObject == go)
            {
                _popupStack.Pop();
                DestroyPopup(top);
                UpdateDimBackground();
            }
        }

        private void DestroyPopup(PopupInstance instance)
        {
            instance.ViewModel?.Dispose();
            if (instance.GameObject != null)
                Destroy(instance.GameObject);
        }

        private void UpdateDimBackground()
        {
            if (_dimBackground != null)
                _dimBackground.SetActive(_popupStack.Count > 0);
        }
    }
}
