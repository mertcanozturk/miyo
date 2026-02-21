using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LitMotion;
using Miyo.Core;
using Miyo.UI.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Miyo.UI.Screens
{
    /// <summary>
    /// PIN girişi ekranının View bileşeni.
    ///
    /// ── Prefab hiyerarşisi ──────────────────────────────────────────────────────
    ///
    ///   [Root]  PinEntryView          ← bu bileşen + CanvasGroup
    ///   ├── Header
    ///   │   ├── TitleText             "Ebeveyn Girişi"
    ///   │   └── SubtitleText          "Devam etmek için PIN'inizi girin."
    ///   ├── PinDotsContainer (RT)     ← _pinDotsContainer  (shake hedefi)
    ///   │   ├── PinDot_0 (Image)      ┐
    ///   │   ├── PinDot_1 (Image)      │ → _pinDots listesi
    ///   │   ├── PinDot_2 (Image)      │   (PinLength kadar aktif olur)
    ///   │   └── PinDot_3 (Image)      ┘
    ///   ├── StatusText (TMP_Text)     ← _statusText
    ///   ├── LockoutPanel              ← _lockoutPanel  (kilitliyken aktif)
    ///   │   └── LockoutText (TMP_Text)← _lockoutText
    ///   ├── Keypad
    ///   │   ├── Btn_1 .. Btn_9        ┐ _digitButtons[0..8]  (1-9 sırasıyla)
    ///   │   ├── Btn_0                 ┘ _digitButtons[9]
    ///   │   └── Btn_Backspace         ← _backspaceButton
    ///   └── Btn_Close                 ← _closeButton  (isteğe bağlı)
    ///
    /// ────────────────────────────────────────────────────────────────────────────
    /// </summary>
    public class PinEntryView : ViewBase<PinEntryViewModel>
    {
        // ── Serileştirilen alanlar ────────────────────────────────────────────────

        [Header("PIN Dots")]
        [SerializeField] private RectTransform _pinDotsContainer;
        [SerializeField] private List<Image>   _pinDots;

        [SerializeField] private Color _dotFilledColor = new(0.22f, 0.47f, 0.97f); 
        [SerializeField] private Color _dotEmptyColor  = new(0.88f, 0.90f, 0.93f);
        [SerializeField] private Color _dotErrorColor  = new(0.93f, 0.27f, 0.27f);
        [SerializeField] private Color _dotSuccessColor= new(0.20f, 0.78f, 0.35f);

        [Header("Status Text")]
        [SerializeField] private TMP_Text  _statusText;

        [Header("lockout Panel")]
        [SerializeField] private GameObject _lockoutPanel;
        [SerializeField] private TMP_Text   _lockoutText;

        [Tooltip("0-9 ")]
        [SerializeField] private List<Button> _digitButtons;
        [SerializeField] private Button       _backspaceButton;

        [Header("Other")]
        [SerializeField] private Button _closeButton;

        private bool _isAnimating;


        protected override void OnBind(PinEntryViewModel vm)
        {
            InitializeDots(vm.PinLength);

            vm.CurrentDigitCount.Subscribe(UpdateDotFill).AddTo(Disposables);
            vm.IsLoading.Subscribe(_ => RefreshKeypadInteractable()).AddTo(Disposables);
            vm.IsLocked.Subscribe(OnLockStateChanged).AddTo(Disposables);

            vm.StatusMessage.Subscribe(msg =>
            {
                if (_statusText != null) _statusText.text = msg;
            }).AddTo(Disposables);

            vm.LockoutSecondsRemaining.Subscribe(sec =>
            {
                if (_lockoutText != null)
                    _lockoutText.text = FormatLockoutText(sec);
            }, invokeImmediately: false).AddTo(Disposables);

            for (int i = 0; i < _digitButtons.Count; i++)
            {
                int _digit = i;
                _digitButtons[i].BindClick(() => vm.OnDigitPressed(_digit)).AddTo(Disposables);
            }

            if (_backspaceButton != null)
                _backspaceButton.BindClick(vm.OnBackspacePressed).AddTo(Disposables);

            if (_closeButton != null)
                _closeButton.BindClick(OnCloseClicked).AddTo(Disposables);

            vm.OnWrongPinEntered  += HandleWrongPin;
            vm.OnCorrectPinEntered += HandleCorrectPin;

            Disposables.Add(new Disposable(() =>
            {
                vm.OnWrongPinEntered  -= HandleWrongPin;
                vm.OnCorrectPinEntered -= HandleCorrectPin;
            }));
        }


        private void InitializeDots(int pinLength)
        {
            for (int i = 0; i < _pinDots.Count; i++)
            {
                bool active = i < pinLength;
                _pinDots[i].gameObject.SetActive(active);
                if (active) _pinDots[i].color = _dotEmptyColor;
            }
        }

        private void UpdateDotFill(int filledCount)
        {
            if (_isAnimating) return;

            for (int i = 0; i < _pinDots.Count; i++)
            {
                if (!_pinDots[i].gameObject.activeSelf) continue;
                _pinDots[i].color = i < filledCount ? _dotFilledColor : _dotEmptyColor;
            }
        }


        private void HandleWrongPin()
        {
            if (_isAnimating) return;
            PlayWrongPinAnimationAsync().Forget();
        }

        private async UniTaskVoid PlayWrongPinAnimationAsync()
        {
            _isAnimating = true;

            SetAllDotsColor(_dotErrorColor);

            float originalX = _pinDotsContainer.localPosition.x;

            await LMotion.Create(-10f, 10f, 0.05f)
                .WithLoops(8, LoopType.Yoyo)
                .WithEase(Ease.InOutSine)
                .Bind(_pinDotsContainer, static (x, rt) =>
                {
                    var p = rt.localPosition;
                    p.x = x;
                    rt.localPosition = p;
                })
                .ToUniTask();

            ResetDotsContainerX(originalX);

            await UniTask.Delay(120);
            SetAllDotsColor(_dotEmptyColor);

            _isAnimating = false;
        }

        private void HandleCorrectPin()
        {
            PlayCorrectPinAnimationAsync().Forget();
        }

        private async UniTaskVoid PlayCorrectPinAnimationAsync()
        {
            SetAllDotsColor(_dotSuccessColor);

            await LMotion.Create(1f, 1.15f, 0.15f)
                .WithEase(Ease.OutCubic)
                .Bind(_pinDotsContainer, static (s, rt) => rt.localScale = new Vector3(s, s, 1f))
                .ToUniTask();

            await LMotion.Create(1.15f, 1f, 0.15f)
                .WithEase(Ease.InCubic)
                .Bind(_pinDotsContainer, static (s, rt) => rt.localScale = new Vector3(s, s, 1f))
                .ToUniTask();
        }


        private void OnLockStateChanged(bool isLocked)
        {
            if (_lockoutPanel != null) _lockoutPanel.SetActive(isLocked);
            RefreshKeypadInteractable();
        }

        private void RefreshKeypadInteractable()
        {
            bool interactive = !ViewModel.IsLoading.Value && !ViewModel.IsLocked.Value;

            foreach (var btn in _digitButtons)
                btn.interactable = interactive;

            if (_backspaceButton != null) _backspaceButton.interactable = interactive;
        }


        private void OnCloseClicked()
        {
            ServiceLocator.Get<INavigationService>().GoBack().Forget();
        }


        private void SetAllDotsColor(Color color)
        {
            foreach (var dot in _pinDots)
                if (dot.gameObject.activeSelf)
                    dot.color = color;
        }

        private void ResetDotsContainerX(float targetX)
        {
            var p = _pinDotsContainer.localPosition;
            p.x = targetX;
            _pinDotsContainer.localPosition = p;
        }

        private static string FormatLockoutText(int seconds)
        {
            if (seconds <= 0) return string.Empty;
            return seconds < 60 ? $"{seconds}s" : $"{seconds / 60}d {seconds % 60}s";
        }
    }
}
