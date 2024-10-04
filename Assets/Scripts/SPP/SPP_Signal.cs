using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SILVO.SPP
{
    [Serializable]
    public class SPP_Signal: IEquatable<SPP_Signal>
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

        public bool EmptySignal => position == null;

        #region SENT TIME

        public string TimeStr => sentTime.ToString("HH:mm:ss");
        public string DateStr => sentTime.ToString("dd-MM-yyyy");
        public string FullDateStr => sentTime.ToString("dd-MM-yyyy HH:mm:ss");
        
        public int Year => sentTime.Year;
        public int Month => sentTime.Month;
        public int Day => sentTime.Day;
        public int Hour => sentTime.Hour;
        public int Minute => sentTime.Minute;
        public int Second => sentTime.Second ;
        
        public float Time_H => Time_M / 60;
        public float Time_M => Time_S / 60;
        public float Time_S => Time_MS / 1000;
        public float Time_MS => sentTime.Hour * 3600000 + sentTime.Minute * 60000 + sentTime.Second * 1000 + sentTime.Millisecond;

        #endregion
        
        
        public SPP_Signal(int id = -1, DateTime receivedTime = default, DateTime sentTime = default, Vector2 position = default, SignalType type = SignalType.Seq)
        {
            this.id = id;
            this.receivedTime = receivedTime;
            this.sentTime = sentTime;
            this.position = position;
            this.type = type;
        }

        public override string ToString() => $"[{id} - {FullDateStr}]: {SignalTypeLabel} at {position}.";

        public string ToFormatedString() =>
            $"<b>[{id}</b> <color=gray>{FullDateStr}</color>]:\t" +
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

        // public int CompareTo(SPP_Signal other)
        // {
        //     if (ReferenceEquals(this, other)) return 0;
        //     if (ReferenceEquals(null, other)) return 1;
        //     int idComparison = id.CompareTo(other.id);
        //     if (idComparison != 0) return idComparison;
        //     
        //     int yearComparison = Year.CompareTo(other.Year);
        //     int monthComparison = Month.CompareTo(other.Month);
        //     int dayComparison = Day.CompareTo(other.Day);
        //     int timeSComparison = Time_S.CompareTo(other.Time_S);
        //     if (yearComparison != 0) return yearComparison;
        //     if (monthComparison != 0) return monthComparison;
        //     if (dayComparison != 0) return dayComparison;
        //     if (timeSComparison != 0) return timeSComparison;
        //     
        //     return 0;
        // }

        public bool Equals(SPP_Signal other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return id == other.id && sentTime.Equals(other.sentTime);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SPP_Signal)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(id, sentTime);
        }
    }
}
