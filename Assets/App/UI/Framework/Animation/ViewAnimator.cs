using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;

namespace Miyo.UI.MVVM
{
    public enum SlideDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    public static class ViewAnimator
    {
        private const float DefaultFadeDuration = 0.35f;
        private const float DefaultSlideDuration = 0.4f;
        private const float DefaultScaleDuration = 0.3f;

        public static async UniTask FadeIn(CanvasGroup canvasGroup, float duration = DefaultFadeDuration)
        {
            canvasGroup.alpha = 0f;
            await LMotion.Create(0f, 1f, duration)
                .WithEase(Ease.OutCubic)
                .Bind(canvasGroup, static (x, cg) => cg.alpha = x)
                .ToUniTask();
        }

        public static async UniTask FadeOut(CanvasGroup canvasGroup, float duration = DefaultFadeDuration)
        {
            await LMotion.Create(1f, 0f, duration)
                .WithEase(Ease.InCubic)
                .Bind(canvasGroup, static (x, cg) => cg.alpha = x)
                .ToUniTask();
        }

        public static async UniTask SlideIn(
            RectTransform rectTransform,
            CanvasGroup canvasGroup,
            SlideDirection direction = SlideDirection.Right,
            float duration = DefaultSlideDuration)
        {
            var offset = GetSlideOffset(direction, rectTransform);
            var targetPos = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = targetPos + offset;
            canvasGroup.alpha = 0f;

            await UniTask.WhenAll(
                LMotion.Create(targetPos + offset, targetPos, duration)
                    .WithEase(Ease.OutCubic)
                    .Bind(rectTransform, static (x, rt) => rt.anchoredPosition = x)
                    .ToUniTask(),
                LMotion.Create(0f, 1f, duration)
                    .WithEase(Ease.OutCubic)
                    .Bind(canvasGroup, static (x, cg) => cg.alpha = x)
                    .ToUniTask()
            );
        }

        public static async UniTask SlideOut(
            RectTransform rectTransform,
            CanvasGroup canvasGroup,
            SlideDirection direction = SlideDirection.Left,
            float duration = DefaultSlideDuration)
        {
            var startPos = rectTransform.anchoredPosition;
            var offset = GetSlideOffset(direction, rectTransform);

            await UniTask.WhenAll(
                LMotion.Create(startPos, startPos + offset, duration)
                    .WithEase(Ease.InCubic)
                    .Bind(rectTransform, static (x, rt) => rt.anchoredPosition = x)
                    .ToUniTask(),
                LMotion.Create(1f, 0f, duration)
                    .WithEase(Ease.InCubic)
                    .Bind(canvasGroup, static (x, cg) => cg.alpha = x)
                    .ToUniTask()
            );
        }

        public static async UniTask ScaleIn(Transform transform, CanvasGroup canvasGroup, float duration = DefaultScaleDuration)
        {
            transform.localScale = Vector3.one * 0.8f;
            canvasGroup.alpha = 0f;

            await UniTask.WhenAll(
                LMotion.Create(Vector3.one * 0.8f, Vector3.one, duration)
                    .WithEase(Ease.OutBack)
                    .Bind(transform, static (x, t) => t.localScale = x)
                    .ToUniTask(),
                LMotion.Create(0f, 1f, duration)
                    .WithEase(Ease.OutCubic)
                    .Bind(canvasGroup, static (x, cg) => cg.alpha = x)
                    .ToUniTask()
            );
        }

        public static async UniTask ScaleOut(Transform transform, CanvasGroup canvasGroup, float duration = DefaultScaleDuration)
        {
            await UniTask.WhenAll(
                LMotion.Create(Vector3.one, Vector3.one * 0.8f, duration)
                    .WithEase(Ease.InCubic)
                    .Bind(transform, static (x, t) => t.localScale = x)
                    .ToUniTask(),
                LMotion.Create(1f, 0f, duration)
                    .WithEase(Ease.InCubic)
                    .Bind(canvasGroup, static (x, cg) => cg.alpha = x)
                    .ToUniTask()
            );
        }

        private static Vector2 GetSlideOffset(SlideDirection direction, RectTransform rectTransform)
        {
            var rect = rectTransform.rect;
            return direction switch
            {
                SlideDirection.Left => new Vector2(-rect.width, 0),
                SlideDirection.Right => new Vector2(rect.width, 0),
                SlideDirection.Up => new Vector2(0, rect.height),
                SlideDirection.Down => new Vector2(0, -rect.height),
                _ => Vector2.zero
            };
        }
    }
}
