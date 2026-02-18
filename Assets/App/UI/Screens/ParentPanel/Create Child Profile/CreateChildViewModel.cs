using System;
using Cysharp.Threading.Tasks;
using Miyo.Core;
using Miyo.Services.DateTimePicker;
using Miyo.UI.MVVM;

namespace Miyo.UI.Screens
{
    public class CreateChildViewModel : ViewModelBase
    {
        public ReactiveProperty<string> ChildName { get; } = new("");
        public ReactiveProperty<DateTime?> BirthDate { get; } = new(null);
        public ReactiveProperty<string> BirthDateText { get; } = new("");
        public ReactiveProperty<float> WeekdayLimit { get; } = new(75f);
        public ReactiveProperty<float> WeekendLimit { get; } = new(90f);
        public ReactiveProperty<string> WeekdayLimitText { get; } = new("75 Dakika");
        public ReactiveProperty<string> WeekendLimitText { get; } = new("90 Dakika");
        public ReactiveProperty<bool> CanSubmit { get; } = new(false);
        public ReactiveProperty<bool> IsLoading { get; } = new(false);

        public const float MinLimit = 15f;
        public const float MaxLimit = 180f;

        protected override void Initialize()
        {
            ChildName.Subscribe(_ => ValidateForm(), invokeImmediately: false).AddTo(Disposables);
            BirthDate.Subscribe(_ => ValidateForm(), invokeImmediately: false).AddTo(Disposables);

            BirthDate.Subscribe(value =>
            {
                BirthDateText.Value = value.HasValue
                    ? value.Value.ToString("dd/MM/yyyy")
                    : "";
            }).AddTo(Disposables);

            WeekdayLimit.Subscribe(value =>
            {
                value = Math.Clamp(value, MinLimit, MaxLimit);
                WeekdayLimitText.Value = $"{(int)value} Dakika";
            }).AddTo(Disposables);

            WeekendLimit.Subscribe(value =>
            {
                value = Math.Clamp(value, MinLimit, MaxLimit);
                WeekendLimitText.Value = $"{(int)value} Dakika";
            }).AddTo(Disposables);
        }

        private void ValidateForm()
        {
            bool hasName = !string.IsNullOrWhiteSpace(ChildName.Value);
            bool hasDate = BirthDate.Value.HasValue;
            CanSubmit.Value = hasName && hasDate;
        }

        public async void OnCreateClicked()
        {
            if (!CanSubmit.Value || IsLoading.Value) return;

            IsLoading.Value = true;

            // TODO: Profil kaydetme servisi entegrasyonu
            // var profileService = ServiceLocator.Get<IChildProfileService>();
            // await profileService.CreateChild(
            //     ChildName.Value,
            //     BirthDate.Value.Value,
            //     (int)WeekdayLimit.Value,
            //     (int)WeekendLimit.Value);

            await UniTask.Delay(1000); // Simülasyon

            IsLoading.Value = false;

            var nav = ServiceLocator.Get<INavigationService>();
            nav.GoBack().Forget();
        }

        public void OnBackClicked()
        {
            var nav = ServiceLocator.Get<INavigationService>();
            nav.GoBack().Forget();
        }
    }
}
