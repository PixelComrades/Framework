using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelComrades {
    [Serializable]
    public class TagsComponent : IComponent {

        private Entity _entity;

        public int Owner { get { return _entity.Id; } set { } }

        private int[] _tags = new int[EntityTags.MAXLIMIT];

        public void Dispose() {
            _tags = null;
            _entity = null;
        }

        public void Add(params int[] ids) {
            for (var i = 0; i < ids.Length; i++) {
                var index = ids[i];
                _tags[index]++;
            }
            _entity.Post(EntitySignals.TagsChanged);
        }
        /// <summary>
        /// id must be from EntityTags
        /// </summary>
        /// <param name="id"></param>
        public void Add(int id) {
            _tags[id]++;
            _entity.Post(EntitySignals.TagsChanged);
        }

        public void AddWithRoot(int id) {
            Add(id);
            _entity.TryRoot(e => e.Tags.Add(id));
        }

        public void AddWithParent(int id) {
            Add(id);
            _entity.TryParent(e => e.Tags.Add(id));
        }

        public bool Contain(int val) {
            return _tags[val] > 0;
        }

        public bool ContainWithParent(int val) {
            var parent = _entity.GetParent();
            if (parent == null) {
                return _tags[val] > 0;
            }
            return _tags[val] > 0 || parent.Tags.Contain(val);
        }

        public bool ContainAll(params int[] filter) {
            for (var i = 0; i < filter.Length; i++) {
                if (_tags[filter[i]] <= 0) {
                    return false;
                }
            }
            return true;
        }

        public bool ContainAny(params int[] filter) {
            for (var i = 0; i < filter.Length; i++) {
                if (_tags[filter[i]] > 0) {
                    return true;
                }
            }
            return false;
        }

        public void Remove(params int[] ids) {
            for (var i = 0; i < ids.Length; i++) {
                var index = ids[i];
                _tags[index] = Math.Max(_tags[index] - 1, 0);
            }
            _entity.Post(EntitySignals.TagsChanged);
        }

        public void Remove(int id) {
            _tags[id] = Math.Max(_tags[id] - 1, 0);
            _entity.Post(EntitySignals.TagsChanged);
        }

        public void RemoveWithRoot(int id) {
            Remove(id);
            _entity.TryRoot(e => e.Tags.Remove(id));
        }

        public void RemoveWithParent(int id) {
            Add(id);
            _entity.TryParent(e => e.Tags.Remove(id));
        }

        public TagsComponent(Entity owner) {
            _entity = owner;
        }
    }
}