using System;
using System.Collections.Generic;
using Miyo.Data;
using UnityEngine;

namespace Miyo.UI
{
    public class ChildSelectorView : MonoBehaviour
    {
        private const int AddButtonResId = 1;

        [SerializeField] private UICollection<ChildSelectorItemUI> _childItems;

        public event Action<string> OnChildSelected;
        public event Action OnAddChildClicked;

        private string _selectedChildId;

        public void SetChildren(List<ChildProfile> children, string selectedChildId,bool showAddButton = true)
        {
            _selectedChildId = selectedChildId;
            _childItems.Count = 0;

            for (int i = 0; i < children.Count; i++)
            {
                var item = _childItems.AddElement();
                var child = children[i];
                bool isSelected = child.Id == selectedChildId;
                item.Prepare(child.Id, child.Name, isSelected, null, OnItemSelected);
            }

            if (showAddButton)
            {
                var addBtn = _childItems.AddElement(AddButtonResId);
                addBtn.PrepareAsAction(() => OnAddChildClicked?.Invoke());
            }
        }

        public void UpdateSelection(string selectedChildId)
        {
            _selectedChildId = selectedChildId;
            for (int i = 0; i < _childItems.Count; i++)
            {
                _childItems[i].SetSelected(_childItems[i].name == selectedChildId);
            }
        }

        private void OnItemSelected(string childId)
        {
            _selectedChildId = childId;
            OnChildSelected?.Invoke(childId);
        }
    }
}
