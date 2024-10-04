using System.Collections.Generic;
using System.Linq;
using DavidUtils;
using DavidUtils.ExtensionMethods;
using UnityEngine;

namespace SILVO.SPP
{
    public class SPP_TimelineManager: MonoBehaviour
    {
        public GameObject animalTimelinePrefab;
        
        [SerializeField, HideInInspector]
        private SPP_Signal[] signals;

        public SPP_Signal[] Signals
        {
            get => signals;
            set
            {
                signals = value;
                UpdateSignals();
            }
        }
        public Dictionary<int, SPP_Signal[]> SignalsPerId { get; private set; } = new(); 
        
        private List<AnimalTimeline> _timelines = new();
        public List<AnimalTimeline> Timelines
        {
            get => _timelines;
            set
            {
                _timelines = value;
                UpdateSignals();
            }
        }

        public int TimelineCount => _timelines.Count;

        private void Awake()
        {
            _timelines = GetComponentsInChildren<AnimalTimeline>().ToList();
            UpdateSignals();
        }

        private void InstantiateAnimalTimelines()
        {
            SignalsPerId.ForEach(pair =>
            {
                GameObject obj = Instantiate(animalTimelinePrefab, transform);
                obj.name = $"AnimalTimeline_{pair.Key}";
                AnimalTimeline timeline = obj.GetComponent<AnimalTimeline>();
                _timelines.Add(timeline);
                timeline.Signals = pair.Value;  
            });
        }


        private void UpdateSignals()
        {
            if (signals.IsNullOrEmpty()) Clear();
            
            UpdateSignalsPerId();
            if (SignalsPerId.Count != _timelines.Count)
            {
                Clear();
                InstantiateAnimalTimelines();
            }
            else
                _timelines.ForEach(tl => tl.Signals = SignalsPerId[tl.ID]);
        }

        private void UpdateSignalsPerId() => 
            SignalsPerId = Signals.GroupBy(s => s.id).ToDictionary(group => group.Key, group => group.ToArray());


        public void Clear()
        {
            UnityUtils.DestroySafe(_timelines);
            _timelines = new List<AnimalTimeline>();
        }
    }
}
