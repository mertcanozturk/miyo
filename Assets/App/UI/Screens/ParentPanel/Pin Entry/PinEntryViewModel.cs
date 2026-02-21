using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Miyo.Core;
using Miyo.Services.Auth;
using Miyo.UI.MVVM;

namespace Miyo.UI.Screens
{
    public class PinEntryViewModel : ViewModelBase
    {
        // ── Sabit bilgiler (View'ın prefab kurulumunda kullanması için) ──────────────
        public int PinLength { get; private set; }
        public int MaxWrongAttempts { get; private set; }

        // ── Reaktif durum ─────────────────────────────────────────────────────────
        public ReactiveProperty<int>    CurrentDigitCount       { get; } = new(0);
        public ReactiveProperty<bool>   IsLoading               { get; } = new(false);
        public ReactiveProperty<bool>   IsLocked                { get; } = new(false);
        public ReactiveProperty<int>    LockoutSecondsRemaining { get; } = new(0);
        public ReactiveProperty<int>    WrongAttemptCount       { get; } = new(0);
        public ReactiveProperty<string> StatusMessage           { get; } = new(string.Empty);

        // ── Animasyon olayları (View bunlara abone olur) ──────────────────────────
        /// <summary>Yanlış PIN girildiğinde tetiklenir; View shake animasyonu oynatır.</summary>
        public event Action OnWrongPinEntered;

        /// <summary>Doğru PIN girildiğinde tetiklenir; View başarı animasyonu oynatabilir.</summary>
        public event Action OnCorrectPinEntered;

        // ── İç durum ──────────────────────────────────────────────────────────────
        private string _currentPin = string.Empty;
        private PinEntryConfig _config;
        private Func<UniTask> _onSuccess;
        private int _lockoutRound;
        private CancellationTokenSource _lockoutCts;

        // ── Yapılandırma ──────────────────────────────────────────────────────────

        /// <summary>
        /// NavigateTo callback'i içinden çağrılır:
        /// <code>
        ///   nav.NavigateTo&lt;PinEntryViewModel&gt;(vm => vm.Configure(config, OnPinSuccess));
        /// </code>
        /// PIN doğrulaması AuthService.VerifyPinAsync üzerinden yapılır.
        /// </summary>
        /// <param name="config">Inspector'da ayarlanan konfigürasyon asset'i.</param>
        /// <param name="onSuccess">Doğrulama başarılı olunca çalışacak navigasyon/işlem.</param>
        public void Configure(PinEntryConfig config, Func<UniTask> onSuccess)
        {
            _config          = config;
            _onSuccess       = onSuccess;
            PinLength        = config.PinLength;
            MaxWrongAttempts = config.MaxWrongAttempts;
        }

        // ── Tuş takımı komutları ──────────────────────────────────────────────────

        public void OnDigitPressed(int digit)
        {
            if (IsLocked.Value || IsLoading.Value) return;
            if (_currentPin.Length >= PinLength) return;

            UnityEngine.Debug.Log($"[PIN] digit pressed: {digit}");

            _currentPin += digit.ToString();
            CurrentDigitCount.Value = _currentPin.Length;

            if (_currentPin.Length == PinLength)
                SubmitPinAsync().Forget();
        }

        public void OnBackspacePressed()
        {
            if (IsLocked.Value || IsLoading.Value) return;
            if (_currentPin.Length == 0) return;

            _currentPin = _currentPin[..^1];
            CurrentDigitCount.Value = _currentPin.Length;
        }

        // ── Çekirdek mantık ───────────────────────────────────────────────────────

        private async UniTaskVoid SubmitPinAsync()
        {
            IsLoading.Value = true;

            var authService = ServiceLocator.Get<IAuthService>();
            bool isCorrect = await authService.VerifyPinAsync(_currentPin);

            if (isCorrect)
            {
                IsLoading.Value = false;
                WrongAttemptCount.Value = 0;
                StatusMessage.Value = string.Empty;
                OnCorrectPinEntered?.Invoke();
                await _onSuccess();
                return;
            }

            // Yanlış PIN
            WrongAttemptCount.Value++;
            _currentPin = string.Empty;
            CurrentDigitCount.Value = 0;
            IsLoading.Value = false;

            OnWrongPinEntered?.Invoke();

            if (WrongAttemptCount.Value >= MaxWrongAttempts)
            {
                // Shake animasyonunun bitmesini bekle, ardından kilitle
                await UniTask.Delay(450);
                await StartLockoutAsync();
            }
            else
            {
                int remaining = MaxWrongAttempts - WrongAttemptCount.Value;
                StatusMessage.Value = remaining == 1
                    ? "Son deneme hakkınız!"
                    : $"{remaining} deneme hakkınız kaldı";
            }
        }

        private async UniTask StartLockoutAsync()
        {
            _lockoutCts?.Cancel();
            _lockoutCts?.Dispose();
            _lockoutCts = new CancellationTokenSource();

            int duration = _config.GetLockoutDuration(_lockoutRound);
            _lockoutRound++;

            WrongAttemptCount.Value = 0;
            IsLocked.Value          = true;

            try
            {
                for (int remaining = duration; remaining > 0; remaining--)
                {
                    LockoutSecondsRemaining.Value = remaining;
                    StatusMessage.Value = FormatLockoutMessage(remaining);
                    await UniTask.Delay(1000, cancellationToken: _lockoutCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }

            IsLocked.Value                = false;
            LockoutSecondsRemaining.Value = 0;
            StatusMessage.Value           = string.Empty;
        }

        private static string FormatLockoutMessage(int remainingSeconds)
        {
            string time = remainingSeconds < 60
                ? $"{remainingSeconds}s"
                : $"{remainingSeconds / 60}d {remainingSeconds % 60}s";

            return $"Çok fazla hatalı deneme. {time} sonra tekrar deneyin.";
        }

        protected override void OnDispose()
        {
            _lockoutCts?.Cancel();
            _lockoutCts?.Dispose();
        }
    }
}
