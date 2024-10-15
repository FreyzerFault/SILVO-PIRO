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
        [SerializeField]
        private int _id = -1;
        public int ID => _id;

        private AnimalTimelineRenderer atRenderer;

        #region SIGNALS

        private Dictionary<DateTime, SPP_Signal> _timeline = new();
        
        public DateTime[] TimesStamps => _timeline.Keys.OrderBy(t => t).ToArray();
        public SPP_Signal[] SignalsOrdered
        {
            get => _timeline.Values.OrderBy(s => s.SentDateTime).ToArray();
            set
            {
                _timeline = value.ToDictionaryByDate();
                _id = SignalsOrdered.First().id;
                Checkpoints = _timeline.Values.Select(s => s.Position).Select(GetPositionOnTerrain).ToList();
                Debug.Log($"Checkpoints updated with {Checkpoints.Count}\n" +
                          $"Dictionary updated with {_timeline.Count}");
            }
        }

        public SPP_Signal.SignalType[] SignalTypes => SignalsOrdered.Select(s => s.type).ToArray();
        
        public Vector3[] CheckpointsByType(SPP_Signal.SignalType type) =>
            Checkpoints.FromIndices(SignalsOrdered.AllIndices(s => s.type == type)).ToArray();

        
        #endregion


        protected override void OnEnable()
        {
            atRenderer = GetComponent<AnimalTimelineRenderer>() ?? gameObject.AddComponent<AnimalTimelineRenderer>();
            atRenderer.Timeline = this;
        }

        public SPP_Signal this[int index] => SignalsOrdered[index];
        public SPP_Signal this[DateTime time] => _timeline[time];
        
        
        #region CRUD

        public void AddSignal(SPP_Signal signal)
        {
            _timeline[signal.SentDateTime] = signal;
            int index = SignalsOrdered.IndexOf(signal);
            InsertCheckpoint(index, GetPositionOnTerrain(signal.Position));
        }

        public void AddSignals(SPP_Signal[] signals)
        {
            signals.ForEach(s => _timeline.Add(s.SentDateTime, s));
            var signalsOrdered = SignalsOrdered;
            signals.ForEach(s => InsertCheckpoint(signalsOrdered.IndexOf(s), GetPositionOnTerrain(s.Position)));
        }
        
        public void RemoveSignal(SPP_Signal signal)
        {
            _timeline.Remove(signal.SentDateTime);
            RemoveCheckpoint(SignalsOrdered.IndexOf(signal));
        }

        #endregion


        #region TERRAIN POSITION

        private Vector3 GetPositionOnTerrain(Vector2 pos) =>
            TerrainManager.Instance.GetRelativeTerrainPositionWithHeight(pos);
        
        #endregion
        
        
        #region LOG

        public string[] GetSignalsLog() => SignalsOrdered.Select(s =>
            $"[{s.SignalTypeLabel}] on {s.Position} | SENT: {s.SentDateTime} - RECEIVED: {s.ReceivedDateTime}").ToArray();

        #endregion
    }
}
