using System;
using UnityEngine;

namespace Miyo.Games
{
    /// <summary>
    /// Tek bir balonun davranışını yönetir: yukarı hareket, ekran dışına çıkış, pop.
    /// BalloonPopping tarafından Initialize() ile yapılandırılır.
    /// </summary>
    public class Balloon : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private ParticleSystem _popParticles; // opsiyonel patlama efekti

        private string _typeId;
        private float _speed;
        private Camera _camera;
        private Action<Balloon> _onPopped;
        private Action<Balloon> _onExit;
        private bool _isPopped;

        public string TypeId => _typeId;

        public void Initialize(string typeId, float speed, Camera gameCamera,
            Action<Balloon> onPopped, Action<Balloon> onExit)
        {
            _typeId = typeId;
            _speed = speed;
            _camera = gameCamera;
            _onPopped = onPopped;
            _onExit = onExit;
            _isPopped = false;
        }

        private void Update()
        {
            if (_isPopped) return;

            if (float.IsNaN(_speed) || float.IsNaN(transform.position.x))
            {
                Debug.LogError($"Balloon speed or position is NaN: {_speed}, {transform.position}");
                _isPopped = true;
                _onExit?.Invoke(this);
                Destroy(gameObject);
                return;
            }

            transform.position += Vector3.up * _speed * Time.deltaTime;

            if (IsAboveScreen())
            {
                _isPopped = true;
                _onExit?.Invoke(this);
                Destroy(gameObject);
            }
        }

        /// <summary>Dışarıdan (BalloonPopping) çağrılır; patlama efekti çalar ve nesneyi yok eder.</summary>
        public void Pop()
        {
            if (_isPopped) return;
            _isPopped = true;

            _onPopped?.Invoke(this);

            if (_popParticles != null)
            {
                if (_meshRenderer != null)
                {
                    var materialColor = _meshRenderer.material.color;
                    var main = _popParticles.main;
                    main.startColor = materialColor;
                }
                _popParticles.transform.SetParent(null); // prefab yokedilince particle'lar yarıda kesilmesin
                _popParticles.Play();
                Destroy(_popParticles.gameObject, _popParticles.main.duration + _popParticles.main.startLifetime.constantMax);
            }

            Destroy(gameObject);
        }

        private bool IsAboveScreen()
        {
            if (_camera == null) return false;
            Vector3 viewportPos = _camera.WorldToViewportPoint(transform.position);
            return viewportPos.y > 1.1f; // %10 tolerans
        }
    }
}
