using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Miyo.Core;
using Miyo.Data;
using Miyo.Services.Analytics;
using Miyo.Services.Auth;
using Miyo.Services.ChildProfile;
using Miyo.Services.Save;
using Miyo.UI.MVVM;

namespace Miyo.UI.Screens
{
    public class ParentHomeViewModel : ViewModelBase
    {
        public ReactiveProperty<List<GameStatistic>> Statistics { get; } = new();
        public ReactiveProperty<int> SelectedTab { get; } = new(0);
        public ReactiveProperty<List<ChildProfile>> Children { get; } = new();
        public ReactiveProperty<ChildProfile> SelectedChild { get; } = new();
        public ReactiveProperty<List<(CategoryDefinition category, int playedMinutes, int limitMinutes)>> DailyCategorySummaries { get; } = new();
        public ReactiveProperty<int> TotalPlayedTimeForSelectedTab { get; } = new();

        protected override async void Initialize()
        {
            var authService = ServiceLocator.Get<IAuthService>();
            var profileService = ServiceLocator.Get<IChildProfileService>();

            var children = await profileService.GetChildrenForParentAsync(authService.PlayerId);
            Children.Value = children;

            if (children.Count > 0)
                SelectChild(children[0].Id);

            SelectedTab.Subscribe(_ => CalculateTotalPlayedTimeForSelectedTab()).AddTo(Disposables);
            Statistics.Subscribe(_ => CalculateTotalPlayedTimeForSelectedTab()).AddTo(Disposables);
        }

        public async void OnDeleteSaveDataClicked()
        {
            var saveService = ServiceLocator.Get<ISaveService>();
            await saveService.DeleteAllAsync();
            UnityEngine.Application.Quit();
        }

        public void OnLogoutClicked()
        {
            var authService = ServiceLocator.Get<IAuthService>();
            authService.Logout();
            // Navigate to login screen
            var nav = ServiceLocator.Get<INavigationService>();
            nav.NavigateTo<ParentLoginViewModel>().Forget();
        }

        public void SelectChild(string childId)
        {
            var child = Children.Value?.FirstOrDefault(c => c.Id == childId);
            if (child == null) return;

            SelectedChild.Value = child;
            LoadStatisticsForChild(child.Id);
        }

        public void OnAddChildClicked()
        {
            var nav = ServiceLocator.Get<INavigationService>();
            nav.NavigateTo<CreateChildViewModel>().Forget();
        }

        private void LoadStatisticsForChild(string childId)
        {
            var analytics = ServiceLocator.Get<IAnalyticsService>();
            var db = ServiceLocator.Get<GameDatabase>();
            var stats = analytics.GetDummyGameStatistics(childId, db.Games);
            Statistics.Value = new List<GameStatistic>(stats);

            var dailyStats = StatisticExtensions.GetDailyPlayTimeByGame(stats.ToArray(), System.DateTime.Today);
            var child = SelectedChild.Value;
            var isWeekend = System.DateTime.Today.DayOfWeek == System.DayOfWeek.Saturday || System.DateTime.Today.DayOfWeek == System.DayOfWeek.Sunday;
            int limit = isWeekend ? child.WeekendLimitMinutes : child.WeekdayLimitMinutes;
            
            // For now, if limit is 0, we can default it to 60 as a fallback or keep it 0.
            if (limit <= 0) limit = 60;

            var categoryGrouped = dailyStats
                .Where(x => x.game != null && x.game.Category != null)
                .GroupBy(x => x.game.Category)
                .Select(g => (category: g.Key, playedMinutes: g.Sum(x => x.playTime), limitMinutes: limit))
                .ToList();

            DailyCategorySummaries.Value = categoryGrouped;
        }

        private void CalculateTotalPlayedTimeForSelectedTab()
        {
            var stats = Statistics.Value;
            if (stats == null)
            {
                TotalPlayedTimeForSelectedTab.Value = 0;
                return;
            }

            int tabIndex = SelectedTab.Value;
            int total = 0;
            System.DateTime today = System.DateTime.Today;

            if (tabIndex == 0) // Daily
            {
                total = stats.Where(s => s.date.Date == today).Sum(s => s.playTimeMinutes);
            }
            else if (tabIndex == 1) // Weekly
            {
                var start = today.AddDays(-6);
                total = stats.Where(s => s.date.Date >= start && s.date.Date <= today).Sum(s => s.playTimeMinutes);
            }
            else if (tabIndex == 2) // Monthly
            {
                var start = today.AddDays(-29);
                total = stats.Where(s => s.date.Date >= start && s.date.Date <= today).Sum(s => s.playTimeMinutes);
            }

            TotalPlayedTimeForSelectedTab.Value = total;
        }
    }
}
