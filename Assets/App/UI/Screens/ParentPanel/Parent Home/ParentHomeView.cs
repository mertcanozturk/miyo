using System;
using System.Collections.Generic;
using System.Linq;
using Miyo.Data;
using Miyo.UI.MVVM;
using UnityEngine;
using UnityEngine.UI;

namespace Miyo.UI.Screens
{
    public class ParentHomeView : ViewBase<ParentHomeViewModel>
    {
        [SerializeField] private TabView _tabView;
        [SerializeField] private BarGraph _barGraph;
        [SerializeField] private PlayedGamesView _dailyPlayedGamesView;
        [SerializeField] private ChildSelectorView _childSelector;
        [SerializeField] private PlayTimeSummaryDaily _playTimeSummaryDaily;
        [SerializeField] private PlayedTimeByDateSummary _playedTimeSummary;
        [SerializeField] private Button _deleteSaveDataButton;
        [SerializeField] private Button _logoutButton;

        protected override void OnBind(ParentHomeViewModel vm)
        {
            _tabView.OnSelectionChanged += OnSelectionChanged;
            _childSelector.OnChildSelected += OnChildSelected;
            _childSelector.OnAddChildClicked += OnAddChildClicked;
            _deleteSaveDataButton.BindClick(vm.OnDeleteSaveDataClicked).AddTo(Disposables);
            _logoutButton.BindClick(vm.OnLogoutClicked).AddTo(Disposables);


            vm.Children.Subscribe(children =>
            {
                if (children == null) return;
                var selectedId = vm.SelectedChild.Value?.Id;
                _childSelector.SetChildren(children, selectedId);
            }).AddTo(Disposables);

            vm.DailyCategorySummaries.Subscribe(summaries =>
            {
                if (summaries == null) return;
                var childName = vm.SelectedChild.Value?.Name ?? "Çocuk";
                if (_playTimeSummaryDaily != null)
                {
                    _playTimeSummaryDaily.Prepare(childName, summaries);
                }
            }).AddTo(Disposables);

            vm.TotalPlayedTimeForSelectedTab.Subscribe(totalTime => 
            {
                if (_playedTimeSummary != null)
                {
                    _playedTimeSummary.UpdateSummary(vm.SelectedTab.Value, totalTime);
                }
            }).AddTo(Disposables);

            vm.Statistics.Subscribe(_ => UpdateVisuals()).AddTo(Disposables);
        }

        private void OnChildSelected(string childId)
        {
            ViewModel.SelectChild(childId);
        }

        private void OnAddChildClicked()
        {
            ViewModel.OnAddChildClicked();
        }

        private void OnSelectionChanged(int tabIndex)
        {
            ViewModel.SelectedTab.Value = tabIndex;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            var stats = ViewModel.Statistics.Value;
            if (stats == null) return;

            var tabIndex = ViewModel.SelectedTab.Value;

            var playedMode = tabIndex switch
            {
                1 => PlayedGamesView.Mode.Weekly,
                2 => PlayedGamesView.Mode.Monthly,
                _ => PlayedGamesView.Mode.Daily
            };

            _dailyPlayedGamesView.Prepare(stats, playedMode);
            _barGraph.SetData(BuildBarEntries(stats, tabIndex));
            Canvas.ForceUpdateCanvases();
        }

        private static List<BarGraphEntry> BuildBarEntries(List<GameStatistic> stats, int tabIndex)
        {
            var arr = stats.ToArray();
            var entries = new List<BarGraphEntry>();

            switch (tabIndex)
            {
                case 0:
                    for (int i = 6; i >= 0; i--)
                    {
                        var day = DateTime.Today.AddDays(-i);
                        int total = arr.Where(s => s.date.Date == day.Date).Sum(s => s.playTimeMinutes);
                        entries.Add(new BarGraphEntry
                        {
                            Value = total,
                            ValueDisplay = FormatMinutes(total),
                            Label = day.ToString("ddd")
                        });
                    }
                    break;

                case 1:
                    for (int w = 3; w >= 0; w--)
                    {
                        var start = DateTime.Today.AddDays(-w * 7 - 6);
                        var end = DateTime.Today.AddDays(-w * 7);
                        int total = arr.Where(s => s.date.Date >= start && s.date.Date <= end).Sum(s => s.playTimeMinutes);
                        entries.Add(new BarGraphEntry
                        {
                            Value = total,
                            ValueDisplay = FormatMinutes(total),
                            Label = $"Hf {4 - w}"
                        });
                    }
                    break;

                case 2:
                    for (int m = 5; m >= 0; m--)
                    {
                        var month = DateTime.Today.AddMonths(-m);
                        int total = arr.Where(s => s.date.Year == month.Year && s.date.Month == month.Month)
                                       .Sum(s => s.playTimeMinutes);
                        entries.Add(new BarGraphEntry
                        {
                            Value = total,
                            ValueDisplay = FormatMinutes(total),
                            Label = month.ToString("MMM")
                        });
                    }
                    break;
            }

            return entries;
        }

        private static string FormatMinutes(int minutes)
        {
            if (minutes == 0) return "0dk";
            int h = minutes / 60;
            int m = minutes % 60;
            return h > 0 ? $"{h}s {m}dk" : $"{m}dk";
        }

        protected override void OnUnbind()
        {
            base.OnUnbind();
            _tabView.OnSelectionChanged -= OnSelectionChanged;
            _childSelector.OnChildSelected -= OnChildSelected;
            _childSelector.OnAddChildClicked -= OnAddChildClicked;
        }
    }
}
