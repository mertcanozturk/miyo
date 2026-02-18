using System;
using System.Collections;
using System.Collections.Generic;

namespace Miyo.UI.MVVM
{
    public class ReactiveCollection<T> : IList<T>, IDisposable
    {
        private readonly List<T> _list = new();

        public event Action<int, T> OnItemAdded;
        public event Action<int, T> OnItemRemoved;
        public event Action<int, T, T> OnItemReplaced;
        public event Action OnCleared;
        public event Action OnChanged;

        public int Count => _list.Count;
        public bool IsReadOnly => false;

        public T this[int index]
        {
            get => _list[index];
            set
            {
                var old = _list[index];
                _list[index] = value;
                OnItemReplaced?.Invoke(index, old, value);
                OnChanged?.Invoke();
            }
        }

        public void Add(T item)
        {
            _list.Add(item);
            OnItemAdded?.Invoke(_list.Count - 1, item);
            OnChanged?.Invoke();
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
            OnItemAdded?.Invoke(index, item);
            OnChanged?.Invoke();
        }

        public bool Remove(T item)
        {
            int index = _list.IndexOf(item);
            if (index < 0) return false;
            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            var item = _list[index];
            _list.RemoveAt(index);
            OnItemRemoved?.Invoke(index, item);
            OnChanged?.Invoke();
        }

        public void Clear()
        {
            _list.Clear();
            OnCleared?.Invoke();
            OnChanged?.Invoke();
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                _list.Add(item);
                OnItemAdded?.Invoke(_list.Count - 1, item);
            }
            OnChanged?.Invoke();
        }

        public bool Contains(T item) => _list.Contains(item);
        public int IndexOf(T item) => _list.IndexOf(item);
        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose()
        {
            OnItemAdded = null;
            OnItemRemoved = null;
            OnItemReplaced = null;
            OnCleared = null;
            OnChanged = null;
            _list.Clear();
        }
    }
}
