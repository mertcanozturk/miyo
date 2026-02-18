using System;
using Cysharp.Threading.Tasks;
using Miyo.Services.DateTimePicker;
using UnityEngine;

namespace Miyo.Infrastructure.DateTimePicker
{
    public class AndroidDateTimePicker : INativeDateTimePicker
    {
        public async UniTask<DateTimePickerResult> ShowPicker(
            DateTimePickerMode mode,
            DateTime initialValue = default,
            DateTime? minDate = null,
            DateTime? maxDate = null)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (initialValue == default) initialValue = DateTime.Now;

            return mode switch
            {
                DateTimePickerMode.DateOnly => await ShowDatePicker(initialValue, minDate, maxDate),
                DateTimePickerMode.TimeOnly => await ShowTimePicker(initialValue),
                DateTimePickerMode.DateTime => await ShowDateTimePicker(initialValue, minDate, maxDate),
                _ => new DateTimePickerResult { WasCancelled = true }
            };
#else
            return await UniTask.FromResult(new DateTimePickerResult { WasCancelled = true });
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private UniTask<DateTimePickerResult> ShowDatePicker(
            DateTime initial, DateTime? min, DateTime? max)
        {
            var tcs = new UniTaskCompletionSource<DateTimePickerResult>();

            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                var listener = new DateSetListener(tcs, initial);

                using var dialog = new AndroidJavaObject(
                    "android.app.DatePickerDialog",
                    activity,
                    listener,
                    initial.Year,
                    initial.Month - 1,
                    initial.Day);

                if (min.HasValue || max.HasValue)
                {
                    using var datePicker = dialog.Call<AndroidJavaObject>("getDatePicker");
                    if (min.HasValue)
                        datePicker.Call("setMinDate", ToUnixMillis(min.Value));
                    if (max.HasValue)
                        datePicker.Call("setMaxDate", ToUnixMillis(max.Value));
                }

                dialog.Call("setOnCancelListener", new DialogCancelListener(tcs));
                dialog.Call("show");
            }));

            return tcs.Task;
        }

        private UniTask<DateTimePickerResult> ShowTimePicker(DateTime initial)
        {
            var tcs = new UniTaskCompletionSource<DateTimePickerResult>();

            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                var listener = new TimeSetListener(tcs, initial);

                using var dialog = new AndroidJavaObject(
                    "android.app.TimePickerDialog",
                    activity,
                    listener,
                    initial.Hour,
                    initial.Minute,
                    true);

                dialog.Call("setOnCancelListener", new DialogCancelListener(tcs));
                dialog.Call("show");
            }));

            return tcs.Task;
        }

        private async UniTask<DateTimePickerResult> ShowDateTimePicker(
            DateTime initial, DateTime? min, DateTime? max)
        {
            var dateResult = await ShowDatePicker(initial, min, max);
            if (dateResult.WasCancelled)
                return dateResult;

            var timeInitial = new DateTime(
                dateResult.SelectedDateTime.Year,
                dateResult.SelectedDateTime.Month,
                dateResult.SelectedDateTime.Day,
                initial.Hour,
                initial.Minute,
                initial.Second);

            var timeResult = await ShowTimePicker(timeInitial);
            if (timeResult.WasCancelled)
                return timeResult;

            return new DateTimePickerResult
            {
                WasCancelled = false,
                SelectedDateTime = new DateTime(
                    dateResult.SelectedDateTime.Year,
                    dateResult.SelectedDateTime.Month,
                    dateResult.SelectedDateTime.Day,
                    timeResult.SelectedDateTime.Hour,
                    timeResult.SelectedDateTime.Minute,
                    timeResult.SelectedDateTime.Second)
            };
        }

        private static long ToUnixMillis(DateTime dt)
        {
            return new DateTimeOffset(dt).ToUnixTimeMilliseconds();
        }

        private class DateSetListener : AndroidJavaProxy
        {
            private readonly UniTaskCompletionSource<DateTimePickerResult> _tcs;
            private readonly DateTime _initial;

            public DateSetListener(
                UniTaskCompletionSource<DateTimePickerResult> tcs,
                DateTime initial)
                : base("android.app.DatePickerDialog$OnDateSetListener")
            {
                _tcs = tcs;
                _initial = initial;
            }

            // ReSharper disable once InconsistentNaming
            void onDateSet(AndroidJavaObject view, int year, int month, int dayOfMonth)
            {
                var result = new DateTimePickerResult
                {
                    WasCancelled = false,
                    SelectedDateTime = new DateTime(
                        year, month + 1, dayOfMonth,
                        _initial.Hour, _initial.Minute, _initial.Second)
                };
                _tcs.TrySetResult(result);
            }
        }

        private class TimeSetListener : AndroidJavaProxy
        {
            private readonly UniTaskCompletionSource<DateTimePickerResult> _tcs;
            private readonly DateTime _initial;

            public TimeSetListener(
                UniTaskCompletionSource<DateTimePickerResult> tcs,
                DateTime initial)
                : base("android.app.TimePickerDialog$OnTimeSetListener")
            {
                _tcs = tcs;
                _initial = initial;
            }

            // ReSharper disable once InconsistentNaming
            void onTimeSet(AndroidJavaObject view, int hourOfDay, int minute)
            {
                var result = new DateTimePickerResult
                {
                    WasCancelled = false,
                    SelectedDateTime = new DateTime(
                        _initial.Year, _initial.Month, _initial.Day,
                        hourOfDay, minute, 0)
                };
                _tcs.TrySetResult(result);
            }
        }

        private class DialogCancelListener : AndroidJavaProxy
        {
            private readonly UniTaskCompletionSource<DateTimePickerResult> _tcs;

            public DialogCancelListener(UniTaskCompletionSource<DateTimePickerResult> tcs)
                : base("android.content.DialogInterface$OnCancelListener")
            {
                _tcs = tcs;
            }

            // ReSharper disable once InconsistentNaming
            void onCancel(AndroidJavaObject dialog)
            {
                _tcs.TrySetResult(new DateTimePickerResult { WasCancelled = true });
            }
        }
#endif
    }
}
