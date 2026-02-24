using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Miyo.Core.Events;
using Miyo.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Miyo.Games
{
    /// <summary>
    /// Tüm eşleştirme (memory matching) oyunları için ortak abstract base.
    /// Subclass'lar sadece: GameId, GenerateLevel(), OnInitialize(), GetChildLevelIndex(), IncrementChildLevelIndex() implemente eder.
    /// </summary>
    public abstract class MatchPuzzleGame<TSaveData> : GameBase<TSaveData>
        where TSaveData : class, new()
    {
        [SerializeField] protected UICollection<MatchPuzzleCard> _puzzleCards;
        [SerializeField] protected Button _backButton;
        [SerializeField] protected Color _matchHighlightColor = new Color(0.4f, 0.9f, 0.4f, 1f);

        // ── State Machine ────────────────────────────────────────────

        private enum GamePhase
        {
            Dealing,
            WaitingForFirstCard,
            WaitingForSecondCard,
            EvaluatingMatch,
            LevelComplete
        }

        private GamePhase _currentPhase;
        private List<MatchPuzzleCardContent> _levelData;
        private MatchPuzzleCard[] _activeCards;
        private int _firstCardIndex = -1;
        private int _secondCardIndex = -1;
        private int _matchesFound;
        private int _totalPairs;
        private int _attempts;

        // ── Abstract Contract ────────────────────────────────────────

        /// <summary>
        /// Verilen level index için shuffle edilmiş kart içeriklerini üretir.
        /// Liste çift uzunlukta olmalı; aynı MatchKey'e sahip tam 2 eleman (çift) içermeli.
        /// </summary>
        protected abstract List<MatchPuzzleCardContent> GenerateLevel(int levelIndex);

        /// <summary>
        /// Save data'dan bu çocuğun mevcut level index'ini döndürür.
        /// </summary>
        protected abstract int GetChildLevelIndex(TSaveData saveData);

        /// <summary>
        /// Save data'daki level index'i bir artırır (level tamamlandığında çağrılır).
        /// </summary>
        protected abstract void IncrementChildLevelIndex(TSaveData saveData);

        // ── Back Button ──────────────────────────────────────────────

        private void OnEnable()
        {
            if (_backButton != null)
                _backButton.onClick.AddListener(ExitGame);
        }

        private void OnDisable()
        {
            if (_backButton != null)
                _backButton.onClick.RemoveListener(ExitGame);
        }

        // ── GameBase Template Methods ────────────────────────────────

        protected override void OnGameStart()
        {
            ResetTimer();
            StartLevel();
        }

        protected override void OnCleanup()
        {
            _activeCards = null;
        }

        // ── Core Game Logic ──────────────────────────────────────────

        private void StartLevel()
        {
            _firstCardIndex = -1;
            _secondCardIndex = -1;
            _matchesFound = 0;
            _attempts = 0;

            _levelData = GenerateLevel(GetChildLevelIndex(SaveData));
            _totalPairs = _levelData.Count / 2;

            _puzzleCards.Count = _levelData.Count;
            _activeCards = new MatchPuzzleCard[_levelData.Count];

            for (int i = 0; i < _levelData.Count; i++)
            {
                var card = _puzzleCards[i];
                card.Initialize(i, _levelData[i]);
                _activeCards[i] = card;

                int captured = i;
                card.Button.onClick.RemoveAllListeners();
                card.Button.onClick.AddListener(() => OnCardClicked(captured));
            }

            TransitionToPhase(GamePhase.Dealing);
        }

        private async void TransitionToPhase(GamePhase phase)
        {
            _currentPhase = phase;

            try
            {
                switch (phase)
                {
                    case GamePhase.Dealing:
                        SetAllCardsInteractable(false);
                        await CardAnimator.DealCards(_activeCards, destroyCancellationToken);
                        if (!IsPlaying) return;
                        TransitionToPhase(GamePhase.WaitingForFirstCard);
                        break;

                    case GamePhase.WaitingForFirstCard:
                        _firstCardIndex = -1;
                        _secondCardIndex = -1;
                        SetAllCardsInteractable(true);
                        break;

                    case GamePhase.LevelComplete:
                        SetAllCardsInteractable(false);
                        await HandleLevelCompleteAsync();
                        break;
                }
            }
            catch (OperationCanceledException) { }
        }

        private async void OnCardClicked(int index)
        {
            if (!IsPlaying) return;
            if (_activeCards[index].IsMatched || _activeCards[index].IsOpen) return;

            try
            {
                if (_currentPhase == GamePhase.WaitingForFirstCard)
                {
                    _firstCardIndex = index;
                    _currentPhase = GamePhase.WaitingForSecondCard;
                    SetAllCardsInteractable(false);

                    await CardAnimator.FlipCard(_activeCards[index], true, destroyCancellationToken);

                    if (_currentPhase == GamePhase.WaitingForSecondCard && IsPlaying)
                        SetAllCardsInteractable(true);
                }
                else if (_currentPhase == GamePhase.WaitingForSecondCard)
                {
                    if (index == _firstCardIndex) return;

                    _secondCardIndex = index;
                    _currentPhase = GamePhase.EvaluatingMatch;
                    SetAllCardsInteractable(false);

                    await CardAnimator.FlipCard(_activeCards[index], true, destroyCancellationToken);

                    if (!IsPlaying) return;
                    await EvaluateMatchAsync();
                }
            }
            catch (OperationCanceledException) { }
        }

        private async UniTask EvaluateMatchAsync()
        {
            _attempts++;
            bool isMatch = string.Equals(
                _levelData[_firstCardIndex].MatchKey,
                _levelData[_secondCardIndex].MatchKey,
                StringComparison.Ordinal);

            if (isMatch)
            {
                _matchesFound++;
                _activeCards[_firstCardIndex].SetMatched();
                _activeCards[_secondCardIndex].SetMatched();

                EventBus.Publish(new CorrectAnswerEvent
                {
                    GameId = GameId,
                    QuestionIndex = _matchesFound
                });

                await UniTask.WhenAll(
                    CardAnimator.MatchBounce(_activeCards[_firstCardIndex], destroyCancellationToken),
                    CardAnimator.MatchBounce(_activeCards[_secondCardIndex], destroyCancellationToken),
                    CardAnimator.MatchColorFlash(_activeCards[_firstCardIndex], _matchHighlightColor, destroyCancellationToken),
                    CardAnimator.MatchColorFlash(_activeCards[_secondCardIndex], _matchHighlightColor, destroyCancellationToken)
                );

                if (!IsPlaying) return;

                if (_matchesFound >= _totalPairs)
                {
                    TransitionToPhase(GamePhase.LevelComplete);
                    return;
                }
            }
            else
            {
                EventBus.Publish(new WrongAnswerEvent
                {
                    GameId = GameId,
                    QuestionIndex = _attempts
                });

                await UniTask.Delay(800);
                if (!IsPlaying) return;

                await UniTask.WhenAll(
                    CardAnimator.Shake(_activeCards[_firstCardIndex], destroyCancellationToken),
                    CardAnimator.Shake(_activeCards[_secondCardIndex], destroyCancellationToken)
                );

                if (!IsPlaying) return;

                await UniTask.WhenAll(
                    CardAnimator.FlipCard(_activeCards[_firstCardIndex], false, destroyCancellationToken),
                    CardAnimator.FlipCard(_activeCards[_secondCardIndex], false, destroyCancellationToken)
                );
            }

            if (!IsPlaying) return;
            TransitionToPhase(GamePhase.WaitingForFirstCard);
        }

        private async UniTask HandleLevelCompleteAsync()
        {
            await CardAnimator.WaveAnimation(_activeCards, destroyCancellationToken);
            if (!IsPlaying) return;

            int stars = _attempts <= _totalPairs ? 3
                      : _attempts <= _totalPairs * 2 ? 2
                      : 1;

            int completedLevel = GetChildLevelIndex(SaveData);
            IncrementChildLevelIndex(SaveData);

            await CompleteLevel(
                levelIndex: completedLevel,
                stars: stars,
                score: (float)_totalPairs / _attempts * 100f,
                correctAnswers: _matchesFound,
                totalQuestions: _totalPairs
            );

            if (!IsPlaying) return;
            StartLevel();
        }

        private void SetAllCardsInteractable(bool interactable)
        {
            if (_activeCards == null) return;
            for (int i = 0; i < _activeCards.Length; i++)
            {
                bool skipFirst = interactable
                              && i == _firstCardIndex
                              && _currentPhase == GamePhase.WaitingForSecondCard;
                _activeCards[i].SetInteractable(skipFirst ? false : interactable);
            }
        }
    }
}
