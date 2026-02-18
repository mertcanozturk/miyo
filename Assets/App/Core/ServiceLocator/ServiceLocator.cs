using System;
using System.Collections.Generic;

namespace Miyo.Core
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();

        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                UnityEngine.Debug.LogWarning(
                    $"[ServiceLocator] Overwriting existing service: {type.Name}");
            }

            _services[type] = service;
        }

        public static T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return (T)service;

            throw new InvalidOperationException(
                $"[ServiceLocator] Service not found: {typeof(T).Name}. " +
                "Did you forget to register it in ServiceInstaller?");
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var obj))
            {
                service = (T)obj;
                return true;
            }

            service = null;
            return false;
        }

        public static bool Has<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        public static void Unregister<T>() where T : class
        {
            _services.Remove(typeof(T));
        }

        public static void Reset()
        {
            _services.Clear();
        }
    }
}
