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

        private void Awake()
        {
            _timelines = GetComponentsInChildren<AnimalTimeline>().ToList();
            UpdateSignals();
        }


        #region CSV FILE

        [SerializeField, HideInInspector]
        public SPP_CSV csv;
        
        public void ParseCSVFile() => Signals = csv.ParseAllSignals();
        public void ParseCSVFileAsync() => StartCoroutine(ParseCSVFileCoroutine());

        int batchSize = 10;
        public IEnumerator ParseCSVFileCoroutine()
        {
            (SPP_Signal, SPP_CsvLine)[] batchResults = Array.Empty<(SPP_Signal, SPP_CsvLine)>();
            for (var i = 0; i < csv.csvLines.Count; i += batchSize)
            {
                batchResults = csv.csvLines.Skip(i).Take(batchSize).Select((line) => csv.ParseLine(line)).ToArray();
                Signals = csv.signals.ToArray();
                yield return null;
            }
            
            yield return null;
            
            // Dividimos las lineas entre validas (no nulas) e invalidas (Lineas originales de las nulas)
            csv.signals = batchResults.Where((pair) => pair.Item1 != null).Select(pair => pair.Item1).ToList();
            csv.invalidLines = batchResults.Where(pair => pair.Item1 == null).Select(pair => pair.Item2).ToList();
            
            csv.UpdateLog();
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
        
        private void RemoveDuplicates()
        { 
            Debug.Log($"Removing Duplicates. Signals: {Signals.Length} => {signals.Distinct().Count()} Signals\n");
            Debug.Log($"BEFORE: {string.Join("\n", signals.Select(s => s.ToString()))}");
            Debug.Log($"AFTER: {string.Join("\n", signals.Distinct().Select(s => s.ToString()))}");
            signals = signals.Distinct().ToArray();
        }

        private void OrderByTime() => 
            signals = signals.OrderBy(s => s.SentDateTime).ToArray();

        private void UpdateSignalsPerId() => 
            SignalsPerId = Signals
                .GroupBy(s => s.id)
                .ToDictionary(group => group.Key, group => group.ToArray());

        #endregion

        
        #region TIMELINES

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
            Debug.Log(string.Join("\n", signals.Select(s => s.ToString())));
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
            
            Debug.Log($"Timelines Loaded: {_timelines.Count}");
        }
        
        public void Clear()
        {
            UnityUtils.DestroySafe(_timelines);
            _timelines = new List<AnimalTimeline>();
        }

        #endregion
    }
}
