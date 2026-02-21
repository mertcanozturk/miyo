using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using LitMotion;

namespace Miyo.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class NavigationBarView : MonoBehaviour
    {
        [Header("Tabs")]
        [SerializeField] private List<NavigationBarButtonData> _buttons;

        [Header("Content")]
        [SerializeField] private RectTransform _contentContainer;
        [SerializeField] private RectTransform _viewport;

        [Header("Slide")]
        [SerializeField] private float _slideDuration = 0.25f;
        [SerializeField] private Ease _slideEase = Ease.OutCubic;
        [SerializeField] private float _fastForwardDuration = 0.08f;

        [SerializeField] private int _selectedIndex;

        private float _viewportWidth;
        private int _targetIndex;
        private int? _queuedIndex;
        private bool _isAnimating;
        private CancellationTokenSource _slideCts;
        private bool _initialized;

        public int SelectedIndex => _targetIndex;
        public event Action<int> OnSelectionChanged;

        private void Awake()
        {
            _targetIndex = _selectedIndex;
            int count = _buttons != null ? _buttons.Count : 0;
            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, Mathf.Max(0, count - 1));
            _targetIndex = _selectedIndex;

            if (_contentContainer == null || _buttons == null || count == 0)
                return;

            for (int i = 0; i < count; i++)
            {
                var data = _buttons[i];
                var btn = data.Button;
                var page = data.page;
                page.gameObject.SetActive(true);
                if (btn == null) continue;
                btn.Setup(i);
                btn.SetSelected(i == _targetIndex);
                btn.Clicked += OnNavButtonClicked;
            }

            DelayInit(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid DelayInit(CancellationToken ct)
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            if (ct.IsCancellationRequested || _contentContainer == null) return;

            var viewportRt = _viewport != null ? _viewport : _contentContainer.parent as RectTransform;
            if (viewportRt != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(viewportRt);
            Canvas.ForceUpdateCanvases();

            _viewportWidth = viewportRt != null ? viewportRt.rect.width : _contentContainer.rect.width;
            if (_viewportWidth <= 0f)
                _viewportWidth = (_contentContainer.parent as RectTransform)?.rect.width ?? 0f;
            if (_viewportWidth <= 0f)
                return;

            LayoutPages();
            ApplyContentPosition(_targetIndex);
            _initialized = true;
        }

        private void LayoutPages()
        {
            int count = _buttons.Count;
            _contentContainer.anchorMin = new Vector2(0f, 0.5f);
            _contentContainer.anchorMax = new Vector2(0f, 0.5f);
            _contentContainer.pivot = new Vector2(0f, 0.5f);
            _contentContainer.sizeDelta = new Vector2(count * _viewportWidth, _contentContainer.sizeDelta.y);

            for (int i = 0; i < count; i++)
            {
                var data = _buttons[i];
                if (data.page == null) continue;

                var pageRt = data.page as RectTransform;
                if (pageRt == null) continue;

                if (pageRt.parent != _contentContainer)
                    pageRt.SetParent(_contentContainer, false);

                pageRt.anchorMin = new Vector2(0f, 0.5f);
                pageRt.anchorMax = new Vector2(0f, 0.5f);
                pageRt.pivot = new Vector2(0f, 0.5f);
                pageRt.anchoredPosition = new Vector2(i * _viewportWidth, 0f);
                pageRt.sizeDelta = new Vector2(_viewportWidth, _contentContainer.sizeDelta.y);
            }
        }

        private float GetTargetPositionX(int index)
        {
            return -index * _viewportWidth;
        }

        private void ApplyContentPosition(int index)
        {
            var pos = _contentContainer.anchoredPosition;
            pos.x = GetTargetPositionX(index);
            _contentContainer.anchoredPosition = pos;
        }

        private void OnNavButtonClicked(int index)
        {
            if (!_initialized || _contentContainer == null) return;
            if (index == _targetIndex && !_isAnimating) return;

            int count = _buttons.Count;
            index = Mathf.Clamp(index, 0, count - 1);

            for (int i = 0; i < count; i++)
            {
                if (_buttons[i].Button != null)
                    _buttons[i].Button.SetSelected(i == index);
            }

            _selectedIndex = index;

            if (_isAnimating)
            {
                _queuedIndex = index;
                _slideCts?.Cancel();
                return;
            }

            _targetIndex = index;
            _isAnimating = true;
            AnimateTo(index).Forget();
        }

        private async UniTaskVoid AnimateTo(int index)
        {
            _slideCts?.Cancel();
            _slideCts = new CancellationTokenSource();
            var ct = _slideCts.Token;

            float startX = _contentContainer.anchoredPosition.x;
            float endX = GetTargetPositionX(index);

            try
            {
                await LMotion.Create(startX, endX, _slideDuration)
                    .WithEase(_slideEase)
                    .Bind(_contentContainer, static (x, rt) =>
                    {
                        var p = rt.anchoredPosition;
                        p.x = x;
                        rt.anchoredPosition = p;
                    })
                    .ToUniTask(cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                if (_queuedIndex.HasValue)
                    FastForwardThenQueued().Forget();
                return;
            }
            finally
            {
                _slideCts?.Dispose();
                _slideCts = null;
            }

            _isAnimating = false;
            OnSelectionChanged?.Invoke(_targetIndex);

            if (_queuedIndex.HasValue)
            {
                int next = _queuedIndex.Value;
                _queuedIndex = null;
                _targetIndex = next;
                for (int i = 0; i < _buttons.Count; i++)
                {
                    if (_buttons[i].Button != null)
                        _buttons[i].Button.SetSelected(i == next);
                }
                _isAnimating = true;
                AnimateTo(next).Forget();
            }
        }

        private async UniTaskVoid FastForwardThenQueued()
        {
            float startX = _contentContainer.anchoredPosition.x;
            float endX = GetTargetPositionX(_targetIndex);

            await LMotion.Create(startX, endX, _fastForwardDuration)
                .WithEase(Ease.OutCubic)
                .Bind(_contentContainer, static (x, rt) =>
                {
                    var p = rt.anchoredPosition;
                    p.x = x;
                    rt.anchoredPosition = p;
                })
                .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());

            _isAnimating = false;
            OnSelectionChanged?.Invoke(_targetIndex);

            if (!_queuedIndex.HasValue) return;

            int next = _queuedIndex.Value;
            _queuedIndex = null;
            _targetIndex = next;

            for (int i = 0; i < _buttons.Count; i++)
            {
                if (_buttons[i].Button != null)
                    _buttons[i].Button.SetSelected(i == next);
            }

            _isAnimating = true;
            AnimateTo(next).Forget();
        }

        private void OnDestroy()
        {
            _slideCts?.Cancel();
            _slideCts?.Dispose();
            _slideCts = null;
            if (_buttons != null)
            {
                foreach (var data in _buttons)
                {
                    if (data.Button != null)
                        data.Button.Clicked -= OnNavButtonClicked;
                }
            }
        }

        [Serializable]
        public struct NavigationBarButtonData
        {
            public NavigationBarButton Button;
            public Transform page;
        }
    }
}
