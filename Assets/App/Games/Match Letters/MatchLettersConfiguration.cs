using UnityEngine;
using System;
namespace Miyo.Games
{
    public class MatchLettersConfiguration : ScriptableObject
    {
        [SerializeField] private LevelDefinition[] _levels;

        public LevelDefinition[] Levels => _levels;

        [Serializable]
        public struct LevelDefinition{
            public int levelCount;
            public int letterCount;
            public bool isCaseSensitive;
        }

        public LevelDefinition GetDefinition(int levelIndex)
        {
            int level = 0;
            foreach (var levelDefinition in _levels)
            {
                level += levelDefinition.levelCount;
                if (level > levelIndex)
                {
                    return levelDefinition;
                }
            }
            return _levels[_levels.Length - 1];
        }
    }
}
