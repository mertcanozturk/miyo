using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Miyo.UI
{
    public class ChildSelectorItemUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private Image _avatarImage;
        [SerializeField] private Sprite _defaultAvatar;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _selectedIndicator;

        private string _childId;
        private Action<string> _onSelected;

        private void Awake()
        {
            _button.onClick.AddListener(() => _onSelected?.Invoke(_childId));
        }

        public void Prepare(string childId, string childName, bool isSelected, Sprite avatar, Action<string> onSelected)
        {
            _childId = childId;
            _onSelected = onSelected;
            _nameText.text = childName;
            SetAvatar(avatar);
            SetSelected(isSelected);
        }

        public void PrepareAsAction(Action onClick)
        {
            _childId = null;
            _onSelected = _ => onClick?.Invoke();
        }

        public void SetAvatar(Sprite avatar)
        {
            if (_avatarImage != null)
                _avatarImage.sprite = avatar != null ? avatar : _defaultAvatar;
        }

        public void SetSelected(bool selected)
        {
            if (_selectedIndicator != null)
                _selectedIndicator.SetActive(selected);
        }
    }
}
