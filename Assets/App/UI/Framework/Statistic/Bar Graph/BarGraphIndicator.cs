using UnityEngine;
using TMPro;

namespace Miyo.UI
{
    public class BarGraphIndicator : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;

        public void SetLabel(string label)
        {
            if (_label != null)
                _label.text = label;
        }
    }
}
