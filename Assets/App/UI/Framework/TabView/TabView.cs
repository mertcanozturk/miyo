using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Miyo.UI.MVVM;
using Cysharp.Threading.Tasks;
using LitMotion;

namespace Miyo.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class TabView : MonoBehaviour
    {
        [Header("Tabs")]
        [SerializeField] private List<string> _tabLabels;
        [SerializeField] private TabViewButton _buttonPrefab;
        [SerializeField] private RectTransform _buttonsContainer;
        [SerializeField] private RectTransform _movingObject;

        [Header("Moving object")]
        [SerializeField] private float _moveDuration = 0.3f;
        [SerializeField] private Ease _moveEase = Ease.OutCubic;
        [SerializeField] private bool _matchWidth = true; 

        [SerializeField] private int _selectedIndex;

        private readonly List<TabViewButton> _buttons = new List<TabViewButton>();
        private ReactiveProperty<int> _selectedIndexProperty;
        private bool _initialized;
        private CancellationTokenSource _moveCts;

        public IReadOnlyReactiveProperty<int> SelectedIndex => _selectedIndexProperty;

        public event Action<int> OnSelectionChanged;

        private void Awake()
        {
            _selectedIndexProperty = new ReactiveProperty<int>(_selectedIndex);
            BuildButtons();
        }

        public void BuildButtons()
        {
            var container = _buttonsContainer != null ? _buttonsContainer : transform as RectTransform;

            if (_buttonPrefab != null && container != null)
            {
                for (int i = _buttons.Count - 1; i >= 0; i--)
                {
                    if (_buttons[i] != null)
                        Destroy(_buttons[i].gameObject);
                }
                _buttons.Clear();

                for (int i = 0; i < _tabLabels.Count; i++)
                {
                    var btn = Instantiate(_buttonPrefab, container);
                    btn.Setup(i);
                    btn.SetLabel(_tabLabels[i]);
                    btn.Clicked += OnTabClicked;
                    _buttons.Add(btn);
                }
            }
            else
            {
                _buttons.Clear();
                var existing = container != null ? container.GetComponentsInChildren<TabViewButton>(true) : GetComponentsInChildren<TabViewButton>(true);
                for (int i = 0; i < existing.Length; i++)
                {
                    existing[i].Setup(i);
                    if (i < _tabLabels.Count)
                        existing[i].SetLabel(_tabLabels[i]);
                    existing[i].Clicked += OnTabClicked;
                    _buttons.Add(existing[i]);
                }
            }

            _initialized = true;
            ApplySelection(_selectedIndex, false);
            DelaySnapToSelected(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid DelaySnapToSelected(CancellationToken ct)
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            if (ct.IsCancellationRequested || _movingObject == null) return;
            if (_buttonsContainer != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_buttonsContainer);
            Canvas.ForceUpdateCanvases();
            SnapMovingObjectTo(_selectedIndex);
        }

        public void ApplySelection(int index, bool snap = true)
        {
            _selectedIndex = Mathf.Clamp(index, 0, _buttons.Count > 0 ? _buttons.Count - 1 : 0);
            _selectedIndexProperty?.SetValueWithoutNotify(_selectedIndex);
            _selectedIndexProperty?.ForceNotify();

            for (int i = 0; i < _buttons.Count; i++)
                _buttons[i].SetSelected(i == _selectedIndex);

            if (snap)
                SnapMovingObjectTo(_selectedIndex);
        }

        public void SetSelectedIndex(int index)
        {
            if (_selectedIndex == index && _initialized) return;
            _selectedIndex = Mathf.Clamp(index, 0, _buttons.Count > 0 ? _buttons.Count - 1 : 0);
            _selectedIndexProperty.Value = _selectedIndex;
            for (int i = 0; i < _buttons.Count; i++)
                _buttons[i].SetSelected(i == _selectedIndex);
            OnSelectionChanged?.Invoke(_selectedIndex);

            AnimateMovingObjectTo(_selectedIndex).Forget();
        }

        private void OnTabClicked(int index)
        {
            SetSelectedIndex(index);
        }

        private bool TryGetButtonRect(int index, out RectTransform rect)
        {
            rect = null;
            if (index < 0 || index >= _buttons.Count || _buttons[index] == null) return false;
            rect = _buttons[index].transform as RectTransform;
            return rect != null;
        }

        private bool GetTargetPositionInMovingObjectParent(RectTransform target, out Vector2 anchoredPos, out Vector2 size)
        {
            anchoredPos = Vector2.zero;
            size = _movingObject.sizeDelta;
            var parent = _movingObject.parent as RectTransform;
            if (parent == null) return false;

            // Butonun pivot noktasının world konumu
            Vector3 targetWorld = target.TransformPoint(Vector3.zero);
            // TabView (moving object parent) local space'e çevir
            Vector2 localInParent = parent.InverseTransformPoint(targetWorld);

            // anchoredPosition = pivot'ın parent local konumu - anchor'ın parent local konumu
            Vector2 anchorNorm = (_movingObject.anchorMin + _movingObject.anchorMax) * 0.5f;
            Vector2 anchorCenter = new Vector2(
                Mathf.Lerp(parent.rect.min.x, parent.rect.max.x, anchorNorm.x),
                Mathf.Lerp(parent.rect.min.y, parent.rect.max.y, anchorNorm.y));
            anchoredPos = localInParent - anchorCenter;

            if (_matchWidth)
            {
                float widthInParent = target.rect.width * target.lossyScale.x / Mathf.Max(0.001f, parent.lossyScale.x);
                size = new Vector2(widthInParent, _movingObject.sizeDelta.y);
            }
            return true;
        }

        private void SnapMovingObjectTo(int index)
        {
            if (_movingObject == null || !TryGetButtonRect(index, out var target)) return;
            if (!GetTargetPositionInMovingObjectParent(target, out var anchoredPos, out var size)) return;

            _movingObject.anchoredPosition = anchoredPos;
            if (_matchWidth)
                _movingObject.sizeDelta = size;
        }

        private async UniTaskVoid AnimateMovingObjectTo(int index)
        {
            if (_movingObject == null || !TryGetButtonRect(index, out var target)) return;
            if (!GetTargetPositionInMovingObjectParent(target, out var endPos, out var endSize)) return;

            _moveCts?.Cancel();
            _moveCts = new CancellationTokenSource();
            var ct = _moveCts.Token;

            var startPos = _movingObject.anchoredPosition;
            var startSize = _movingObject.sizeDelta;
            if (!_matchWidth) endSize = startSize;

            try
            {
                await UniTask.WhenAll(
                    LMotion.Create(startPos, endPos, _moveDuration)
                        .WithEase(_moveEase)
                        .Bind(_movingObject, static (x, rt) => rt.anchoredPosition = x)
                        .ToUniTask(cancellationToken: ct),
                    _matchWidth
                        ? LMotion.Create(startSize, endSize, _moveDuration)
                            .WithEase(_moveEase)
                            .Bind(_movingObject, static (x, rt) => rt.sizeDelta = x)
                            .ToUniTask(cancellationToken: ct)
                        : UniTask.CompletedTask
                );
            }
            catch (OperationCanceledException) { }
            finally
            {
                _moveCts?.Dispose();
                _moveCts = null;
            }
        }

        private void OnDestroy()
        {
            _moveCts?.Cancel();
            _moveCts?.Dispose();
            _moveCts = null;
            foreach (var btn in _buttons)
            {
                if (btn != null)
                    btn.Clicked -= OnTabClicked;
            }
            _selectedIndexProperty?.Dispose();
        }
    }
}
