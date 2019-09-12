﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class FlyingNode : BaseNode {

        private CachedComponent<FlightEngine> _flight = new CachedComponent<FlightEngine>();
        private CachedComponent<FakeFlightEngine> _fakeFlight = new CachedComponent<FakeFlightEngine>();
        private CachedComponent<HoverEngine> _hover = new CachedComponent<HoverEngine>();
        private CachedComponent<FlightControl> _control = new CachedComponent<FlightControl>();
        private CachedComponent<CosmeticFlightBanking> _banking = new CachedComponent<CosmeticFlightBanking>();
        private CachedComponent<RigidbodyComponent> _rigidBody = new CachedComponent<RigidbodyComponent>();
        private CachedComponent<FlightPlayerInput> _playerInput = new CachedComponent<FlightPlayerInput>();
        private CachedComponent<FlightMoveInput> _moveInput = new CachedComponent<FlightMoveInput>();
        private CachedComponent<SimpleProjectileSpawner> _projectile = new CachedComponent<SimpleProjectileSpawner>();
        public FlightEngine Engine => _flight.c;
        public FakeFlightEngine FakeFlight => _fakeFlight.c;
        public HoverEngine Hover => _hover.c;
        public FlightControl Control => _control.c;
        public CosmeticFlightBanking Banking => _banking.c;
        public Rigidbody Rigidbody => _rigidBody.c.Rb;
        public FlightPlayerInput PlayerInput => _playerInput.c;
        public FlightMoveInput MoveInput => _moveInput.c;
        public SimpleProjectileSpawner Projectile => _projectile.c;

        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _flight, _control, _rigidBody, _fakeFlight, _hover, _banking, _playerInput, _moveInput, _projectile
        };

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(FlightControl),
                typeof(RigidbodyComponent)
            };
        }

        public void SetEngineActivation(bool setActivated) {
            Control.EnginesActivated = setActivated;
            if (!Control.EnginesActivated) {
                Control.ClearValues();
            }
        }

        public void UpdateControl() {
            if (PlayerInput != null) {
                PlayerInput.UpdateControl(Control);
            }
            if (MoveInput != null) {
                MoveInput.UpdateControl(Control);
            }
        }
    }

    public class NpcFlyingNode : FlyingNode {

        private const int WanderRadius = 20;
        
        private CachedComponent<MoveTarget> _moveTarget = new CachedComponent<MoveTarget>();
        private CachedComponent<SensorTargetsComponent> _sensorTargets = new CachedComponent<SensorTargetsComponent>();

        private Timer _wanderTimer = new Timer(6f, false);

        public bool Chasing = false;
        public MoveTarget MoveTarget => _moveTarget.c;
        public SensorTargetsComponent SensorTargets => _sensorTargets.c;
        

        public override List<CachedComponent> GatherComponents {
            get {
                var baseList = base.GatherComponents;
                baseList.Add(_moveTarget);
                baseList.Add(_sensorTargets);
                return baseList;
            }
        }

        public bool TryWander() {
            if (_wanderTimer.IsActive) {
                return true;
            }
            Chasing = false;
            var position = World.Get<PathfindingSystem>().FindOpenPosition(Entity.Tr.position, WanderRadius);
            if (position == Entity.Tr.position) {
                return false;
            }
            _wanderTimer.Restart();
            Entity.Post(new SetMoveTarget(null, position));
            return true;
        }

        public void Stop() {
            MoveTarget.ClearMove();
            Entity.Tags.Remove(EntityTags.Moving);
        }

        public bool Chase(Entity target) {
            //var gridPosition = target.Get<GridPosition>().Position;
            //if (gridPosition.IsNeighbor(Position.c.Position)) {
            //    return false;
            //}
            if (!World.Get<FactionSystem>().AreEnemies(Entity, target)) {
                return false;
            }
            Chasing = true;
            Entity.Post(new SetMoveTarget(target.Tr, null));
            Entity.Post(new SetLookTarget(target, false));
#if DEBUG
            DebugLog.Add(Entity.DebugId + " chasing " + target.DebugId);
#endif
            return true;
        }

        public void StopFaceTarget(Entity target) {
            Stop();
            if (target == Entity) {
                return;
            }
            Entity.Post(new SetLookTarget(target, true));
        }

        public static new System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(FlightControl),
                typeof(RigidbodyComponent),
                typeof(SimpleProjectileSpawner),
                typeof(SensorTargetsComponent),
            };
        }
    }
}