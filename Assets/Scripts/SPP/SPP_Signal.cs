using System;
using System.Collections.Generic;
using System.Linq;
using DavidUtils.ExtensionMethods;
using DotSpatial.Projections;
using DotSpatial.Projections.ProjectedCategories;
using DotSpatial.Projections.Transforms;
using SILVO.Misc_Utils;
using SILVO.Terrain;
using UnityEngine;
using UnityEngine.Serialization;

namespace SILVO.SPP
{
    
    
    [Serializable]
    public class SPP_Signal: IEquatable<SPP_Signal>
    {
        [Serializable]
        public enum SignalType
        {
            Seq,
            Poll,
            Warn,
            Pulse,
            Unknown,
        }

        
        [SerializeField] public int id;
        [SerializeField] private SerializableDateTime receivedTime;
        [SerializeField] private SerializableDateTime sentTime;
        [SerializeField] private Vector2 positionLonLat;
        [SerializeField] private Vector2 positionUTM;
        [SerializeField] public SignalType type;
        
        public string SignalTypeLabel => signalLabel[type];
        public string SignalTypeString => signalStr[type][0];

        public DateTime ReceivedDateTime => receivedTime.DateTime;
        public DateTime SentDateTime => sentTime.DateTime;

        public Vector2 Position => positionUTM;
        
        public SPP_Signal(int id = -1, DateTime receivedTime = default, DateTime sentTime = default, Vector2 positionLonLat = default, SignalType type = SignalType.Seq)
        {
            this.id = id;
            this.receivedTime = new SerializableDateTime(receivedTime);
            this.sentTime = new SerializableDateTime(sentTime);
            this.positionLonLat = positionLonLat;
            this.type = type;

            ProjectionInfo wgsProjInfo = ProjectionInfo.FromEpsgCode(4326);
            ProjectionInfo utm30NProjInfo = ProjectionInfo.FromEpsgCode(25830);
            double[] points = { positionLonLat.x, positionLonLat.y };
            Reproject.ReprojectPoints(points, new double[] {0}, wgsProjInfo, utm30NProjInfo, 0, 1);
            positionUTM = new Vector2((float)points[0], (float)points[1]);
        }

        public override string ToString() => $"[{id} - {sentTime.FullDateStr}]: {SignalTypeLabel} at {positionLonLat}.";

        public string ToFormatedString() =>
            $"<b>[{id}</b> <color=gray>{sentTime.FullDateStr}</color>]:\t" +
            $"<color={signalColorStr[type]}>{SignalTypeLabel}</color> at {positionLonLat}.";

        public static SignalType[] GetTypes => Enum.GetValues(typeof(SignalType)).Cast<SignalType>().ToArray();
        
        public static SignalType GetSignalType(string typeStr) => 
            signalStr.Values.Any(strings => strings.Contains(typeStr)) 
                ? signalStr.First(pair => pair.Value.Contains(typeStr)).Key 
                : SignalType.Unknown;
        
        public static Color GetSignalColor(SignalType signalType) => signalColor[signalType];
        public static Color SetSignalColor(SignalType signalType, Color color) => signalColor[signalType] = color;
        
        private static Dictionary<SignalType, string> signalLabel = new()
        {
            {SignalType.Seq, "Sequence"},
            {SignalType.Poll, "Poll"},
            {SignalType.Warn, "Warning"},
            {SignalType.Pulse, "Pulse"},
            {SignalType.Unknown, "Unknown"},
        };

        private static Dictionary<SignalType, string[]> signalStr = new()
        {
            {SignalType.Seq, new []{"seq_msg", "seq_message" }},
            {SignalType.Poll, new []{"poll_msg", "poll_message" }},
            {SignalType.Warn, new []{"warn_msg", "warning_message" }},
            {SignalType.Pulse, new []{"pulse_msg", "pulse_message" }},
        };
        
        private static Dictionary<SignalType, Color> signalColor = new()
        {
            {SignalType.Seq, Color.white},
            {SignalType.Poll, Color.cyan},
            {SignalType.Warn, Color.yellow},
            {SignalType.Pulse, Color.red},
            {SignalType.Unknown, Color.gray},
        };
        
        private static Dictionary<SignalType, string> signalColorStr = new()
        {
            {SignalType.Seq, "white"},
            {SignalType.Poll, "cyan"},
            {SignalType.Warn, "yellow"},
            {SignalType.Pulse, "red"},
            {SignalType.Unknown, "gray"},
        };

        public bool Equals(SPP_Signal other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return id == other.id && sentTime.CompareTo(other.sentTime) == 0;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((SPP_Signal)obj);
        }

        public override int GetHashCode() => HashCode.Combine(id, sentTime);
    }

    public static class SPP_Signal_Extensions
    {
        public static Dictionary<DateTime, SPP_Signal> ToDictionaryByDate(this SPP_Signal[] signals) => 
            signals.Where(s1 => signals.Count(s1.Equals) == 1).ToDictionary(s => s.SentDateTime);
    }
}
