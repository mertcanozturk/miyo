using TMPro;
using UnityEngine;

namespace Miyo.UI
{
    public class PasswordValidator : MonoBehaviour, IStringFieldValidator
    {
        [SerializeField] private int lenght = 6;
        [SerializeField] private TMP_Text _errorText;
        [SerializeField] private string _errorMessage;

        void Awake()
        {
            _errorText.text = string.Empty;
        }

        public bool Validate(string value)
        {
            if (value.Length != lenght)
            {
                _errorText.text = _errorMessage;
                return false;
            }
            _errorText.text = "";
            return true;
        }
    }
}
