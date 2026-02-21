using UnityEngine;

namespace Miyo.Data
{
    [CreateAssetMenu(menuName = "Miyo/Data/Game/CategoryDefinition")]
    public class CategoryDefinition : ScriptableObject
    {
        [SerializeField] private string categoryName;
        [SerializeField] private Sprite categoryIcon;
        [SerializeField] private Color categoryColor;
        [SerializeField] private Color categoryBgColor;
        public string CategoryName => categoryName;
        public Color CategoryColor => categoryColor;
        public Color CategoryBgColor => categoryBgColor;
        public Sprite CategoryIcon => categoryIcon;
    }
}