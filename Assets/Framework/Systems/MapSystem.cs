using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace PixelComrades {
<<<<<<< HEAD
    public interface IMapProvider {
        LevelCell GetCell(Point3 pos);
    }

    [Priority(Priority.Highest), AutoRegister]
    public sealed class MapSystem : SystemBase, IPeriodicUpdate {

        private IMapProvider _map;
        private TemplateList<UnitOccupyingCellTemplate> _templateList;
        private List<LevelCell> _occupiedCells = new List<LevelCell>(50);
        private ManagedArray<UnitOccupyingCellTemplate>.RefDelegate _del;
        public MapSystem() {
            TemplateFilter<UnitOccupyingCellTemplate>.Setup();
            _templateList = EntityController.GetTemplateList<UnitOccupyingCellTemplate>();
            _del = UpdateNode;
        }

        public override void Dispose() {
            base.Dispose();
            _templateList = null;
        }

        public void OnPeriodicUpdate() {
            for (int i = 0; i < _occupiedCells.Count; i++) {
                _occupiedCells[i].Occupied = null;
            }
            _occupiedCells.Clear();
            _templateList.Run(_del);
        }

        private void UpdateNode(ref UnitOccupyingCellTemplate template) {
            if (template.Entity.IsDead() || template.Tr == null) {
                return;
            }
            var position = template.Tr.position.ToCellGridP3ZeroY();
            ref var gridPos = ref template.PositionComponent.GetReference();
            gridPos.Value = position;
            var cell = GetCell(position);
            if (cell == null) {
                return;
            }
            cell.Occupied = template.Entity;
            _occupiedCells.Add(cell);
        }


        public void SetMapProvider(IMapProvider provider) {
            _map = provider;
        }
        
        public LevelCell GetCell(Point3 pos) {
            if (_map == null) {
                return null;
            }
            return _map.GetCell(pos);
=======
    [Priority(Priority.Highest), AutoRegister]
    public sealed class MapSystem : SystemBase<MapSystem> {

        private LevelCellMap _map;
        // private TemplateList<UnitOccupyingCellTemplate> _templateList;
        // private List<LevelCell> _occupiedCells = new List<LevelCell>(50);
        // private ManagedArray<UnitOccupyingCellTemplate>.RefDelegate _del;
        //
        // public MapSystem() {
        //     _templateList = EntityController.GetTemplateList<UnitOccupyingCellTemplate>();
        //     _del = UpdateNode;
        // }
        //
        //
        // public override void Dispose() {
        //     base.Dispose();
        //     _templateList = null;
        // }

        // public void OnPeriodicUpdate() {
        //     for (int i = 0; i < _occupiedCells.Count; i++) {
        //         _occupiedCells[i].Occupied = null;
        //     }
        //     _occupiedCells.Clear();
        //     _templateList.Run(_del);
        // }
        //
        // private void UpdateNode(ref UnitOccupyingCellTemplate template) {
        //     if (template.Entity.IsDead() || template.Tr == null) {
        //         return;
        //     }
        //     var position = template.Tr.position.ToMapGridP3ZeroY();
        //     ref var gridPos = ref template.PositionComponent.GetReference();
        //     gridPos.Value = position;
        //     var cell = GetCell(position);
        //     if (cell == null) {
        //         return;
        //     }
        //     cell.Occupied = template.Entity;
        //     _occupiedCells.Add(cell);
        // }

        public void SetMap(LevelCellMap map) {
            _map = map;
        }
        
        public LevelCell GetCell(Vector3 pos) {
            return _map.Get(pos);
>>>>>>> FirstPersonAction
        }

        public LevelCell GetCell(Point3 pos) {
            return _map.Get(pos);
        }
    }
}