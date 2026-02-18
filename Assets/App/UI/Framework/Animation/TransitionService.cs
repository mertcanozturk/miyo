using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;

namespace Miyo.UI.MVVM
{
    public enum TransitionType
    {
        Fade,
        MiyoWalk,
        StarWipe
    }

    public interface ITransitionService
    {
        bool IsTransitioning { get; }
        UniTask PlayTransition(Action onMidpoint, TransitionType type = TransitionType.Fade);
        UniTask PlayOut(TransitionType type = TransitionType.Fade);
        UniTask PlayIn(TransitionType type = TransitionType.Fade);
    }

    public class TransitionService : MonoBehaviour, ITransitionService
    {
        [SerializeField] private CanvasGroup _fadeOverlay;
        [SerializeField] private float _transitionDuration = 0.5f;

        public bool IsTransitioning { get; private set; }

        public async UniTask PlayOut(TransitionType type = TransitionType.Fade)
        {
            IsTransitioning = true;
            _fadeOverlay.gameObject.SetActive(true);
            _fadeOverlay.alpha = 0f;

            await LMotion.Create(0f, 1f, _transitionDuration)
                .WithEase(Ease.InOutQuad)
                .Bind(_fadeOverlay, static (x, cg) => cg.alpha = x)
                .ToUniTask();
        }

        public async UniTask PlayIn(TransitionType type = TransitionType.Fade)
        {
            await LMotion.Create(1f, 0f, _transitionDuration)
                .WithEase(Ease.InOutQuad)
                .Bind(_fadeOverlay, static (x, cg) => cg.alpha = x)
                .ToUniTask();

            _fadeOverlay.gameObject.SetActive(false);
            IsTransitioning = false;
        }

        public async UniTask PlayTransition(Action onMidpoint, TransitionType type = TransitionType.Fade)
        {
            await PlayOut(type);
            onMidpoint?.Invoke();
            await PlayIn(type);
        }
    }
}
