using System.Linq;
using UnityEngine;

namespace Miyo.Data
{
    [CreateAssetMenu(menuName = "Miyo/Data/GameDatabase")]
    public class GameDatabase : ScriptableObject
    {
        [SerializeField] private GameDefinition[] _games;
        [SerializeField] private CategoryDefinition[] _categories;

        public GameDefinition[] Games => _games;
        public CategoryDefinition[] Categories => _categories;

        public GameDefinition GetGame(string gameName) =>
            _games.FirstOrDefault(g => g.GameName == gameName);

        public CategoryDefinition GetCategory(string categoryName) =>
            _categories.FirstOrDefault(c => c.CategoryName == categoryName);
    }
}
