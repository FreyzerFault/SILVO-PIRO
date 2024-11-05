using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DavidUtils;
using DavidUtils.ExtensionMethods;
using UnityEngine;

namespace SILVO.SPP
{
    [ExecuteAlways]
    public class SPP_TimelineManager: MonoBehaviour
    {
        public GameObject animalTimelinePrefab;

        private void OnEnable()
        {
            _timelines = GetComponentsInChildren<AnimalTimeline>().ToList();
            UpdateSignals();
        }


        #region CSV FILE

        [SerializeField, HideInInspector]
        public SPP_CSV csv;
        
        public void ParseCSVFile(int maxLines = -1, bool freeMemoryWhenParsed = false) => Signals = csv.ParseAllSignals(maxLines, freeMemoryWhenParsed);
        public void ParseCSVFileAsync() => StartCoroutine(ParseCSVFileCoroutine());

        int batchSize = 10;
        public IEnumerator ParseCSVFileCoroutine()
        {
            (SPP_Signal, SPP_CsvLine)[] batchResults = Array.Empty<(SPP_Signal, SPP_CsvLine)>();
            for (var i = 0; i < csv.csvLines.Length; i += batchSize)
            {
                csv.csvLines.Skip(i).Take(batchSize).Select((line) => csv.ParseLine(line));
                
                Signals = csv.signals.ToArray();
                
                Debug.Log($"Parsed lines [{i},{Math.Min(csv.LineCount, i + batchSize - 1)}]. {Signals.Length} Signals Loaded");
                yield return null;
            }
            
            yield return null;
        }

        #endregion
        
        
        #region SIGNALS

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
        
        private void UpdateSignals()
        {
            if (signals.IsNullOrEmpty())
            {
                Clear();
                return;
            }
            
            RemoveDuplicates();
            OrderByTime();
            UpdateSignalsPerId();
            
            UpdateAnimalTimelines();
        }
        
        private void RemoveDuplicates() => signals = signals.Distinct().ToArray();

        private void OrderByTime() => 
            signals = signals.OrderBy(s => s.SentDateTime).ToArray();

        private void UpdateSignalsPerId() => 
            SignalsPerId = Signals
                .GroupBy(s => s.id)
                .ToDictionary(group => group.Key, group => group.ToArray());

        #endregion

        
        #region TIMELINES

        [SerializeField]
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

        public void UpdateAnimalTimelines()
        {
            UpdateSignalsPerId();
            
            if (SignalsPerId.Count != _timelines.Count)
            {
                Clear();
                InstantiateAnimalTimelines();
            }
            else
                _timelines.ForEach(tl => tl.Signals = SignalsPerId[tl.ID]);
        }
        
        private void InstantiateAnimalTimelines()
        {
            _timelines = SignalsPerId.Select(pair =>
            {
                GameObject obj = Instantiate(animalTimelinePrefab, transform);
                obj.name = $"AnimalTimeline_{pair.Key}";
                AnimalTimeline timeline = obj.GetComponent<AnimalTimeline>();
                _timelines.Add(timeline);
                timeline.Signals = pair.Value;
                return timeline;
            }).ToList();
        }
        
        public void Clear()
        {
            UnityUtils.DestroySafe(_timelines);
            _timelines = new List<AnimalTimeline>();
        }

        public void Reset()
        {
            Clear();
            UpdateAnimalTimelines();
        }

        #endregion
        
        
        public TimelineRenderer[] Renderers => Timelines.Select(tl => tl.Renderer).ToArray();
    }
}
