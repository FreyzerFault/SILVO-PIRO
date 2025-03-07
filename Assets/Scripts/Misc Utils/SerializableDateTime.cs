using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace SILVO.Misc_Utils
{
    [Serializable]
    public class SerializableDateTime : IComparable<SerializableDateTime>
    {
        [SerializeField]
        private long ticks;
        
        public DateTime DateTime => new(ticks);

        public SerializableDateTime(DateTime dateTime) => ticks = dateTime.Ticks;

        public int CompareTo(SerializableDateTime other)
        {
            return other == null ? 1 : ticks.CompareTo(other.ticks);
        }
        
        
        public string TimeStr => DateTime.ToString("HH:mm:ss");
        public string DateStr => DateTime.ToString("dd-MM-yyyy");
        public string FullDateStr => DateTime.ToString("dd-MM-yyyy HH:mm:ss");
        
        public int Year => DateTime.Year;
        public int Month => DateTime.Month;
        public int Day => DateTime.Day;
        public int Hour => DateTime.Hour;
        public int Minute => DateTime.Minute;
        public int Second => DateTime.Second ;
        
        public float Time_H => Time_M / 60;
        public float Time_M => Time_S / 60;
        public float Time_S => Time_MS / 1000;
        public float Time_MS => DateTime.Hour * 3600000 + DateTime.Minute * 60000 + DateTime.Second * 1000 + DateTime.Millisecond;
    }
}
