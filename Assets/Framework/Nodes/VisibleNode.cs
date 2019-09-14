﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class VisibleNode : BaseNode {

        public CachedComponent<ModelComponent> Model = new CachedComponent<ModelComponent>();
        public CachedComponent<LabelComponent> Label = new CachedComponent<LabelComponent>();
        public CachedComponent<RigidbodyComponent> Rb = new CachedComponent<RigidbodyComponent>();
        public CachedComponent<ColliderComponent> Collider = new CachedComponent<ColliderComponent>();

        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            Label, Model, Rb, Collider,
        };

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(RigidbodyComponent),
                typeof(ModelComponent),
                typeof(LabelComponent),
            };
        }

        public void Setup(GameObject obj) {
            Model.Value.Set(obj.GetComponent<ModelWrapper>());
            Entity.Tr = obj.transform;
        }

        public Vector3 position {
            get {
                if (Entity.Tr != null) {
                    return Entity.Tr.position + Collider.Value?.LocalCenter ?? new Vector3(0,1,0);
                }
                return Vector3.zero;
            }
        }
        public Quaternion rotation { get { return Entity.Tr?.rotation ?? Quaternion.identity; } }

    }
}
