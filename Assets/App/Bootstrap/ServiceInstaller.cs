using Miyo.Core;
using Miyo.Services.DateTimePicker;
using UnityEngine;

namespace Miyo.Bootstrap
{
    public class ServiceInstaller : MonoBehaviour
    {
        private void Awake()
        {
            RegisterDateTimePicker();
        }

        private void RegisterDateTimePicker()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            ServiceLocator.Register<INativeDateTimePicker>(
                new Infrastructure.DateTimePicker.AndroidDateTimePicker());
#elif UNITY_IOS && !UNITY_EDITOR
            ServiceLocator.Register<INativeDateTimePicker>(
                new Infrastructure.DateTimePicker.IOSDateTimePicker());
#else
            ServiceLocator.Register<INativeDateTimePicker>(
                new Infrastructure.DateTimePicker.EditorDateTimePicker());
#endif
        }
    }
}
