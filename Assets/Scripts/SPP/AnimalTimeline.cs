using System;
using System.Collections.Generic;
using System.Linq;
using DavidUtils.ExtensionMethods;
using SILVO.Terrain;
using UnityEngine;
using UnityEngine.Serialization;

namespace SILVO.SPP
{
    [ExecuteAlways]
    public class AnimalTimeline : Timeline
    {
        [SerializeField] private int id = -1;
        public int ID => id;

        private AnimalTimelineRenderer _atRenderer;

        #region SIGNALS

        private Dictionary<DateTime, SPP_Signal> _timeline = new();
        
        public DateTime[] TimesStamps => _timeline.Keys.OrderBy(t => t).ToArray();
        public SPP_Signal[] Signals
        {
            get => _timeline.Values.ToArray();
            set
            {
                
                _timeline = value.ToDictionaryByDate();
                
                Debug.Log($"Setting Checkpoints: {_timeline.Count}\n" +
                          $"First: {_timeline.Values.First().Position} to {TerrainManager.Instance.WorldToTerrain3D(_timeline.Values.First().Position)}\n");
                
                id = Signals.First().id;
                Checkpoints = _timeline.Values.Select(s => TerrainManager.Instance.WorldToTerrain3D(s.Position)).ToList();
            }
        }

        public SPP_Signal.SignalType[] SignalTypes => Signals.Select(s => s.type).ToArray();
        
        public Vector3[] CheckpointsByType(SPP_Signal.SignalType type) =>
            Checkpoints.FromIndices(Signals.AllIndices(s => s.type == type)).ToArray();

        
        #endregion


        protected override void OnEnable()
        {
            _atRenderer = GetComponent<AnimalTimelineRenderer>() ?? gameObject.AddComponent<AnimalTimelineRenderer>();
            _atRenderer.Timeline = this;
        }

        public SPP_Signal this[int index] => Signals[index];
        public SPP_Signal this[DateTime time] => _timeline[time];
        
        
        #region CRUD

        public void AddSignal(SPP_Signal signal)
        {
            _timeline[signal.SentDateTime] = signal;
            int index = Signals.IndexOf(signal);
            InsertCheckpoint(index, TerrainManager.Instance.WorldToTerrain3D(signal.Position));
        }

        public void AddSignals(SPP_Signal[] signals)
        {
            signals.ForEach(s => _timeline.Add(s.SentDateTime, s));
            var signalsOrdered = Signals;
            signals.ForEach(s =>
                InsertCheckpoint(signalsOrdered.IndexOf(s), TerrainManager.Instance.WorldToTerrain3D(s.Position)));
        }
        
        public void RemoveSignal(SPP_Signal signal)
        {
            _timeline.Remove(signal.SentDateTime);
            RemoveCheckpoint(Signals.IndexOf(signal));
        }

        #endregion
        
        
        #region LOG

        public string[] GetSignalsLog() => Signals.Select(s =>
            $"[{s.SignalTypeLabel}] on {s.Position} | SENT: {s.SentDateTime} - RECEIVED: {s.ReceivedDateTime}").ToArray();

        #endregion
    }
}
