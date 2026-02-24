using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;

namespace Miyo.Games
{
    public static class CardAnimator
    {
        private const float FlipHalfDuration = 0.15f;
        private const float BounceDuration = 0.3f;
        private const float ShakeDuration = 0.3f;
        private const float DealScaleDuration = 0.25f;
        private const float DealStaggerDelay = 0.05f;
        private const float WaveStaggerDelay = 0.08f;

        public static async UniTask FlipCard(MatchPuzzleCard card, bool toOpen,
            CancellationToken ct = default)
        {
            var t = card.transform;

            await LMotion.Create(1f, 0f, FlipHalfDuration)
                .WithEase(Ease.InCubic)
                .Bind(t, static (x, tr) =>
                {
                    var s = tr.localScale;
                    s.x = x;
                    tr.localScale = s;
                })
                .ToUniTask(cancellationToken: ct);

            card.SetOpen(toOpen);

            await LMotion.Create(0f, 1f, FlipHalfDuration)
                .WithEase(Ease.OutCubic)
                .Bind(t, static (x, tr) =>
                {
                    var s = tr.localScale;
                    s.x = x;
                    tr.localScale = s;
                })
                .ToUniTask(cancellationToken: ct);
        }

        public static async UniTask MatchBounce(MatchPuzzleCard card,
            CancellationToken ct = default)
        {
            var t = card.transform;

            await LMotion.Create(1f, 1.2f, BounceDuration * 0.4f)
                .WithEase(Ease.OutBack)
                .Bind(t, static (x, tr) => tr.localScale = Vector3.one * x)
                .ToUniTask(cancellationToken: ct);

            await LMotion.Create(1.2f, 1f, BounceDuration * 0.6f)
                .WithEase(Ease.OutCubic)
                .Bind(t, static (x, tr) => tr.localScale = Vector3.one * x)
                .ToUniTask(cancellationToken: ct);
        }

        public static async UniTask MatchColorFlash(MatchPuzzleCard card, Color highlightColor,
            CancellationToken ct = default)
        {
            if (card.CardFront == null) return;

            var original = card.CardFront.color;

            await LMotion.Create(original, highlightColor, 0.15f)
                .WithEase(Ease.OutCubic)
                .Bind(card.CardFront, static (c, img) => img.color = c)
                .ToUniTask(cancellationToken: ct);

            await LMotion.Create(highlightColor, original, 0.25f)
                .WithEase(Ease.InCubic)
                .Bind(card.CardFront, static (c, img) => img.color = c)
                .ToUniTask(cancellationToken: ct);
        }

        public static async UniTask Shake(MatchPuzzleCard card,
            CancellationToken ct = default)
        {
            var rt = card.RectTransform;
            var originalPos = rt.anchoredPosition;

            await LMotion.Create(0f, 1f, ShakeDuration)
                .WithEase(Ease.OutCubic)
                .Bind(rt, (progress, r) =>
                {
                    float amplitude = 8f * (1f - progress);
                    float offset = Mathf.Sin(progress * Mathf.PI * 6f) * amplitude;
                    r.anchoredPosition = originalPos + new Vector2(offset, 0);
                })
                .ToUniTask(cancellationToken: ct);

            rt.anchoredPosition = originalPos;
        }

        public static async UniTask DealCards(MatchPuzzleCard[] cards,
            CancellationToken ct = default)
        {
            var tasks = new UniTask[cards.Length];
            for (int i = 0; i < cards.Length; i++)
            {
                tasks[i] = DealSingleCard(cards[i], i * DealStaggerDelay, ct);
            }
            await UniTask.WhenAll(tasks);
        }

        private static async UniTask DealSingleCard(MatchPuzzleCard card, float delay,
            CancellationToken ct)
        {
            if (delay > 0)
                await UniTask.Delay((int)(delay * 1000), cancellationToken: ct);

            card.CanvasGroup.alpha = 1f;

            await LMotion.Create(Vector3.zero, Vector3.one, DealScaleDuration)
                .WithEase(Ease.OutBack)
                .Bind(card.transform, static (v, t) => t.localScale = v)
                .ToUniTask(cancellationToken: ct);
        }

        public static async UniTask WaveAnimation(MatchPuzzleCard[] cards,
            CancellationToken ct = default)
        {
            var tasks = new UniTask[cards.Length];
            for (int i = 0; i < cards.Length; i++)
            {
                tasks[i] = WaveSingleCard(cards[i], i * WaveStaggerDelay, ct);
            }
            await UniTask.WhenAll(tasks);
        }

        private static async UniTask WaveSingleCard(MatchPuzzleCard card, float delay,
            CancellationToken ct)
        {
            if (delay > 0)
                await UniTask.Delay((int)(delay * 1000), cancellationToken: ct);

            var t = card.transform;

            await LMotion.Create(1f, 1.15f, 0.15f)
                .WithEase(Ease.OutCubic)
                .Bind(t, static (x, tr) => tr.localScale = Vector3.one * x)
                .ToUniTask(cancellationToken: ct);

            await LMotion.Create(1.15f, 1f, 0.15f)
                .WithEase(Ease.InCubic)
                .Bind(t, static (x, tr) => tr.localScale = Vector3.one * x)
                .ToUniTask(cancellationToken: ct);
        }
    }
}
