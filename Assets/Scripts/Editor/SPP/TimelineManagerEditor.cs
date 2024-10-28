using System;
using System.Linq;
using DavidUtils.Editor.Rendering;
using DavidUtils.ExtensionMethods;
using JetBrains.Annotations;
using SILVO.SPP;
using UnityEditor;
using UnityEngine;

namespace SILVO.Editor.SPP
{
    [CustomEditor(typeof(SPP_TimelineManager))]
    public class TimelineManagerEditor: UnityEditor.Editor
    {
        SPP_TimelineManager _manager;
        
        SerializedProperty _animalTimelinePrefab;
        
        bool _foldoutRendering = true;

        private void OnEnable()
        {
            _manager = (SPP_TimelineManager)target;
            _animalTimelinePrefab = serializedObject.FindProperty("animalTimelinePrefab");
        }

        public override void OnInspectorGUI()
        {
            if (_manager == null) return;
            
            _manager.animalTimelinePrefab = (GameObject) EditorGUILayout.ObjectField("Timeline Prefab", _animalTimelinePrefab.objectReferenceValue, typeof(GameObject), false);
            
            EditorGUILayout.Separator();

            if (_manager.Signals.IsNullOrEmpty())
            {
                if (GUILayout.Button("Load Timelines"))
                    _manager.ParseCSVFileAsync();
                
                return;
            }
            
            SignalsInfoGUI(_manager.csv);
            
            EditorGUILayout.Separator();
            
            EditorGUILayout.LabelField($"{_manager.TimelineCount} Timelines Loaded", EditorStyles.largeLabel);
            
            EditorGUILayout.Separator();
            
            if (GUILayout.Button("Update Timelines")) _manager.UpdateAnimalTimelines();
            if (GUILayout.Button("Clear Timelines")) _manager.Clear();
            
            EditorGUILayout.Separator();
            
            _foldoutRendering = EditorGUILayout.Foldout(_foldoutRendering, "RENDERING", true, EditorStyles.foldoutHeader);
            EditorGUILayout.Separator();
            if (_foldoutRendering) 
                RenderingGUI(serializedObject);
        }

        private static void SignalsInfoGUI(SPP_CSV csv)
        {
            int validSignals = csv.validLineIndices.Length;
            int invalidSignals = csv.invalidLineIndices.Length;
            int totalSignals = csv.LineCount;
            EditorGUILayout.LabelField(validSignals + invalidSignals < totalSignals
                ? $"Loading {validSignals + invalidSignals} / {totalSignals} signals..."
                : $"Loaded {validSignals} signals and {invalidSignals} invalid signals.");
        }

        private static void RenderingGUI(SerializedObject serializedObject)
        {
            var manager = serializedObject.targetObject as SPP_TimelineManager;
            if (manager == null) return;
            
            EditorGUI.indentLevel++;
            if (manager.TimelineCount == 0)
            {
                EditorGUILayout.HelpBox("No Timelines Loaded. Update Them", MessageType.Warning);
                return;
            }

            var prefabRenderer = manager.animalTimelinePrefab.GetComponent<AnimalTimelineRenderer>();
            if (prefabRenderer == null)
            {
                EditorGUILayout.HelpBox("No TimelineRenderer found in AnimalTimeline Prefab", MessageType.Error);
                return;
            }
            

            if (manager.TimelineCount > 0)
            {
                var timelineSerializedObj = new SerializedObject(manager.Renderers.Cast<UnityEngine.Object>().ToArray());
                
                AnimalTimelineRendererEditor.CheckpointsGUI(timelineSerializedObj);
                EditorGUILayout.Separator();
                TimelineRendererEditor.LineRendererGUI(timelineSerializedObj);
            }
            
            EditorGUI.indentLevel--;
        }
    }
}
