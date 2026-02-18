using System.Collections.Generic;
using UnityEngine;

namespace Miyo.Core.ObjectPool
{
    public class ObjectPool<T> where T : Component, IPoolable
    {
        private readonly Queue<T> _available = new();
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly int _maxSize;

        public int CountActive { get; private set; }
        public int CountInactive => _available.Count;

        public ObjectPool(T prefab, Transform parent, int initialSize, int maxSize)
        {
            _prefab = prefab;
            _parent = parent;
            _maxSize = maxSize;

            for (int i = 0; i < initialSize; i++)
            {
                var obj = CreateNew();
                obj.gameObject.SetActive(false);
                _available.Enqueue(obj);
            }
        }

        public T Get()
        {
            T obj;
            if (_available.Count > 0)
            {
                obj = _available.Dequeue();
            }
            else
            {
                obj = CreateNew();
            }

            obj.gameObject.SetActive(true);
            obj.OnSpawn();
            CountActive++;
            return obj;
        }

        public void Return(T obj)
        {
            obj.OnDespawn();
            obj.gameObject.SetActive(false);
            CountActive--;

            if (_available.Count < _maxSize)
            {
                _available.Enqueue(obj);
            }
            else
            {
                Object.Destroy(obj.gameObject);
            }
        }

        public void ReturnAll()
        {
            // Note: This only returns objects that are tracked.
            // Objects must be returned individually via Return().
        }

        public void Clear()
        {
            while (_available.Count > 0)
            {
                var obj = _available.Dequeue();
                if (obj != null)
                    Object.Destroy(obj.gameObject);
            }

            CountActive = 0;
        }

        private T CreateNew()
        {
            var obj = Object.Instantiate(_prefab, _parent);
            return obj;
        }
    }
}
