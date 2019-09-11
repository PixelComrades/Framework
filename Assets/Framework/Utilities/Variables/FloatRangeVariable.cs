﻿using UnityEngine;

namespace PixelComrades {
    [CreateAssetMenu(menuName = "Assets/FloatRange")]
    public class FloatRangeVariable : FloatVariable {

        [SerializeField] private float _min = 0;
        [SerializeField] private float _max = 0;

        public override float Value { get { return Game.Random.NextFloat(_min, _max); } }
        public float Min { get { return _min; } }
        public float Max { get { return _max; } }

        public float Lerp(float t) {
            return Mathf.Lerp(_min, _max, t);
        }
    }
}