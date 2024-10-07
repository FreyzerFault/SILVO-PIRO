using DavidUtils.ExtensionMethods;
using SILVO.SPP;
using UnityEditor;
using UnityEngine;

namespace SILVO.Editor.SPP
{
    [CustomEditor(typeof(SPP_TimelineManager))]
    public class TimelineManagerEditor: UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var timelineManager = (SPP_TimelineManager) target;
            if (timelineManager == null) return;
            
            EditorGUILayout.Separator();
            
            if (timelineManager.Signals.IsNullOrEmpty() && GUILayout.Button("Load Timelines"))
                timelineManager.ParseCSVFile();
            
            if (timelineManager.Signals.NotNullOrEmpty())
            {
                int validSignals = timelineManager.Signals.Length;
                int invalidSignals = timelineManager.csv.invalidLines.Count;
                int totalSignals = timelineManager.csv.csvLines.Count;
                EditorGUILayout.LabelField(validSignals + invalidSignals < totalSignals
                    ? $"Loading {validSignals + invalidSignals} / {totalSignals} signals..."
                    : $"Loaded {validSignals} signals and {invalidSignals} invalid signals.");
            }
            
            EditorGUILayout.Separator();
            
            if (GUILayout.Button("Clear Timelines")) timelineManager.Clear();
        }
    }
}
