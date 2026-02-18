using System;
using UnityEngine;

namespace Miyo.UI.MVVM
{
    [CreateAssetMenu(menuName = "Miyo/UI/Screen Registry", fileName = "ScreenRegistry")]
    public class ScreenRegistry : ScriptableObject
    {
        [Serializable]
        public class ScreenEntry
        {
            [Tooltip("ViewModel tipinden otomatik türetilir.\nÖrnek: ParentLoginViewModel → \"parent-login\"\n         HomeViewModel → \"home\"")]
            public string screenId;
            public GameObject prefab;
        }

        [SerializeField] private ScreenEntry[] _screens;

        public GameObject GetPrefab(string screenId)
        {
            for (int i = 0; i < _screens.Length; i++)
            {
                if (_screens[i].screenId == screenId)
                    return _screens[i].prefab;
            }

            Debug.LogError($"[ScreenRegistry] Screen not found: {screenId}");
            return null;
        }

        public bool HasScreen(string screenId)
        {
            for (int i = 0; i < _screens.Length; i++)
            {
                if (_screens[i].screenId == screenId)
                    return true;
            }
            return false;
        }
    }
}
