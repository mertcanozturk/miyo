using UnityEngine;

namespace Miyo.Data
{
    [CreateAssetMenu(menuName = "Miyo/Data/Game/GameDefinition")]
    public class GameDefinition : ScriptableObject
    {
        [SerializeField] private string gameName;
        [SerializeField] private Sprite gameIcon;
        [SerializeField] private CategoryDefinition category;
        public string GameName => gameName;
        public CategoryDefinition Category => category;
        public Sprite GameIcon => gameIcon;
    }
}