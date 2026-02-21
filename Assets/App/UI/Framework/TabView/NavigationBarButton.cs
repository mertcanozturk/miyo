using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Miyo.UI
{
    [RequireComponent(typeof(Button))]
    public class NavigationBarButton : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Shadow _shadow;

        [Header("Selected Style")]
        [SerializeField] private Color _selectedBackgroundColor = Color.white;
        [SerializeField] private Color _selectedTextColor = new Color(0.2f, 0.2f, 0.2f);
        [SerializeField] private FontWeight _selectedFontWeight = FontWeight.Bold;

        [Header("Normal Style")]
        [SerializeField] private Color _normalBackgroundColor = new Color(1f, 1f, 1f, 0f);
        [SerializeField] private Color _normalTextColor = new Color(0.5f, 0.55f, 0.65f);
        [SerializeField] private FontWeight _normalFontWeight = FontWeight.Regular;

        private Button _button;
        private int _index;

        public int Index => _index;
        public event Action<int> Clicked;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button != null)
                _button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnClick);
        }

        public void Setup(int index)
        {
            _index = index;
        }

        public void SetLabel(string text)
        {
            if (_label != null)
                _label.text = text;
        }

        public void SetSelected(bool selected)
        {
            if (_background != null)
            {
                _background.color = selected ? _selectedBackgroundColor : _normalBackgroundColor;
                _background.raycastTarget = false;
            }

            if (_shadow != null)
                _shadow.enabled = selected;

            if (_label != null)
            {
                _label.color = selected ? _selectedTextColor : _normalTextColor;
                _label.fontWeight = selected ? _selectedFontWeight : _normalFontWeight;
            }
        }

        private void OnClick()
        {
            Clicked?.Invoke(_index);
        }
    }
}
