using UnityEngine;

namespace Miyo.Games
{
    public readonly struct MatchPuzzleCardContent
    {
        public enum ContentKind { Text, Sprite }

        public readonly ContentKind Kind;
        public readonly string Text;
        public readonly Sprite SpriteRef;
        /// <summary>
        /// Eşleşme kontrolü bu alan üzerinden yapılır.
        /// İki kartın MatchKey değeri aynıysa eşleşmiş kabul edilir.
        /// </summary>
        public readonly string MatchKey;

        public static MatchPuzzleCardContent FromText(string display, string matchKey)
            => new MatchPuzzleCardContent(ContentKind.Text, display, null, matchKey);

        public static MatchPuzzleCardContent FromSprite(Sprite sprite, string matchKey)
            => new MatchPuzzleCardContent(ContentKind.Sprite, null, sprite, matchKey);

        private MatchPuzzleCardContent(ContentKind kind, string text, Sprite sprite, string matchKey)
        {
            Kind = kind;
            Text = text;
            SpriteRef = sprite;
            MatchKey = matchKey;
        }
    }
}
