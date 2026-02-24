using System;
using System.Collections.Generic;
using Miyo.Core.Extensions;
using UnityEngine;

namespace Miyo.Games
{
    public class MatchImages : MatchPuzzleGame<MatchImages.MatchImagesSaveData>
    {
        [SerializeField] private MatchImagesConfiguration _configuration;

        private ChildSaveEntry _childSave;

        protected override string GameId => "match_images";

        // ── GameBase ─────────────────────────────────────────────────

        protected override void OnInitialize(string childName, MatchImagesSaveData saveData)
        {
            _childSave = saveData.GetOrCreate(childName);
        }

        // ── MatchPuzzleGame Contract ──────────────────────────────────

        protected override int GetChildLevelIndex(MatchImagesSaveData saveData)
            => _childSave.levelIndex;

        protected override void IncrementChildLevelIndex(MatchImagesSaveData saveData)
            => _childSave.levelIndex++;

        protected override List<MatchPuzzleCardContent> GenerateLevel(int levelIndex)
        {
            var definition = _configuration.GetDefinition(levelIndex);

            // Sprite havuzunu karıştır ve pairCount kadar seç
            var pool = new List<Sprite>(definition.sprites);
            pool.Shuffle();

            int pairCount = Mathf.Min(definition.pairCount, pool.Count);
            var cards = new List<MatchPuzzleCardContent>(pairCount * 2);

            for (int i = 0; i < pairCount; i++)
            {
                Sprite sprite = pool[i];
                // MatchKey olarak sprite adını kullan — aynı sprite'ın iki kartı eşleşir
                string matchKey = sprite.name;
                cards.Add(MatchPuzzleCardContent.FromSprite(sprite, matchKey));
                cards.Add(MatchPuzzleCardContent.FromSprite(sprite, matchKey));
            }
            cards.Shuffle();
            return cards;
        }

        // ── Save Data ────────────────────────────────────────────────

        [Serializable]
        public class MatchImagesSaveData : MatchPuzzleSaveData { }
    }
}
