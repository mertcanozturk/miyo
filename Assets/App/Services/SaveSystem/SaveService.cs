using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using UnityEngine;

namespace Miyo.Services.Save
{
    public class SaveService : ISaveService
    {
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            NullValueHandling = NullValueHandling.Include,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public async UniTask SaveAsync<T>(string key, T data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data, _jsonSettings);
                var saveData = new Dictionary<string, object> { { key, json } };
                await CloudSaveService.Instance.Data.Player.SaveAsync(saveData);
                Debug.Log($"[SaveService] Saved '{key}' to Cloud Save.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveService] Failed to save '{key}': {ex.Message}");
                throw;
            }
        }

        public async UniTask<T> LoadAsync<T>(string key, T defaultValue = default)
        {
            try
            {
                var keys = new HashSet<string> { key };
                var result = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

                if (result.TryGetValue(key, out var item))
                {
                    var json = item.Value.GetAs<string>();
                    Debug.Log($"[SaveService] Loaded '{key}' from Cloud Save.");
                    return JsonConvert.DeserializeObject<T>(json, _jsonSettings);
                }

                Debug.Log($"[SaveService] No cloud data found for '{key}', returning default.");
                return defaultValue;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveService] Failed to load '{key}': {ex.Message}. Returning default.");
                return defaultValue;
            }
        }

        public async UniTask<bool> ExistsAsync(string key)
        {
            try
            {
                var keys = new HashSet<string> { key };
                var result = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);
                return result.ContainsKey(key);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveService] Failed to check existence of '{key}': {ex.Message}");
                return false;
            }
        }

        public async UniTask DeleteAsync(string key)
        {
            try
            {
                await CloudSaveService.Instance.Data.Player.DeleteAsync(key);
                Debug.Log($"[SaveService] Deleted '{key}' from Cloud Save.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveService] Failed to delete '{key}': {ex.Message}");
                throw;
            }
        }

        public async UniTask DeleteAllAsync()
        {
            try
            {
                await AuthenticationService.Instance.DeleteAccountAsync();
                Debug.Log("[SaveService] All cloud saves deleted.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveService] Failed to delete all saves: {ex.Message}");
                throw;
            }
        }

        public byte[] Serialize<T>(T data)
        {
            var json = JsonConvert.SerializeObject(data, _jsonSettings);
            return Encoding.UTF8.GetBytes(json);
        }

        public T Deserialize<T>(byte[] bytes)
        {
            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(json, _jsonSettings);
        }
    }
}
