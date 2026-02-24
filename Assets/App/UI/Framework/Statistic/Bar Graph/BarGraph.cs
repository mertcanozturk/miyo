using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Miyo.UI
{
    [Serializable]
    public struct BarGraphEntry
    {
        public float Value;
        public string ValueDisplay;
        public string Label;
    }

    [RequireComponent(typeof(RectTransform))]
    public class BarGraph : MonoBehaviour
    {
        [Header("Collections")]
        [SerializeField] private UICollection<BarGraphValue> _valueElements;
        [SerializeField] private UICollection<BarGraphIndicator> _indicators;

        private float _barWidth;

        public void SetData(IReadOnlyList<BarGraphEntry> entries, float maxValue = 0f)
        {
            if (entries == null || entries.Count == 0)
            {
                Clear();
                return;
            }

            if (_valueElements == null || _indicators == null)
            {
                Debug.LogError("UICollection'lar Inspector'dan atanmalı!");
                return;
            }

            float calculatedMaxValue = maxValue > 0 ? maxValue : entries.Max(e => e.Value);
            if (calculatedMaxValue <= 0) calculatedMaxValue = 1f;

            _valueElements.Count = entries.Count;
            _indicators.Count = entries.Count;

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];

                if (_valueElements.Count > i)
                {
                    var barGraphValue = _valueElements[i];
                    barGraphValue.SetValue(entry.ValueDisplay);
                    barGraphValue.SetFillAmount(entry.Value / calculatedMaxValue);
                }

                if (_indicators.Count > i)
                {
                    var barGraphIndicator = _indicators[i];
                    barGraphIndicator.SetLabel(entry.Label);
                }
            }

            UpdateLayouts();
        }

        private void Clear()
        {
            if (_valueElements != null)
            {
                _valueElements.Count = 0;
            }

            if (_indicators != null)
            {
                _indicators.Count = 0;
            }
        }

        private void UpdateLayouts()
        {
            if (_valueElements == null || _indicators == null)
                return;

            var topContainer = _valueElements.CurrentParent;
            var bottomContainer = _indicators.CurrentParent;

            if (topContainer != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(topContainer);
            if (bottomContainer != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(bottomContainer);

            Canvas.ForceUpdateCanvases();

            SetBarWidths();

            StartCoroutine(DelayedLayoutUpdate());
        }

        private void SetBarWidths()
        {
            if (_valueElements == null || _indicators == null)
                return;

            int count = Mathf.Min(_valueElements.Count, _indicators.Count);
            var topContainer = _valueElements.CurrentParent;
            var bottomContainer = _indicators.CurrentParent;
            
            if (count > 0 && topContainer != null && bottomContainer != null)
            {
                var topLayoutGroup = topContainer.GetComponent<HorizontalLayoutGroup>();
                var bottomLayoutGroup = bottomContainer.GetComponent<HorizontalLayoutGroup>();

                float topSpacing = topLayoutGroup != null ? topLayoutGroup.spacing : 0f;
                float bottomSpacing = bottomLayoutGroup != null ? bottomLayoutGroup.spacing : 0f;
                float spacing = Mathf.Max(topSpacing, bottomSpacing);

                float containerWidth = topContainer.rect.width;
                if (containerWidth <= 0) containerWidth = bottomContainer.rect.width;

                if (containerWidth > 0)
                {
                    _barWidth = (containerWidth - spacing * (count - 1)) / count;

                    for (int i = 0; i < count; i++)
                    {
                        var valueElement = _valueElements[i];
                        if (valueElement != null)
                        {
                            var valueRect = valueElement.transform as RectTransform;
                            if (valueRect != null)
                                valueRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _barWidth);
                        }
                        
                        var indicator = _indicators[i];
                        if (indicator != null)
                        {
                            var indicatorRect = indicator.transform as RectTransform;
                            if (indicatorRect != null)
                                indicatorRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _barWidth);
                        }
                    }
                }
            }
        }

        private System.Collections.IEnumerator DelayedLayoutUpdate()
        {
            yield return null;

            if (_valueElements == null || _indicators == null)
                yield break;

            var topContainer = _valueElements.CurrentParent;
            var bottomContainer = _indicators.CurrentParent;

            if (topContainer != null && bottomContainer != null)
            {
                var topLayoutElement = topContainer.GetComponent<LayoutElement>();
                var bottomLayoutElement = bottomContainer.GetComponent<LayoutElement>();

                if (topLayoutElement == null)
                    topLayoutElement = topContainer.gameObject.AddComponent<LayoutElement>();
                if (bottomLayoutElement == null)
                    bottomLayoutElement = bottomContainer.gameObject.AddComponent<LayoutElement>();

                float containerHeight = Mathf.Max(topContainer.rect.height, bottomContainer.rect.height);
                if (containerHeight > 0)
                {
                    topLayoutElement.preferredHeight = containerHeight;
                    bottomLayoutElement.preferredHeight = containerHeight;
                }
            }

            if (topContainer != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(topContainer);
            if (bottomContainer != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(bottomContainer);

            Canvas.ForceUpdateCanvases();

            SetBarWidths();
        }

        private void OnDestroy()
        {
            Clear();
        }
    }
}
