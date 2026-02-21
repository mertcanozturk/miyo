using TMPro;
using UnityEngine;

namespace Miyo.UI
{
    public class PlayedTimeByDateSummary : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _timeText;

        public void UpdateSummary(int tabIndex, int totalMinutes)
        {
            if (_titleText != null)
            {
                // To match the design, we keep it static "TOPLAM SÜRE".
                // If we want it dynamic per tab later, uncomment below:
                /*
                _titleText.text = tabIndex switch {
                    1 => "HAFTALIK SÜRE",
                    2 => "AYLIK SÜRE",
                    _ => "BUGÜNKÜ SÜRE"
                };
                */
                _titleText.text = "TOPLAM SÜRE";
            }

            if (_timeText != null)
            {
                _timeText.text = FormatMinutes(totalMinutes);
            }
        }

        private string FormatMinutes(int minutes)
        {
            if (minutes == 0) return "0 dk";
            int h = minutes / 60;
            int m = minutes % 60;
            return h > 0 ? $"{h}s {m} dk" : $"{m} dk";
        }
    }
}
