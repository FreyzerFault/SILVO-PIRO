using System;
using System.Collections.Generic;
using System.Linq;
using DavidUtils.ExtensionMethods;
using SILVO.Terrain;
using UnityEngine;

namespace SILVO.SPP
{
    public class AnimalTimeline : Timeline
    {
        private int _id = -1;
        public int ID => _id;

        private Dictionary<DateTime, SPP_Signal> _timeline = new();
        
        public DateTime[] TimesStamps => _timeline.Keys.ToArray();
        public SPP_Signal[] Signals
        {
            get => _timeline.Values.ToArray();
            set
            {
                _timeline = value.Distinct().Where(s1 => value.Count(s2 => s2.sentTime == s1.sentTime) <= 1)
                    .ToDictionary(s => s.sentTime);
                UpdateSignals();
            }
        }

        public Vector2[] Positions => _timeline.Values.Select(s => s.position).ToArray();
        public SPP_Signal.SignalType[] Messages => _timeline.Values.Select(s => s.type).ToArray();
        
        private Dictionary<DateTime, Vector3> _positionsOnTerrain = new();
        public override Vector3[] Checkpoints => _positionsOnTerrain.Values.ToArray();

        private void Start()
        {
            if (Signals.IsNullOrEmpty()) return;
            
            UpdateSignals();
        }

        public SPP_Signal this[int index] => Signals[index];
        public SPP_Signal this[DateTime time] => _timeline[time];


        private void OnEnable() => onCheckpointAdded += UpdateCheckpointColors;
        private void OnDisable() => onCheckpointAdded -= UpdateCheckpointColors;


        private void UpdateSignals()
        {
            if (_id == -1)
                _id = Signals[0].id;
            
            _positionsOnTerrain = GetPositionsOnTerrainTimeline();
            
            Renderer.Timeline = this;
            UpdateCheckpointColors();
        }

        #region CRUD

        public void AddSignal(SPP_Signal signal)
        {
            _timeline[signal.sentTime] = signal;
            _positionsOnTerrain[signal.sentTime] = GetPositionOnTerrain(signal.position);
            onCheckpointAdded?.Invoke();
        }

        public void AddSignals(SPP_Signal[] signals) => signals.ForEach(AddSignal);

        #endregion


        #region TERRAIN POSITION

        private float GetHeight(Vector2 pos) => TerrainManager.Instance.Terrain.GetInterpolatedHeight(pos);
        private float GetHeight(DateTime time) => GetHeight(_timeline[time].position);
        private float GetHeight(int index) => GetHeight(Positions[index]);

        private Vector3 GetPositionOnTerrain(Vector2 pos) =>
            TerrainManager.Instance.GetRelativeTerrainPositionWithHeight(pos);
        private Vector3 GetPositionOnTerrain(DateTime time) => GetPositionOnTerrain(_timeline[time].position);
        private Vector3 GetPositionOnTerrain(int index) => GetPositionOnTerrain(Positions[index]);
        
        private Vector3[] GetPositionsOnTerrain() => Positions.Select(GetPositionOnTerrain).ToArray();
        private Dictionary<DateTime, Vector3> GetPositionsOnTerrainTimeline() =>
            TimesStamps.ToDictionary((time) => time, GetPositionOnTerrain);

        #endregion
        
        
        #region RENDERING
        
        public void UpdateCheckpointColors() => Renderer.Colors = Signals.Select(s => SPP_Signal.GetSignalColor(s.type)).ToArray();

        #endregion
    }
}
