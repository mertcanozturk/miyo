using System;
using System.Runtime.InteropServices;
using AOT;
using Cysharp.Threading.Tasks;
using Miyo.Services.DateTimePicker;

namespace Miyo.Infrastructure.DateTimePicker
{
    public class IOSDateTimePicker : INativeDateTimePicker
    {
        private static UniTaskCompletionSource<DateTimePickerResult> _tcs;

        private delegate void DateTimePickerCallback(
            int year, int month, int day, int hour, int minute, bool cancelled);

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void _ShowNativeDateTimePicker(
            int mode, int year, int month, int day, int hour, int minute,
            bool hasMin, long minUnixMs, bool hasMax, long maxUnixMs,
            DateTimePickerCallback callback);
#endif

        public UniTask<DateTimePickerResult> ShowPicker(
            DateTimePickerMode mode,
            DateTime initialValue = default,
            DateTime? minDate = null,
            DateTime? maxDate = null)
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (initialValue == default) initialValue = DateTime.Now;

            _tcs = new UniTaskCompletionSource<DateTimePickerResult>();

            long minMs = minDate.HasValue
                ? new DateTimeOffset(minDate.Value).ToUnixTimeMilliseconds()
                : 0;
            long maxMs = maxDate.HasValue
                ? new DateTimeOffset(maxDate.Value).ToUnixTimeMilliseconds()
                : 0;

            _ShowNativeDateTimePicker(
                (int)mode,
                initialValue.Year, initialValue.Month, initialValue.Day,
                initialValue.Hour, initialValue.Minute,
                minDate.HasValue, minMs,
                maxDate.HasValue, maxMs,
                OnDateTimePicked);

            return _tcs.Task;
#else
            return UniTask.FromResult(new DateTimePickerResult { WasCancelled = true });
#endif
        }

        [MonoPInvokeCallback(typeof(DateTimePickerCallback))]
        private static void OnDateTimePicked(
            int year, int month, int day, int hour, int minute, bool cancelled)
        {
            if (cancelled)
            {
                _tcs?.TrySetResult(new DateTimePickerResult { WasCancelled = true });
            }
            else
            {
                _tcs?.TrySetResult(new DateTimePickerResult
                {
                    WasCancelled = false,
                    SelectedDateTime = new DateTime(year, month, day, hour, minute, 0)
                });
            }
        }
    }
}
