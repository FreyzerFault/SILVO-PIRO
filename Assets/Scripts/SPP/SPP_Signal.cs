using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SILVO.Asset_Importers
{
    public class SPP_Signal: IComparable<SPP_Signal>
    {
        public enum SignalType
        {
            Seq,
            Poll,
            Warn,
            Pulse,
            Unknown,
        }

        
        public int id;
        public DateTime receivedTime;
        public DateTime sentTime;
        public Vector2 position;
        public SignalType type;
        public string SignalTypeLabel => signalLabel[type];
        public string SignalTypeString => signalStr[type];


        #region SENT TIME

        public string timeStr => sentTime.ToString("HH:mm:ss");
        public string dateStr => sentTime.ToString("dd-MM-yyyy");
        public string fullDateStr => sentTime.ToString("dd-MM-yyyy HH:mm:ss");
        
        public int year => sentTime.Year;
        public int month => sentTime.Month;
        public int day => sentTime.Day;
        public int hour => sentTime.Hour;
        public int minute => sentTime.Minute;
        public int second => sentTime.Second;
        
        public float time_h => sentTime.Hour + sentTime.Minute / 60f + sentTime.Second / 3600f + sentTime.Millisecond / 3600000f;
        public float time_m => sentTime.Hour * 60 + sentTime.Minute + sentTime.Second / 60f + sentTime.Millisecond / 60000f;
        public float time_s => sentTime.Hour * 3600 + sentTime.Minute * 60 + sentTime.Second + sentTime.Millisecond / 1000f;
        public float time_ms => sentTime.Hour * 3600000 + sentTime.Minute * 60000 + sentTime.Second * 1000 + sentTime.Millisecond;

        #endregion
        
        
        public SPP_Signal(int id = -1, DateTime receivedTime = default, DateTime sentTime = default, Vector2 position = default, SignalType type = SignalType.Seq)
        {
            this.id = id;
            this.receivedTime = receivedTime;
            this.sentTime = sentTime;
            this.position = position;
            this.type = type;
        }

        public override string ToString() => $"[{id} - {fullDateStr}]: {SignalTypeLabel} at {position}.";

        public string ToFormatedString() =>
            $"<b>[{id}</b> <color=gray>{fullDateStr}</color>]:\t" +
            $"<color={signalColorStr[type]}>{SignalTypeLabel}</color> at {position}.";

        public static SignalType GetSignalType(string typeStr) => 
            !signalStr.ContainsValue(typeStr) 
                ? SignalType.Unknown 
                : signalStr.First(s => s.Value == typeStr).Key;
        
        public static Color GetSignalColor(SignalType signalType) => signalColor[signalType];
        
        private static Dictionary<SignalType, string> signalLabel = new()
        {
            {SignalType.Seq, "Sequence"},
            {SignalType.Poll, "Poll"},
            {SignalType.Warn, "Warning"},
            {SignalType.Pulse, "Pulse"},
            {SignalType.Unknown, "Unknown"},
        };

        private static Dictionary<SignalType, string> signalStr = new()
        {
            {SignalType.Seq, "seq_msg"},
            {SignalType.Poll, "poll_msg"},
            {SignalType.Warn, "warning_message"},
            {SignalType.Pulse, "pulse_message"},
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

        public int CompareTo(SPP_Signal other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            int idComparison = id.CompareTo(other.id);
            if (idComparison != 0) return idComparison;
            
            int yearComparison = year.CompareTo(other.year);
            int monthComparison = month.CompareTo(other.month);
            int dayComparison = day.CompareTo(other.day);
            int timeSComparison = time_s.CompareTo(other.time_s);
            if (yearComparison != 0) return yearComparison;
            if (monthComparison != 0) return monthComparison;
            if (dayComparison != 0) return dayComparison;
            if (timeSComparison != 0) return timeSComparison;
            
            return 0;
        }
    }
}
