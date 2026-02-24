using System;
using System.Collections.Generic;
using System.Globalization;
using Miyo.Core.Extensions;
using Miyo.Data;
using UnityEngine;

namespace Miyo.Games
{
    public class MatchLetters : MatchPuzzleGame<MatchLetters.MatchLettersSaveData>
    {
        [SerializeField] private MatchLettersConfiguration _configuration;

        private readonly CultureInfo _cultureInfo = new CultureInfo("tr-TR");
        private ChildSaveEntry _childSave;

        protected override string GameId => "match_letters";

        // ── GameBase ─────────────────────────────────────────────────

        protected override void OnInitialize(string childName, MatchLettersSaveData saveData)
        {
            _childSave = saveData.GetOrCreate(childName);
        }

        // ── MatchPuzzleGame Contract ──────────────────────────────────

        protected override int GetChildLevelIndex(MatchLettersSaveData saveData)
            => _childSave.levelIndex;

        protected override void IncrementChildLevelIndex(MatchLettersSaveData saveData)
            => _childSave.levelIndex++;

        protected override List<MatchPuzzleCardContent> GenerateLevel(int levelIndex)
        {
            var definition = _configuration.GetDefinition(levelIndex);
            var letters = SelectUniqueLetters(definition.letterCount, definition.isCaseSensitive);

            var cards = new List<MatchPuzzleCardContent>(letters.Count);
            foreach (var letter in letters)
            {
                // MatchKey: küçük harfli Türkçe form — hem büyük hem küçük harf kartları eşleşir
                string matchKey = letter.ToLower(_cultureInfo);
                cards.Add(MatchPuzzleCardContent.FromText(letter, matchKey));
            }
            cards.Shuffle();
            return cards;
        }

        private List<string> SelectUniqueLetters(int count, bool isCaseSensitive)
        {
            var available = new List<string>();
            for (int i = 0; i < count; i++)
            {
                while (true)
                {
                    int rndIndex = UnityEngine.Random.Range(0, Constants.TurkishLettersUppercase.Length);
                    if (!available.Contains(Constants.TurkishLettersUppercase[rndIndex]))
                    {
                        available.Add(Constants.TurkishLettersUppercase[rndIndex]);
                        if (isCaseSensitive)
                            available.Add(Constants.TurkishLettersLowercase[rndIndex]);
                        else
                            available.Add(Constants.TurkishLettersUppercase[rndIndex]);
                        break;
                    }
                }
            }
            available.Shuffle();
            return available;
        }

        // ── Save Data ────────────────────────────────────────────────
        // MatchPuzzleSaveData'dan türer; JSON alanı ('saves') aynı kalır, mevcut kayıtlar korunur.
        [Serializable]
        public class MatchLettersSaveData : MatchPuzzleSaveData { }
    }
}
