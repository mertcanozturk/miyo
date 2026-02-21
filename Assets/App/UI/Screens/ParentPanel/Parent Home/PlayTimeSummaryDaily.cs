using System.Collections.Generic;
using Miyo.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Miyo.UI.Screens
{
    public class PlayTimeSummaryDaily : MonoBehaviour
    {
        [Header("Progress")]
        [SerializeField] private Image _progressImage;
        [SerializeField] private TMP_Text _playedMinutesText;

        [Header("Summary")]
        [SerializeField] private TMP_Text _summaryText;
        [SerializeField] private string _formatText = "{childName} bugün toplam <color=#{colorHex}>{totalLimit} dk</color> günlük limitinin <color=#{colorHex}>%{percentage}</color> kadarını kullandı.";
        [SerializeField] private Color _summaryHighlightColor = new Color(0.91f, 0.486f, 0.224f); // #E87C39

        [Header("Categories")]
        [SerializeField] private UICollection<CategoryUI> _categoryCollection;

        public void Prepare(string childName, List<(CategoryDefinition category, int playedMinutes, int limitMinutes)> categoryDataList)
        {
            if (categoryDataList == null || categoryDataList.Count == 0)
            {
                _categoryCollection.Count = 0;
                return;
            }

            int totalPlayed = 0;
            int totalLimit = 0;

            _categoryCollection.Count = categoryDataList.Count;

            for (int i = 0; i < categoryDataList.Count; i++)
            {
                var data = categoryDataList[i];
                totalPlayed += data.playedMinutes;
                totalLimit += data.limitMinutes;

                var categoryUI = _categoryCollection[i];
                if (data.category != null)
                {
                    categoryUI.SetCategory(data.category);
                }
            }

            UpdateVisuals(childName, totalPlayed, totalLimit);
        }

        private void UpdateVisuals(string childName, int totalPlayed, int totalLimit)
        {
            float fillAmount = 0f;
            int percentage = 0;

            if (totalLimit > 0)
            {
                fillAmount = Mathf.Clamp01((float)totalPlayed / totalLimit);
                percentage = Mathf.RoundToInt(fillAmount * 100f);
            }

            if (_progressImage != null)
            {
                _progressImage.fillAmount = fillAmount;
            }

            if (_playedMinutesText != null)
            {
                _playedMinutesText.text = totalPlayed.ToString();
            }

            if (_summaryText != null)
            {
                string colorHex = ColorUtility.ToHtmlStringRGB(_summaryHighlightColor);

                _summaryText.text = _formatText.Replace("{childName}", childName)
                    .Replace("{colorHex}", colorHex)
                    .Replace("{totalLimit}", totalLimit.ToString())
                    .Replace("{percentage}", percentage.ToString());
            }
        }
    }
}
