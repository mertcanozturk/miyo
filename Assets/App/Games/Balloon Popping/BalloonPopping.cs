using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Miyo.Core.Events;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Miyo.Games
{
    public class BalloonPopping : GameBase<BalloonPopping.BalloonPoppingSaveData>
    {
        [Header("Config")]
        [SerializeField] private BalloonConfig _config;

        [Header("Sahne Referansları")]
        [SerializeField] private Button _backButton;
        private Camera _gameCamera;
        [SerializeField] private Transform _spawnParent;
        [SerializeField] private Transform[] _spawnPoints;

        [Header("HUD")]
        [SerializeField] private TMP_Text _tokenText;
        [SerializeField] private Transform _targetDisplayContainer; // hedef balonun prefab'ı burada instantiate edilir
        [SerializeField] private TMP_Text _targetLabel;
        [SerializeField] private CanvasGroup _wrongFeedbackOverlay; // yanlış seçimde kısa süre görünür

        protected override string GameId => "balloon_popping";

        // ── Durum ────────────────────────────────────────────────────

        private int _tokens;
        private int _successfulPops;
        private int _currentStreak;
        private int _popsOnCurrentTarget;
        private float _currentSpawnInterval;
        private float _tempoBumpTimer;
        private bool _isTempoBumpActive;
        private BalloonTypeDefinition _currentTarget;
        private GameObject _targetDisplayInstance;
        private readonly List<Balloon> _activeBalloons = new();

        // ── Back Button ──────────────────────────────────────────────

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            if (_backButton != null)
                _backButton.onClick.AddListener(ExitGame);
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
            if (_backButton != null)
                _backButton.onClick.RemoveListener(ExitGame);
        }

        // ── GameBase Template Metodları ──────────────────────────────

        protected override void OnInitialize(string childName, BalloonPoppingSaveData saveData)
        {
            _tokens = 0;
            _successfulPops = 0;
            _currentStreak = 0;
            _popsOnCurrentTarget = 0;
            _tempoBumpTimer = 0;
            _isTempoBumpActive = false;
            _currentSpawnInterval = _config.baseSpawnInterval;
            _gameCamera = Camera.main;
        }

        protected override void OnGameStart()
        {
            PickNewTarget(firstTime: true);
            UpdateHUD();
            SpawnLoopAsync(destroyCancellationToken).Forget();
            TempoBumpWatcherAsync(destroyCancellationToken).Forget();
        }

        protected override void OnCleanup()
        {
            foreach (var balloon in _activeBalloons)
            {
                if (balloon != null)
                    Destroy(balloon.gameObject);
            }
            _activeBalloons.Clear();

            if (_targetDisplayInstance != null)
                Destroy(_targetDisplayInstance);
        }

        // ── Update: Tıklama / Dokunma ────────────────────────────────

        protected override void Update()
        {
            base.Update(); // ElapsedTime günceller

            if (!IsPlaying) return;

            Vector2? screenPos = null;

            // Dokunmatik ekran
            foreach (var touch in Touch.activeTouches)
            {
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    screenPos = touch.screenPosition;
                    break;
                }
            }

            // Mouse (editör / masaüstü)
            if (screenPos == null && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                screenPos = Mouse.current.position.ReadValue();

            if (screenPos.HasValue)
            {
                Ray ray = _gameCamera.ScreenPointToRay(screenPos.Value);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    var balloon = hit.collider.GetComponentInParent<Balloon>();
                    balloon?.Pop();
                }
            }
        }

        // ── Spawn Döngüsü ────────────────────────────────────────────

        private async UniTaskVoid SpawnLoopAsync(System.Threading.CancellationToken ct)
        {
            try
            {
                while (IsPlaying)
                {
                    float interval = _isTempoBumpActive
                        ? _currentSpawnInterval * _config.tempoBumpSpeedMult
                        : _currentSpawnInterval;

                    await UniTask.Delay((int)(interval * 1000), cancellationToken: ct);
                    if (!IsPlaying) break;

                    SpawnBalloon();
                }
            }
            catch (OperationCanceledException) { }
        }

        private void SpawnBalloon()
        {
            if (_config.balloonTypes == null || _config.balloonTypes.Length == 0) return;
            if (_spawnPoints == null || _spawnPoints.Length == 0) return;
            if (_currentTarget == null) return;

            int count = Mathf.Clamp(
                UnityEngine.Random.Range((int)_config.spawnCountRange.x, (int)_config.spawnCountRange.y + 1),
                1,
                _spawnPoints.Length);

            var spawnIndices = GetRandomUniqueIndices(_spawnPoints.Length, count);

            for (int i = 0; i < spawnIndices.Count; i++)
            {
                BalloonTypeDefinition typeDef = ChooseBalloonType();
                Transform spawnPoint = _spawnPoints[spawnIndices[i]];
                if (spawnPoint == null) continue;

                Vector3 pos = spawnPoint.position;
                if (float.IsNaN(pos.x) || float.IsNaN(pos.y) || float.IsNaN(pos.z))
                    continue;

                var go = Instantiate(typeDef.prefab, pos, Quaternion.identity, _spawnParent);
                var balloon = go.GetComponent<Balloon>();
                if (balloon == null) balloon = go.AddComponent<Balloon>();

                balloon.Initialize(typeDef.typeId, CurrentBalloonSpeed(), _gameCamera,
                    onPopped: OnBalloonPopped,
                    onExit: OnBalloonExited);

                _activeBalloons.Add(balloon);
            }
        }

        private List<int> GetRandomUniqueIndices(int maxCount, int takeCount)
        {
            var indices = new List<int>(maxCount);
            for (int i = 0; i < maxCount; i++)
                indices.Add(i);

            for (int i = 0; i < takeCount; i++)
            {
                int swapIndex = UnityEngine.Random.Range(i, maxCount);
                (indices[i], indices[swapIndex]) = (indices[swapIndex], indices[i]);
            }

            indices.RemoveRange(takeCount, maxCount - takeCount);
            return indices;
        }

        private BalloonTypeDefinition ChooseBalloonType()
        {
            // hedefDışıOran: minNonTargetRatio'dan başlar, zorluk arttıkça maxNonTargetRatio'ya yaklaşır
            float difficultyProgress = Mathf.Clamp01(_successfulPops / 100f);
            float nonTargetRatio = Mathf.Lerp(_config.minNonTargetRatio, _config.maxNonTargetRatio, difficultyProgress);

            if (UnityEngine.Random.value > nonTargetRatio)
            {
                return _currentTarget;
            }

            var others = new List<BalloonTypeDefinition>(_config.balloonTypes.Length);
            foreach (var t in _config.balloonTypes)
            {
                if (t.typeId != _currentTarget.typeId)
                    others.Add(t);
            }
            return others.Count > 0
                ? others[UnityEngine.Random.Range(0, others.Count)]
                : _currentTarget;
        }

        private float CurrentBalloonSpeed()
        {
            float denom = _config.baseSpawnInterval - _config.minSpawnInterval;
            if (denom <= 0.001f) return _config.minBalloonSpeed;

            float normalized = Mathf.Clamp01(
                (_currentSpawnInterval - _config.minSpawnInterval) / denom);
            return Mathf.Lerp(_config.maxBalloonSpeed, _config.minBalloonSpeed, normalized);
        }

        // ── Yanlış Balon Geri Bildirimi ──────────────────────────────

        private async UniTaskVoid ShowWrongFeedbackAsync(System.Threading.CancellationToken ct)
        {
            if (_wrongFeedbackOverlay == null) return;
            try
            {
                _wrongFeedbackOverlay.alpha = 0.35f;
                await UniTask.Delay(200, cancellationToken: ct);
                _wrongFeedbackOverlay.alpha = 0f;
            }
            catch (OperationCanceledException)
            {
                if (_wrongFeedbackOverlay != null)
                    _wrongFeedbackOverlay.alpha = 0f;
            }
        }

        // ── Pop Geri Bildirimleri ────────────────────────────────────

        private void OnBalloonPopped(Balloon balloon)
        {
            _activeBalloons.Remove(balloon);

            if (balloon.TypeId == _currentTarget.typeId)
            {
                _tokens++;
                _successfulPops++;
                _currentStreak++;

                _currentSpawnInterval = Mathf.Max(
                    _config.minSpawnInterval,
                    _config.baseSpawnInterval * Mathf.Pow(_config.difficultyFactor, _successfulPops));

                EventBus.Publish(new CorrectAnswerEvent
                {
                    GameId = GameId,
                    QuestionIndex = _successfulPops
                });

                UpdateHUD();

                _popsOnCurrentTarget++;
                if (_popsOnCurrentTarget >= _config.popsPerTarget)
                    PickNewTarget(firstTime: false);
            }
            else
            {
                _currentStreak = 0;

                EventBus.Publish(new WrongAnswerEvent
                {
                    GameId = GameId,
                    QuestionIndex = _successfulPops
                });

                ShowWrongFeedbackAsync(destroyCancellationToken).Forget();

#if UNITY_ANDROID || UNITY_IOS
                Handheld.Vibrate();
#endif
            }
        }

        private void OnBalloonExited(Balloon balloon)
        {
            _activeBalloons.Remove(balloon);
        }

        // ── Hedef Seçimi ─────────────────────────────────────────────

        private void PickNewTarget(bool firstTime)
        {
            if (_config.balloonTypes == null || _config.balloonTypes.Length == 0) return;

            if (firstTime || _config.balloonTypes.Length == 1)
            {
                _currentTarget = _config.balloonTypes[UnityEngine.Random.Range(0, _config.balloonTypes.Length)];
            }
            else
            {
                BalloonTypeDefinition next;
                do
                {
                    next = _config.balloonTypes[UnityEngine.Random.Range(0, _config.balloonTypes.Length)];
                }
                while (next.typeId == _currentTarget.typeId);
                _currentTarget = next;
            }

            _popsOnCurrentTarget = 0;

            // Önceki hedef göstergesi instance'ını temizle ve yeni prefab'ı yerleştir
            if (_targetDisplayInstance != null)
                Destroy(_targetDisplayInstance);

            if (_targetDisplayContainer != null && _currentTarget.prefab != null)
            {
                _targetDisplayInstance = Instantiate(
                    _currentTarget.prefab,
                    _targetDisplayContainer.position,
                    _targetDisplayContainer.rotation,
                    _targetDisplayContainer);

                // Hedef göstergesindeki balon hareket etmemeli
                var balloon = _targetDisplayInstance.GetComponent<Balloon>();
                if (balloon != null) Destroy(balloon);
            }

            if (_targetLabel != null) _targetLabel.text = _currentTarget.displayName;
        }

        // ── Tempo Bump ───────────────────────────────────────────────

        private async UniTaskVoid TempoBumpWatcherAsync(System.Threading.CancellationToken ct)
        {
            try
            {
                while (IsPlaying)
                {
                    await UniTask.Delay(1000, cancellationToken: ct);
                    if (!IsPlaying) break;

                    _tempoBumpTimer += 1f;

                    bool timeTriggered = _tempoBumpTimer >= _config.tempoBumpEverySeconds;
                    bool streakTriggered = _currentStreak >= _config.tempoBumpStreakCount;

                    if ((timeTriggered || streakTriggered) && !_isTempoBumpActive)
                    {
                        _tempoBumpTimer = 0f;
                        _currentStreak = 0;
                        _isTempoBumpActive = true;

                        await UniTask.Delay((int)(_config.tempoBumpDuration * 1000), cancellationToken: ct);
                        _isTempoBumpActive = false;
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        // ── HUD ─────────────────────────────────────────────────────

        private void UpdateHUD()
        {
            if (_tokenText != null)
                _tokenText.text = _tokens.ToString();
        }

        // ── Save Data ────────────────────────────────────────────────

        [Serializable]
        public class BalloonPoppingSaveData { } // endless oyun — persist edilecek data yok
    }
}
