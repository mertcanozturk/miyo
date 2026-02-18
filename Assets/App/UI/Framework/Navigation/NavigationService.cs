using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Miyo.Core;
using UnityEngine;

namespace Miyo.UI.MVVM
{
    public class NavigationService : MonoBehaviour, INavigationService
    {
        [SerializeField] private ScreenRegistry _registry;
        [SerializeField] private Transform _screenContainer;

        private readonly struct ScreenInstance
        {
            public readonly string ScreenId;
            public readonly GameObject GameObject;
            public readonly ViewModelBase ViewModel;

            public ScreenInstance(string screenId, GameObject gameObject, ViewModelBase viewModel)
            {
                ScreenId = screenId;
                GameObject = gameObject;
                ViewModel = viewModel;
            }
        }

        private readonly Stack<ScreenInstance> _screenStack = new();
        private ScreenInstance? _currentScreen;
        private bool _isTransitioning;

        public string CurrentScreenId => _currentScreen?.ScreenId;
        public bool CanGoBack => _screenStack.Count > 0;
        public bool IsTransitioning => _isTransitioning;

        private void Awake()
        {
            ServiceLocator.Register<INavigationService>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<INavigationService>();
        }

        public async UniTask NavigateTo<TViewModel>(Action<TViewModel> configure = null)
            where TViewModel : ViewModelBase, new()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            try
            {
                if (_currentScreen.HasValue)
                {
                    var current = _currentScreen.Value;
                    await AnimateOutView(current.GameObject);
                    current.GameObject.SetActive(false);
                    _screenStack.Push(current);
                }

                _currentScreen = await CreateAndShowScreen<TViewModel>(configure);
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        public async UniTask GoBack()
        {
            if (_isTransitioning || _screenStack.Count == 0) return;
            _isTransitioning = true;

            try
            {
                if (_currentScreen.HasValue)
                {
                    var current = _currentScreen.Value;
                    await AnimateOutView(current.GameObject);
                    UnbindAndDestroy(current);
                }

                var previous = _screenStack.Pop();
                previous.GameObject.SetActive(true);
                _currentScreen = previous;

                await AnimateInView(previous.GameObject);
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        public async UniTask ClearAndNavigateTo<TViewModel>(Action<TViewModel> configure = null)
            where TViewModel : ViewModelBase, new()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            try
            {
                if (_currentScreen.HasValue)
                {
                    var current = _currentScreen.Value;
                    await AnimateOutView(current.GameObject);
                    UnbindAndDestroy(current);
                }

                while (_screenStack.Count > 0)
                    UnbindAndDestroy(_screenStack.Pop());

                _currentScreen = await CreateAndShowScreen<TViewModel>(configure);
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        private async UniTask<ScreenInstance> CreateAndShowScreen<TViewModel>(Action<TViewModel> configure)
            where TViewModel : ViewModelBase, new()
        {
            var screenId = ViewModelIdHelper.GetId<TViewModel>();
            var prefab = _registry.GetPrefab(screenId);
            if (prefab == null)
                throw new InvalidOperationException($"Screen prefab not found: {screenId}");

            var go = Instantiate(prefab, _screenContainer);
            var view = go.GetComponent<ViewBase<TViewModel>>();

            if (view == null)
            {
                Debug.LogError($"[NavigationService] ViewBase<{typeof(TViewModel).Name}> not found on prefab: {screenId}");
                Destroy(go);
                throw new InvalidOperationException($"ViewBase<{typeof(TViewModel).Name}> not found on prefab: {screenId}");
            }

            var viewModel = new TViewModel();
            configure?.Invoke(viewModel);
            view.Bind(viewModel);

            go.SetActive(true);
            await view.AnimateIn();

            return new ScreenInstance(screenId, go, viewModel);
        }

        private void UnbindAndDestroy(ScreenInstance instance)
        {
            instance.ViewModel?.Dispose();
            if (instance.GameObject != null)
                Destroy(instance.GameObject);
        }

        private async UniTask AnimateOutView(GameObject go)
        {
            var canvasGroup = go.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                await ViewAnimator.FadeOut(canvasGroup);
        }

        private async UniTask AnimateInView(GameObject go)
        {
            var canvasGroup = go.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                await ViewAnimator.FadeIn(canvasGroup);
        }
    }
}
