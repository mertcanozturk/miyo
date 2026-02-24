using System;
using UnityEngine;

namespace Miyo.Games
{
    [CreateAssetMenu(menuName = "Miyo/Games/Match Numbers Configuration")]
    public class MatchNumbersConfiguration : ScriptableObject
    {
        [SerializeField] private LevelDefinition[] _levels;

        [Serializable]
        public struct LevelDefinition
        {
            public int levelCount; // bu band'ı kullanan ardışık level sayısı
            public int pairCount;  // tahtadaki eşleşme çifti sayısı
            public int maxNumber;  // sayılar [1, maxNumber] aralığından seçilir
        }

        public LevelDefinition GetDefinition(int levelIndex)
        {
            int accumulated = 0;
            foreach (var def in _levels)
            {
                accumulated += def.levelCount;
                if (accumulated > levelIndex)
                    return def;
            }
            return _levels[_levels.Length - 1];
        }
    }
}
