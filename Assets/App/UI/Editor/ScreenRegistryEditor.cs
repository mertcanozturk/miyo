using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Miyo.UI.MVVM;

namespace Miyo.UI.Editor
{
    [CustomEditor(typeof(ScreenRegistry))]
    public class ScreenRegistryEditor : UnityEditor.Editor
    {
        private SerializedProperty _screensProp;

        private void OnEnable()
        {
            _screensProp = serializedObject.FindProperty("_screens");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(4);

            // Header
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Screen Registry", EditorStyles.boldLabel);

                if (GUILayout.Button("Prefabs Scan", GUILayout.Width(100)))
                    ScanAndRegister();
            }


            // Column headers
            var headerRect = EditorGUILayout.GetControlRect(false, 18);
            var idRect = new Rect(headerRect.x, headerRect.y, headerRect.width * 0.45f, headerRect.height);
            var prefabRect = new Rect(idRect.xMax + 4, headerRect.y, headerRect.width * 0.45f, headerRect.height);
            var removeRect = new Rect(prefabRect.xMax + 4, headerRect.y, headerRect.width * 0.1f - 4, headerRect.height);

            EditorGUI.LabelField(idRect, "Screen ID", EditorStyles.miniLabel);
            EditorGUI.LabelField(prefabRect, "Prefab", EditorStyles.miniLabel);

            EditorGUILayout.Space(2);

            // Entries
            int removeIndex = -1;
            for (int i = 0; i < _screensProp.arraySize; i++)
            {
                var entry = _screensProp.GetArrayElementAtIndex(i);
                var idProp = entry.FindPropertyRelative("screenId");
                var prefabProp = entry.FindPropertyRelative("prefab");

                var rowRect = EditorGUILayout.GetControlRect(false, 22);
                idRect = new Rect(rowRect.x, rowRect.y + 1, rowRect.width * 0.45f, 20);
                prefabRect = new Rect(idRect.xMax + 4, rowRect.y + 1, rowRect.width * 0.45f, 20);
                removeRect = new Rect(prefabRect.xMax + 4, rowRect.y + 1, rowRect.width * 0.1f - 4, 20);

                // Detect prefab drag-drop to auto-fill ID
                var prevPrefab = prefabProp.objectReferenceValue as GameObject;
                EditorGUI.PropertyField(idRect, idProp, GUIContent.none);
                var newPrefab = (GameObject)EditorGUI.ObjectField(prefabRect, prefabProp.objectReferenceValue, typeof(GameObject), false);

                if (newPrefab != prevPrefab)
                {
                    prefabProp.objectReferenceValue = newPrefab;

                    if (newPrefab != null && string.IsNullOrEmpty(idProp.stringValue))
                    {
                        var detectedId = DetectScreenId(newPrefab);
                        if (!string.IsNullOrEmpty(detectedId))
                            idProp.stringValue = detectedId;
                    }
                }

                if (GUI.Button(removeRect, "✕", EditorStyles.miniButton))
                    removeIndex = i;
            }

            if (removeIndex >= 0)
                _screensProp.DeleteArrayElementAtIndex(removeIndex);

            EditorGUILayout.Space(4);

            if (GUILayout.Button("+ Add", GUILayout.Height(24)))
            {
                _screensProp.arraySize++;
                var newEntry = _screensProp.GetArrayElementAtIndex(_screensProp.arraySize - 1);
                newEntry.FindPropertyRelative("screenId").stringValue = "";
                newEntry.FindPropertyRelative("prefab").objectReferenceValue = null;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ScanAndRegister()
        {
            var found = new List<(string id, GameObject prefab)>();
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/App/UI" });

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                var id = DetectScreenId(prefab);
                if (string.IsNullOrEmpty(id)) continue;

                // Skip if already registered
                bool alreadyExists = false;
                for (int i = 0; i < _screensProp.arraySize; i++)
                {
                    var existingId = _screensProp.GetArrayElementAtIndex(i)
                        .FindPropertyRelative("screenId").stringValue;
                    if (existingId == id) { alreadyExists = true; break; }
                }

                if (!alreadyExists)
                    found.Add((id, prefab));
            }

            if (found.Count == 0)
            {
                EditorUtility.DisplayDialog("Tara", "Yeni ekran prefab'ı bulunamadı.", "Tamam");
                return;
            }

            foreach (var (id, prefab) in found)
            {
                int idx = _screensProp.arraySize;
                _screensProp.arraySize++;
                var entry = _screensProp.GetArrayElementAtIndex(idx);
                entry.FindPropertyRelative("screenId").stringValue = id;
                entry.FindPropertyRelative("prefab").objectReferenceValue = prefab;
            }

            serializedObject.ApplyModifiedProperties();
            Debug.Log($"[ScreenRegistry] {found.Count} yeni ekran eklendi.");
        }

        private static string DetectScreenId(GameObject prefab)
        {
            // Prefab üzerindeki tüm MonoBehaviour'ları tara
            foreach (var mono in prefab.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mono == null) continue;
                var type = mono.GetType();

                // ViewBase<TViewModel> mı?
                var baseType = type.BaseType;
                while (baseType != null && baseType != typeof(MonoBehaviour))
                {
                    if (baseType.IsGenericType)
                    {
                        var genericDef = baseType.GetGenericTypeDefinition();
                        var viewBaseType = FindViewBaseGenericType();

                        if (viewBaseType != null && genericDef == viewBaseType)
                        {
                            var vmType = baseType.GetGenericArguments()[0];
                            return ViewModelIdHelper.GetId(vmType);
                        }
                    }
                    baseType = baseType.BaseType;
                }
            }
            return null;
        }

        private static Type _viewBaseGenericType;
        private static Type FindViewBaseGenericType()
        {
            if (_viewBaseGenericType != null) return _viewBaseGenericType;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in asm.GetTypes())
                {
                    if (type.Name == "ViewBase`1" && type.Namespace == "Miyo.UI.MVVM")
                    {
                        _viewBaseGenericType = type;
                        return _viewBaseGenericType;
                    }
                }
            }
            return null;
        }
    }
}
