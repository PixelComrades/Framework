﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class PathfindingMapDebugging : MonoBehaviour {
#if UNITY_EDITOR

        [SerializeField] private bool _showCost = false;
        [SerializeField] private bool _disableMap = false;

        private GameOptions.CachedInt _defaultCost = new GameOptions.CachedInt("PathfindGridDefaultCost");
        //private GameOptions.CachedFloat _occupiedCost = new GameOptions.CachedFloat("PathfindGridOccupiedCost");
        private GameOptions.CachedInt _playerCost = new GameOptions.CachedInt("PathfindGridPlayerCost");

        [Button]
        public void Clear() {
            World.Get<PathfindingSystem>().Clear();
        }

        void OnDrawGizmos() {
            var simpleGrid = World.Get<PathfindingSystem>().Grid;
            if (_disableMap || simpleGrid == null || simpleGrid.CellsCount == 0) {
                return;
            }
            simpleGrid.RunActionOnCells(
                (c, d) => {
                    Color color;
                    if (!d.IsWalkable) {
                        color = Color.grey;
                    }
                    else {
                        color = Color.Lerp(Color.green, Color.red, Mathf.InverseLerp(_defaultCost, _playerCost, d.TraversalCost));
                    }
                    Gizmos.color = color;
                    Gizmos.DrawWireCube(c.toVector3(), Vector3.one * 0.9f);
                    if (_showCost) {
                        UnityEditor.Handles.Label(c.toVector3(), d.TraversalCost.ToString("F1"));
                    }
                }
            );
        }
#endif
    }
}
