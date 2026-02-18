using TMPro;
using UnityEngine;

namespace Miyo.UI
{
    public class EmptyFieldValidator : MonoBehaviour, IStringFieldValidator
    {
        [SerializeField] private TMP_Text _errorText;
        [SerializeField] private string _errorMessage = "Bu alan boş bırakılamaz.";

        void Awake()
        {
            _errorText.text = string.Empty;
        }

        public bool Validate(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                _errorText.text = _errorMessage;
                return false;
            }
            _errorText.text = "";
            return true;
        }
    }
}
