﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class DespawnOnActionEventComplete : MonoBehaviour, IActionPrefab, IOnCreate {

        [SerializeField] private float _maxTime = 45;
        [SerializeField] private float _minTime = 1f;
        [SerializeField] private bool _parentAnimTr = false;

        private PrefabEntity _entity;

        private IEnumerator WaitForEventComplete(ActionEvent actionEvent) {
            var maxTime = TimeManager.Time + _maxTime;
            var minTime = TimeManager.Time + _minTime;
            while (true) {
                if (TimeManager.Time > maxTime || actionEvent.Origin != null && actionEvent.Origin.Entity.IsDead()) {
                    break;
                }
                if (TimeManager.Time > minTime && actionEvent.State != ActionState.Activate) {
                    break;
                }
                yield return null;
            }
            ItemPool.Despawn(_entity);
        }

        public void OnCreate(PrefabEntity entity) {
            _entity = entity;
        }

        public void OnActionSpawn(ActionEvent actionEvent) {
            if (_parentAnimTr && actionEvent.Origin != null) {
                var parentTr = actionEvent.Origin.Tr;
                if (parentTr != null) {
                    parentTr.SetChild(transform);
                }
            }
            TimeManager.StartUnscaled(WaitForEventComplete(actionEvent));

        }
    }
}
