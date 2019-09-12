﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EventGenerateCollisionEvent : IActionEvent {

        public ActionStateEvents StateEvent { get; }

        public List<IActionImpact> Impacts;

        public EventGenerateCollisionEvent(ActionStateEvents stateEvent, List<IActionImpact> impacts) {
            StateEvent = stateEvent;
            Impacts = impacts;
        }

        public void Trigger(ActionUsingNode node, string eventName) {
            var entity = node.Entity;
            var cmdTarget = entity.Get<CommandTarget>();
            Entity target;
            if (cmdTarget != null && cmdTarget.Target != null) {
                target = cmdTarget.Target;
            }
            else {
                target = node.Entity;
            }
            if (target != null) {
                CollisionExtensions.GenerateHitLocDir(entity, target, out var hitPoint, out var dir);
                var ce = new CollisionEvent(node.ActionEvent.Action.Entity, target, hitPoint, dir, Impacts);
                target.Post(ce);
                entity.Post(new PerformedCollisionEvent(entity, target, ce.HitPoint, ce.HitNormal, Impacts));
                var stateEvent = new ActionStateEvent(node.Entity, ce.Target, ce.HitPoint, Quaternion.LookRotation(ce.HitNormal), StateEvent);
                node.Entity.Post(stateEvent);
            }
        }
    }
}