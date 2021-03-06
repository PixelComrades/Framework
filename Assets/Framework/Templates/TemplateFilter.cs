﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class TemplateFilter {

        private System.Type[] _requiredTypes;

        public Type[] RequiredTypes { get => _requiredTypes; }

        protected TemplateFilter(System.Type[] types) {
            _requiredTypes = types;
        }

        public bool TryAdd(Entity entity) {
            if (ContainsEntity(entity)) {
                return true;
            }
            for (int i = 0; i < _requiredTypes.Length; i++) {
                if (!entity.HasReference(_requiredTypes[i])) {
                    return false;
                }
            }
            AddEntity(entity);
            return true;
        }

        public void CheckRemove(Entity entity) {
            if (!ContainsEntity(entity)) {
                return;
            }
            for (int i = 0; i < _requiredTypes.Length; i++) {
                if (entity.HasReference(_requiredTypes[i])) {
                    continue;
                }
#if DEBUG
                DebugLog.Add("Removed " + entity.DebugId + " for missing " + _requiredTypes[i].Name);
#endif
                RemoveEntity(entity);
                return;
            }
        }

        protected abstract void AddEntity(Entity entity);
        public abstract void RemoveEntity(Entity entity);
        public abstract bool ContainsEntity(Entity entity);
    }

    public class TemplateFilter<T> : TemplateFilter where T : class, IEntityTemplate, new() {

        private Dictionary<int, T> _templates = new Dictionary<int, T>();
        private TemplateList<T> _allTemplates = new TemplateList<T>();
        private GenericPool<T> _pool = new GenericPool<T>(25, obj => obj.Dispose());

        public TemplateList<T> AllTemplates { get => _allTemplates; }

        protected TemplateFilter(System.Type[] types) : base(types) {}

        public static TemplateFilter<T> Setup() {
            var instance = new T();
            var filter = new TemplateFilter<T>(instance.GetTypes());
            EntityController.RegisterTemplateFilter(filter, typeof(T));
            return filter;
        }
        
        public override bool ContainsEntity(Entity entity) {
            return _templates.ContainsKey(entity);
        }

        protected override void AddEntity(Entity entity) {
            if (_templates.ContainsKey(entity)) {
                return;
            }
#if DEBUG
                DebugLog.Add("Add " + entity.DebugId + " to " + typeof(T).Name);
#endif
            T node = _pool.New();
            node.Register(entity);
            _allTemplates.Add(node);
            _templates.Add(entity, node);
        }

        public override void RemoveEntity(Entity entity) {
            if (!_templates.TryGetValue(entity, out var node)) {
                return;
            }
            node.Dispose();
            _allTemplates.Remove(node);
            _templates.Remove(entity);
            _pool.Store(node);
        }

        public T GetNode(Entity entity) {
            return _templates.TryGetValue(entity, out var node) ? node : null;
        }
    }
}