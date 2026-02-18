using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace Miyo.UI
{
    public class MailValidator : MonoBehaviour, IStringFieldValidator
    {
        [SerializeField] private TMP_Text _errorText;
        [SerializeField] private string _invalidMailMessage = "Geçerli bir e-posta adresi giriniz.";
        [SerializeField] private string _usedErrorMessage = "Bu e-posta adresi zaten kullanılmış.";

        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        void Awake()
        {
            _errorText.text = string.Empty;
        }


        public bool Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                if (_errorText != null)
                    _errorText.text = _invalidMailMessage;
                return false;
            }

            if (!IsValidEmail(value))
            {
                if (_errorText != null)
                    _errorText.text = _invalidMailMessage;
                return false;
            }

            if (IsMailUsedBefore(value))
            {
                if (_errorText != null)
                    _errorText.text = _usedErrorMessage;
                return false;
            }

            if (_errorText != null)
                _errorText.text = "";
            return true;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            if (!EmailRegex.IsMatch(email))
                return false;

            if (email.Length > 254) 
                return false;

            if (email.StartsWith(".") || email.StartsWith("@") || email.EndsWith(".") || email.EndsWith("@"))
                return false;

            int atIndex = email.IndexOf('@');
            if (atIndex <= 0 || atIndex >= email.Length - 1)
                return false;

            string domain = email.Substring(atIndex + 1);
            if (!domain.Contains("."))
                return false;

            return true;
        }
        private bool IsMailUsedBefore(string email)
        {
            //TODO: Implement mail used before validation
            return false;
        }
    }
}
