﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class GridGizmo : MonoBehaviour {

        [SerializeField] private Color _gridColor = Color.grey;
        [SerializeField] private bool _active = false;
        [SerializeField] private int _maxGridSize = 25;
        [SerializeField] private float _gridSize = 3;
        [SerializeField] private float _height = 0;
        [SerializeField] private Vector3 _offset = Vector3.zero;

#if UNITY_EDITOR
        private void DrawSceneGrid() {
            UnityEditor.Handles.color = _gridColor;
            var max = (_maxGridSize + 1) * _gridSize;
            var min = -(_maxGridSize * _gridSize);
            for (int x = -_maxGridSize; x <= _maxGridSize + 1; x++) {
                var xPos = x * _gridSize;
                UnityEditor.Handles.DrawLine(transform.TransformPoint(new Vector3(xPos, _height, min) + _offset),
                    transform.TransformPoint(new Vector3(xPos, _height, max) + _offset));
            }
            for (int z = -_maxGridSize; z <= _maxGridSize + 1; z++) {
                var zPos = z * _gridSize;
                UnityEditor.Handles.DrawLine(transform.TransformPoint(
                    new Vector3(min, _height, zPos) + _offset),
                    transform.TransformPoint(new Vector3(max, _height, zPos) + _offset));
            }
        }

        void OnDrawGizmos() {
            if (_active && !Application.isPlaying) {
                DrawSceneGrid();
            }
        }
#endif
    }
}
