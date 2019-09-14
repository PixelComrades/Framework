﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using System.Runtime.Serialization;

namespace PixelComrades {

    public abstract class ManagedArray {
        public abstract System.Type ArrayType { get; }
        public abstract object Get(int index);
        public abstract void Remove(int index);
        public abstract bool IsInvalid(int index);
    }

    public class FastStack<T> : ISerializable {
        private List<T> _list;
        private HashSet<T> _hash;

        public int Count {  get {  return _list.Count; } }

        public void Add(T value) {
            _list.Add(value);
            _hash.Add(value);
        }

        public void TryAdd(T value) {
            if (_hash.Contains(value)) {
                return;
            }
            _list.Add(value);
            _hash.Add(value);
        }

        public void Remove(T value) {
            _list.Remove(value);
            _hash.Remove(value);
        }

        public void TryRemove(T value) {
            if (!_hash.Contains(value)) {
                return;
            }
            _list.Remove(value);
            _hash.Remove(value);
        }

        public bool Contains(T value) {
            return _hash.Contains(value);
        }

        public T Pop() {
            var element = _list[_list.Count - 1];
            _list.RemoveAt(_list.Count - 1);
            _hash.Remove(element);
            return element;
        }

        public void Clear() {
            _list.Clear();
            _hash.Clear();
        }

        public FastStack() {
            _list = new List<T>();
            _hash = new HashSet<T>();
        }

        public FastStack(int cnt) {
            _list = new List<T>(cnt);
            _hash = new HashSet<T>();
        }

        public FastStack(SerializationInfo info, StreamingContext context) {
            _list = info.GetValue(nameof(_list), _list);
            _hash = info.GetValue(nameof(_hash), _hash);
        }
        
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_list), _list);
            info.AddValue(nameof(_hash), _hash);
        }
    }

    public class ManagedArray<T> : ManagedArray, IEnumerable<T>, ISerializable {

        private const int DefaultSize = 32;

        protected T[] Array;
        private int _max = 0;
        private FastStack<int> _freeIndices = new FastStack<int>();

        public int Max { get { return _max; } }
        public int Count { get { return _max; } }
        public int UsedCount { get { return _max - _freeIndices.Count; } }
        public bool IsFull { get { return _max == Array.Length && _freeIndices.Count == 0; } }
        public override System.Type ArrayType { get { return typeof(T); } }
        public ref T this[int index] { get { return ref Array[index]; } }

        public ManagedArray(int initialSize = DefaultSize) {
            Array = new T[initialSize];
        }

        public ManagedArray(SerializationInfo info, StreamingContext context) {
            Array = info.GetValue(nameof(Array), Array);
            _max = info.GetValue(nameof(_max), _max);
            _freeIndices = info.GetValue(nameof(_freeIndices), _freeIndices);
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Array), Array);
            info.AddValue(nameof(_max), _max);
            info.AddValue(nameof(_freeIndices), _freeIndices);
        }

        public override object Get(int index) {
            return Array[index];
        }

        public void Set(int index, T item) {
            Array[index] = item;
            _freeIndices.TryRemove(index);
        }

        public bool IndexFree(int index) {
            if (index < _max) {
                return true;
            }
            return _freeIndices.Contains(index);
        }

        public bool HasIndex(int index) {
            return Array.HasIndex(index);
        }

        public int Add(T newComponent) {
            if (_max < Array.Length) {
                Array[_max] = newComponent;
                _max++;
                return _max - 1;
            }
            if (_freeIndices.Count > 0) {
                var index = _freeIndices.Pop();
                Array[index] = newComponent;
                return index;
            }
            _max = Array.Length;
            System.Array.Resize(ref Array, _max * 2);
            Array[_max] = newComponent;
            _max++;
            return _max - 1;
        }

        public void Replace(ManagedArray<T> source) {
            Clear();
            if (Array.Length < source.Count) {
                System.Array.Resize(ref Array, source.Count * 2);
            }
            for (int i = 0; i < source.Count; i++) {
                Array[i] = source[i];
            }
            _max = source.Max;
        }

        public void Remove(T component) {
            var index = Array.FindIndex(component);
            if (index >= 0) {
                Remove(index);
            }
        }

        public T Pop() {
            for (int i = 0; i < _max; i++) {
                if (Array[i] != null) {
                    var value = Array[i];
                    Remove(i);
                    return value;
                }
            }
            return default(T);
        }

        public override void Remove(int index) {
            Array[index] = default(T);
            _freeIndices.TryAdd(index);
        }

        public void Clear() {
            for (int i = 0; i < Array.Length; i++) {
                Array[i] = default(T);
            }
            _max = 0;
            _freeIndices.Clear();
        }

        public void Reset() {
            Array = new T[Array.Length];
            _freeIndices.Clear();
            _max = 0;
        }

        public override bool IsInvalid(int index) {
            return _freeIndices.Contains(index);
        }

        public bool Contains(T value) {
            if (value == null) {
                return false;
            }
            for (int a = 0; a < Max; a++) {
                if (Array[a] == null) {
                    continue;
                }
                if (Array[a].Equals(value)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Must sort nulls to top
        /// </summary>
        /// <param name="comparer"></param>
        public void Sort(IComparer<T> comparer) {
            _freeIndices.Clear();
            System.Array.Sort(Array, comparer);
            for (int i = 0; i < Array.Length; i++) {
                if (Array[i] == null) {
                    _max = i;
                    break;
                }
            }
        }

        public void Run(Delegate del) {
            if (_max == 0) {
                return;
            }
            for (int i = 0; i < _max; i++) {
                if (_freeIndices.Contains(i)) {
                    continue;
                }
                del(Array[i]);
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            if (_enumerators.Count == 0) {
                return new Enumerator(this);
            }
            return _enumerators.Dequeue();
        }

        public IEnumerator GetEnumerator() {
            if (_enumerators.Count == 0) {
                return new Enumerator(this);
            }
            return _enumerators.Dequeue();
        }

        private Queue<Enumerator> _enumerators = new Queue<Enumerator>();

        private class Enumerator : IEnumerator<T> {

            private ManagedArray<T> _array;

            public Enumerator(ManagedArray<T> array) {
                _array = array;
                _position = -1;
            }

            public void Reset() {
                _position = -1;
            }

            private int _position;
            public object Current {
                get {
                    if (_position >= _array._max) {
                        return default(T);
                    }
                    return _array[_position];
                }
            }
            T IEnumerator<T>.Current {
                get {
                    if (_position >= _array._max) {
                        return default(T);
                    }
                    return _array[_position];
                }
            }

            public void Dispose() {
                _position = -1;
                _array._enumerators.Enqueue(this);
            }

            public bool MoveNext() {
                while (_position < _array._max) {
                    _position++;
                    if (_position >= _array._max) {
                        return false;
                    }
                    if (!_array._freeIndices.Contains(_position)) {
                        return true;
                    }
                }
                return false;
            }
        }

        public delegate void Delegate(T value);
    }

    public class ComponentArray<T> : ManagedArray<T> where T : IComponent {

        private Dictionary<int, int> _entityToIndex = new Dictionary<int, int>();
        private Dictionary<T, int> _componentToEntity = new Dictionary<T, int>();

        public ComponentArray(int initialSize) : base(initialSize) {}
        public ComponentArray() {}

        public ComponentArray(SerializationInfo info, StreamingContext context) : base(info, context) {
            _entityToIndex = info.GetValue(nameof(_entityToIndex), _entityToIndex);
            _componentToEntity = info.GetValue(nameof(_componentToEntity), _componentToEntity);
        }
        
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);
            info.AddValue(nameof(_entityToIndex), _entityToIndex);
            info.AddValue(nameof(_componentToEntity), _componentToEntity);
        }

        public void Add(Entity entity, T newComponent) {
            if (_entityToIndex.TryGetValue(entity, out var index)) {
                Set(index, newComponent);
            }
            else {
                index = Add(newComponent);
                _entityToIndex.Add(entity, index);
                _componentToEntity.AddOrUpdate(newComponent, entity);
                entity.AddReference(new ComponentReference(index, this));
            }
//            if (newComponent is IComponentOnAttach attach) {
//                attach.OnAttach(entity);
//            }
        }

        public Entity GetEntity(T component) {
            if (component == null) {
                return null;
            }
            if (_componentToEntity.TryGetValue(component, out var ent)) {
                return EntityController.GetEntity(ent);
            }
            return null;
        }

        public bool HasComponent(Entity index) {
            return _entityToIndex.ContainsKey(index);
        }

        public bool TryGet(Entity entity, out T value) {
            if (_entityToIndex.TryGetValue(entity, out var index)) {
                value = this[index];
                return true;
            }
            value = default(T);
            return false;
        }

//        public override void Remove(int index) {
//            base.Remove(index);
//        }

        public void Remove(Entity entity) {
            if (_entityToIndex.TryGetValue(entity, out var existing)) {
                _componentToEntity.Remove(this[existing]);
                Remove(existing);
                _entityToIndex.Remove(entity);
                entity.Remove(ArrayType);
            }
        }

        public T Get(Entity entity) {
            if (_entityToIndex.TryGetValue(entity, out var index)) {
                return this[index];
            }
            return default(T);
        }
    }

    public struct ComponentReference : IComparable<ComponentReference>, IEquatable<ComponentReference> {
        public int Index { get; }
        public ManagedArray Array { get; }

        public ComponentReference(int index, ManagedArray array) {
            Index = index;
            Array = array;
        }

        public System.Object Get() {
            return Array.Get(Index);
        }

        public ref T Get<T>() {
            return ref ((ManagedArray<T>) Array)[Index];
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            return obj is ComponentReference other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (Index * 397) ^ (Array != null ? Array.GetHashCode() : 0);
            }
        }

        public int CompareTo(ComponentReference other) {
            return other.Index.CompareTo(Index);
        }

        public bool Equals(ComponentReference other) {
            return other.Index == Index && other.Array == Array;
        }
    }

    public static class ComponentReferenceExtensions {

        public static Entity GetEntity<T>(this T component) where T : IComponent {
            return EntityController.GetComponentArray<T>().GetEntity(component);
        }
        
        //public static bool ContainsType(this IList<ComponentReference> compList, System.Type type) {
        //    for (int i = 0; i < compList.Count; i++) {
        //        if (compList[i].Type == type) {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        //public static bool ContainsDerivedType(this IList<ComponentReference> compList, System.Type type) {
        //    for (int i = 0; i < compList.Count; i++) {
        //        if (compList[i].Type.IsAssignableFrom(type)) {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        //public static bool ContainsDerivedType(this Dictionary<Type, ComponentReference> compList, System.Type type) {
        //    foreach (var cref in compList) {
        //        if (type.IsAssignableFrom(cref.Key)) {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
    }
}
