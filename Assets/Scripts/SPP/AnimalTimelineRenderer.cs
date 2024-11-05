using System;
using System.Collections.Generic;
using System.Linq;
using DavidUtils.Collections;
using DavidUtils.ExtensionMethods;
using External_Packages.SerializableDictionary;
using UnityEngine;
using SignalType = SILVO.SPP.SPP_Signal.SignalType;

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
        
        [Serializable]
        public class SignalTypeBoolDictionary : SerializableDictionaryByEnum<SignalType, bool>
        {}
        
        
        [SerializeField]
        public SignalTypeBoolDictionary checkpointTypeVisibility = new();

        private SignalType[] VisibleTypes =>
            checkpointTypeVisibility.Where(v => v.Value)
                .Select(v => v.Key).ToArray();

        private Vector3[] VisibleCheckpoints => VisibleTypes
            .SelectMany(type => AnimalTimeline.CheckpointsByType(type))
            .Select(ToRenderPosition)
            .ToArray();
        
        public void SetTypeVisible(SignalType type, bool visible)
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
            
            UpdateAllObj(VisibleCheckpoints);
            UpdateColorsByType();
        }

        #region COLORS

        private static Dictionary<SignalType, Color> signalColor = new()
        {
            {SignalType.Seq, Color.white},
            {SignalType.Poll, Color.cyan},
            {SignalType.Warn, Color.yellow},
            {SignalType.Pulse, Color.red},
            {SignalType.Unknown, Color.gray},
        };
        
        public static Color GetSignalColor(SignalType signalType) => signalColor[signalType];
        public static Color SetSignalColor(SignalType signalType, Color color) => signalColor[signalType] = color;

        private Color WarnColor
        {
            get => GetSignalColor(SignalType.Warn);
            set => SetSignalColor(SignalType.Warn, value);
        }
        private Color PulseColor
        {
            get => GetSignalColor(SignalType.Pulse);
            set => SetSignalColor(SignalType.Pulse, value);
        }

        public override void UpdateColor()
        {
            UpdateColorsByType();
        }

        public void UpdateColorsByType()
        {
            colors = VisibleTypes.SelectMany(type =>
                GetSignalColor(type)
                    .ToFilledArray(
                        AnimalTimeline.Signals.Count(s => s.type == type)))
                .ToArray();
            base.UpdateColor();
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
