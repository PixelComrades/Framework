﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using PixelComrades.DungeonCrawler;

namespace PixelComrades {
    public struct ForwardMover : IComponent {
        public ForwardMover(SerializationInfo info, StreamingContext context) {
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
        }
    }
    //public struct ForwardTargetMover : IComponent {
    //    public int Owner { get; set; }
    //}
}
