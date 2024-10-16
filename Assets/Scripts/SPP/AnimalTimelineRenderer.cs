using System;
using System.Collections.Generic;
using System.Linq;
using DavidUtils.ExtensionMethods;
using UnityEngine;

namespace SILVO.SPP
{
    [ExecuteAlways]
    public class AnimalTimelineRenderer : TimelineRenderer
    {
        public AnimalTimeline AnimalTimeline
        {
            get => (AnimalTimeline)(timeline ??= GetComponent<AnimalTimeline>());
            set
            {
                timeline = value;
                UpdateTimeline();
            }
        }
        
        public static Dictionary<SPP_Signal.SignalType, bool> checkpointTypeVisibility = new()
        {
            {SPP_Signal.SignalType.Seq, false},
            {SPP_Signal.SignalType.Poll, false},
            {SPP_Signal.SignalType.Warn, true},
            {SPP_Signal.SignalType.Pulse, true},
            {SPP_Signal.SignalType.Unknown, false}
        };

        private static SPP_Signal.SignalType[] VisibleTypes =>
            checkpointTypeVisibility.Where(v => v.Value)
                .Select(v => v.Key).ToArray();

        private Vector3[] VisibleCheckpoints => VisibleTypes
            .SelectMany(type => AnimalTimeline.CheckpointsByType(type))
            .ToArray();
        
        public void SetTypeVisible(SPP_Signal.SignalType type, bool visible)
        {
            checkpointTypeVisibility[type] = visible;
            UpdateCheckPoints();
        }
        
        public override void UpdateCheckPoints()
        {
            if (!ShowCheckpoints || Timeline.IsEmpty)
            {
                Clear();
                return;
            }
            
            // Debug.Log("Updating Checkpoints\n" +
            //           $"{VisibleCheckpoints.Length} visible checkpoints / {timeline.Checkpoints.Count}\n" +
            //           $"{string.Join("\n", VisibleCheckpoints.Select(p => p.ToString()))}");
            UpdateAllObj(VisibleCheckpoints.Select(p => transform.InverseTransformPoint(p)));
            UpdateColorsByType();
        }

        #region COLORS

        private Color WarnColor
        {
            get => SPP_Signal.GetSignalColor(SPP_Signal.SignalType.Warn);
            set => SPP_Signal.SetSignalColor(SPP_Signal.SignalType.Warn, value);
        }
        private Color PulseColor
        {
            get => SPP_Signal.GetSignalColor(SPP_Signal.SignalType.Pulse);
            set => SPP_Signal.SetSignalColor(SPP_Signal.SignalType.Pulse, value);
        }

        public void UpdateColorsByType()
        {
            Colors = VisibleTypes.SelectMany(type =>
                SPP_Signal.GetSignalColor(type)
                    .ToFilledArray(
                        AnimalTimeline.SignalsOrdered.Count(s => s.type == type)))
                .ToArray();
        }
        
        public override void AddCheckpoint(Vector3 checkpoint)
        {
            base.AddCheckpoint(checkpoint);
            UpdateColorsByType();
        }
        
        public override void InsertCheckpoint(int index, Vector3 checkpoint)
        {
            base.InsertCheckpoint(index, checkpoint);
            UpdateColorsByType();
        }
        
        public override void RemoveCheckpoint(int index, Vector3 checkpoint)
        {
            base.RemoveCheckpoint(index, checkpoint);
            UpdateColorsByType();
        }
        
        public override void MoveCheckpoint(int index, Vector3 checkpoint)
        {
            base.MoveCheckpoint(index, checkpoint);
            UpdateColorsByType();
        }

        #endregion
       
    }
}
