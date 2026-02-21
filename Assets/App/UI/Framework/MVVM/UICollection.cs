using System;
using System.Collections.Generic;
using UnityEngine;

namespace Miyo.UI
{
    [Serializable]
    public class UICollection<T> : ISerializationCallbackReceiver where T : Component
    {
        public T itemRes;
        public T[] predefinedItems;
        public RectTransform customParent;
        [SerializeField, HideInInspector] private int _count;
        [SerializeField, HideInInspector] private List<ElementReference> _items;
        [SerializeField, HideInInspector] private bool _initialized;

        private Transform _currentParent;
        private RectTransform _cachedRect;
        public RectTransform CurrentParent {
            get {
                EnsureCache();
                return _cachedRect;
            }
        }

        private bool IsStateWrong()
        {
            if (Application.isPlaying) return false;
            return _count > 0 || (_items != null && _items.Count > 0) || _initialized;
        }
        
        // [InfoBox("The state seems to be wrong!", InfoMessageType.Error)]
        // [ShowIf(nameof(IsStateWrong))]
        // [Button]
        // private void FixState()
        // {
        //     _count = 0;
        //     _items = null;
        //     _initialized = false;
        // }

        private Transform EnsureCache() {
            if (_currentParent == null) {
                _currentParent = customParent ? customParent : (itemRes ? itemRes.transform.parent : ((predefinedItems != null && predefinedItems.Length > 0) ?predefinedItems[0].transform.parent : null ));
                _cachedRect = _currentParent as RectTransform;
            }
            return _currentParent;
        }

        public void OnBeforeSerialize() {
            //if (_items != null) {
            //    for (int i = 0; i < _items.Count; i++) {
            //        var it = _items[i];
            //        if (!ContainsAsRes(it)) {
            //            it.gameObject.SetActive(false);
            //        }
            //    }
            //}
        }
        public void OnAfterDeserialize() {

        }
        public bool ContainsAsRes(ElementReference er) {
            if(er.item == itemRes) {
                return true;
            }
            if (predefinedItems != null) {
                for (int i = 0; i < predefinedItems.Length; i++) {
                    if (predefinedItems[i] == er.item) {
                        return true;
                    }
                }
            }
            return false;
        }


        public int Count {
            get => _count;
            set {
                if (!_initialized) {
                    _initialized = true;
                    if (_items == null) {
                        _items = new List<ElementReference>();
                    }
                    else {
                        _items.Clear();
                    }
                    if (itemRes) {
                        if (itemRes.gameObject.activeSelf) {
                            itemRes.gameObject.SetActive(false);
                        }
                    }
                    if (predefinedItems != null) {
                        for (int i = 0; i < predefinedItems.Length; i++) {
                            var pi = predefinedItems[i];
                            pi.gameObject.SetActive(false);
                            _items.Add(new ElementReference(pi, i));
                        }
                    }
                }
                InitializeListItems(_items, itemRes, ref _count, value, EnsureCache());
            }
        }
        public bool IsValid {
            get {
                return itemRes != null || (_items != null && _items.Count > 0);
            }
        }

        public T this[int index] {
            get => _items[index].item;
        }
        public int GetResourceId(int index) {
            return _items[index].resId;
        }
        public T SetResourceId(int index, int resId) {
            var el = _items[index];
            if(el.resId!=resId) {
                int last = _count;
                var id = this.AddElement(resId);
                _items[index] = new ElementReference(id, resId);
                _items[last] = el;
                id.transform.SetSiblingIndex(index);

                el.gameObject.SetActive(false);
                
                _count--;
                return id;
            }
            return el.item;
        }
        public void ReverseOrder() {
            for (int i = 1; i < _items.Count; i++) {
                _items[i].transform.SetAsFirstSibling();
            }
        }
        public T RemoveElement(int index) {
            if (index >= _count) {
                throw new IndexOutOfRangeException();
            }
            var item = _items[index];
            _items.RemoveAt(index);
            item.item.gameObject.SetActive(false);
            _items.Add(item);
            
            _count--;
            item.item.transform.SetSiblingIndex(_items.Count - 1);


            return item.item;
        }
        public int IndexOf(T item) {
            for(int i = 0; i < Count; i++) {
                if(item == this[i]) {
                    return i;
                }
            }
            return -1;
        }

        public K AddElement<K>(int itemId = -1) where K : T {
            return (K)AddElement(itemId);
        }
        public int NormalizeItemId(int itemId) {
            if(itemId >= 0) {
                if(predefinedItems == null || itemId >= predefinedItems.Length) {
                    return -1;
                }
            }
            return itemId;
        }
        public T AddElement(int itemId = -1) {
            return InsertElement(_count, itemId);
        }
        public T InsertElement(int insertIndex, int itemId = -1) {
            var parent = EnsureCache();
            int index = insertIndex;
            int listIndex = -1;
            for(int i = _count; i < _items.Count; i++) {
                if(_items[i].resId == itemId) {
                    listIndex = i;
                    break;
                }
            }
            _count++;

            if (listIndex < 0) {
                T itemRes;
                if(itemId >= 0) {
                    itemRes = predefinedItems[itemId];
                }
                else {
                    itemRes = this.itemRes;
                }
                var er = new ElementReference(UnityEngine.Object.Instantiate(itemRes, parent, false), itemId);
                _items.Insert(insertIndex, er);
            } else if (listIndex != index) {
                var er = _items[listIndex];
                _items.RemoveAt(listIndex);
                _items.Insert(index, er);
            }
            var it = _items[index].item;
            SetVisibleSiblingIndex(it.transform, index);
            if(!it.gameObject.activeSelf) {
                it.gameObject.SetActive(true);
            }
            return it;
        }

		public void SetVisibleSiblingIndex(Transform target, int index) {
			int i = 0;
			var parent = target.parent;
			int siblingCount = target.parent.childCount;
			while(index > 0) {
				if(i >= siblingCount) {
					break;
				}
				if(parent.GetChild(i).gameObject.activeSelf) {
					index--;
				}
				i++;
			}
			target.SetSiblingIndex(i);

		}
        public static void InitializeListItems(List<ElementReference> items, T res, ref int lastCount, int targetCount, Transform parent) {

            if (res) {
                
                while (items.Count < targetCount) {
                    var item = UnityEngine.Object.Instantiate(res, parent, false);
                    items.Add(new ElementReference(item, -1));
                }
            }
            while (lastCount < targetCount) {
                var item = items[lastCount];
                item.gameObject.SetActive(true);
                item.transform.SetSiblingIndex(lastCount);
                lastCount++;
            }
            while (lastCount > targetCount) {
                lastCount--;
                items[lastCount].gameObject.SetActive(false);
            }
        }
        
        public void Sort(IComparer<UICollection<T>.ElementReference> comparer)
        {
            _items.Sort(comparer);
            for (int i = Count - 1; i >= 0; i--)
            {
                var tr = _items[i].transform;
                if (tr)
                {
                    tr.SetAsFirstSibling();
                }
            }
        }
        
        [Serializable]
        public struct ElementReference {
            public int resId;
            public T item;
            public Transform transform => item.transform;
            public GameObject gameObject => item.gameObject;
            public ElementReference(T item, int id) {
                this.item = item;
                this.resId = id;
            }
        }


    }
}