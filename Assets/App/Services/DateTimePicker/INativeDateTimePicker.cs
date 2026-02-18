using System;
using Cysharp.Threading.Tasks;

namespace Miyo.Services.DateTimePicker
{
    public enum DateTimePickerMode
    {
        DateOnly,
        TimeOnly,
        DateTime
    }

    public struct DateTimePickerResult
    {
        public bool WasCancelled;
        public DateTime SelectedDateTime;
    }

    public interface INativeDateTimePicker
    {
        UniTask<DateTimePickerResult> ShowPicker(
            DateTimePickerMode mode,
            DateTime initialValue = default,
            DateTime? minDate = null,
            DateTime? maxDate = null);
    }
}
