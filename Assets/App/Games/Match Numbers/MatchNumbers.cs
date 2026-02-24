using System;
using System.Collections.Generic;
using Miyo.Core.Extensions;
using UnityEngine;

namespace Miyo.Games
{
    public class MatchNumbers : MatchPuzzleGame<MatchNumbers.MatchNumbersSaveData>
    {
        [SerializeField] private MatchNumbersConfiguration _configuration;

        private ChildSaveEntry _childSave;

        protected override string GameId => "match_numbers";

        // ── GameBase ─────────────────────────────────────────────────

        protected override void OnInitialize(string childName, MatchNumbersSaveData saveData)
        {
            _childSave = saveData.GetOrCreate(childName);
        }

        // ── MatchPuzzleGame Contract ──────────────────────────────────

        protected override int GetChildLevelIndex(MatchNumbersSaveData saveData)
            => _childSave.levelIndex;

        protected override void IncrementChildLevelIndex(MatchNumbersSaveData saveData)
            => _childSave.levelIndex++;

        protected override List<MatchPuzzleCardContent> GenerateLevel(int levelIndex)
        {
            var definition = _configuration.GetDefinition(levelIndex);

            var chosenNumbers = new List<int>(definition.pairCount);
            while (chosenNumbers.Count < definition.pairCount)
            {
                int n = UnityEngine.Random.Range(1, definition.maxNumber + 1);
                if (!chosenNumbers.Contains(n))
                    chosenNumbers.Add(n);
            }

            var cards = new List<MatchPuzzleCardContent>(chosenNumbers.Count * 2);
            foreach (int n in chosenNumbers)
            {
                string display = n.ToString();
                cards.Add(MatchPuzzleCardContent.FromText(display, display));
                cards.Add(MatchPuzzleCardContent.FromText(display, display));
            }
            cards.Shuffle();
            return cards;
        }

        // ── Save Data ────────────────────────────────────────────────

        [Serializable]
        public class MatchNumbersSaveData : MatchPuzzleSaveData { }
    }
}
