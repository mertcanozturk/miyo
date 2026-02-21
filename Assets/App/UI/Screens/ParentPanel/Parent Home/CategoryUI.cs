using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Miyo.Data;
namespace Miyo.UI
{
    public class CategoryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _categoryNameText;
        [SerializeField] private Image _categoryBg;
        [SerializeField] private Image _categoryIcon;
        [SerializeField] private bool _updateTextColor = true;

        public void SetCategory(CategoryDefinition category)
        {

            if(_categoryNameText != null)
            {
                _categoryNameText.text = category.CategoryName;
                if(_updateTextColor)
                {
                    _categoryNameText.color = category.CategoryColor;
                }
            }

            if(_categoryBg != null)
            {
                _categoryBg.color = category.CategoryBgColor;
            }

            if(_categoryIcon != null)
            {
                _categoryIcon.sprite = category.CategoryIcon;
            }
        }
    }
}
