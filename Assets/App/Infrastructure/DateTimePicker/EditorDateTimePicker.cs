using System;
using Cysharp.Threading.Tasks;
using Miyo.Services.DateTimePicker;
using UnityEngine;

namespace Miyo.Infrastructure.DateTimePicker
{
    public class EditorDateTimePicker : INativeDateTimePicker
    {
        public UniTask<DateTimePickerResult> ShowPicker(
            DateTimePickerMode mode,
            DateTime initialValue = default,
            DateTime? minDate = null,
            DateTime? maxDate = null)
        {
            if (initialValue == default) initialValue = DateTime.Now;

            Debug.Log($"[EditorDateTimePicker] ShowPicker called - Mode: {mode}, " +
                      $"Initial: {initialValue:yyyy-MM-dd HH:mm}, " +
                      $"Returning initial value as result.");

            return UniTask.FromResult(new DateTimePickerResult
            {
                WasCancelled = false,
                SelectedDateTime = initialValue
            });
        }
    }
}
