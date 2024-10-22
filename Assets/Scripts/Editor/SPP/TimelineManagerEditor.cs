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
    public class TimelineManagerEditor: UnityEditor.Editor, IUndoableEditor
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
            
            _foldoutRendering = EditorGUILayout.Foldout(_foldoutRendering, "Rendering", true, EditorStyles.foldoutHeader);
            if (_foldoutRendering) RenderingGUI(_manager);
        }

        private static void SignalsInfoGUI(SPP_CSV csv)
        {
            int validSignals = csv.validLines.Count;
            int invalidSignals = csv.invalidLines.Count;
            int totalSignals = csv.csvLines.Count;
            EditorGUILayout.LabelField(validSignals + invalidSignals < totalSignals
                ? $"Loading {validSignals + invalidSignals} / {totalSignals} signals..."
                : $"Loaded {validSignals} signals and {invalidSignals} invalid signals.");
        }

        private static void RenderingGUI(SPP_TimelineManager manager)
        {
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
                EditorGUI.BeginChangeCheck();
                AnimalTimelineRenderer firstTimelineRenderer = manager.Timelines.First().GetComponent<AnimalTimelineRenderer>();
                AnimalTimelineRendererEditor.CheckpointsGUI(firstTimelineRenderer);

                if (EditorGUI.EndChangeCheck())
                {
                    manager.Timelines.ForEach((timeline, i) =>
                    {
                        var renderer = timeline.GetComponent<AnimalTimelineRenderer>();
                        Undo.RecordObject(renderer, UndoName_TimelineCheckpointsChanged + $"_{i}");
                        renderer.ShowCheckpoints = firstTimelineRenderer.ShowCheckpoints;
                        renderer.Mode = firstTimelineRenderer.Mode;
                        renderer.Radius = firstTimelineRenderer.Radius;
                    });
                }
            
                EditorGUILayout.Separator();
                
                EditorGUI.BeginChangeCheck();
                TimelineRendererEditor.LineRendererGUI(firstTimelineRenderer);
                if (EditorGUI.EndChangeCheck())
                {
                    manager.Timelines.ForEach((timeline, i) =>
                    {
                        Undo.RecordObject(timeline.Renderer, UndoName_TimelineLineChanged + $"_{i}");
                        timeline.Renderer.LineColor = firstTimelineRenderer.LineColor;
                        timeline.Renderer.LineColorCompleted = firstTimelineRenderer.LineColorCompleted;
                        timeline.Renderer.LineWidth = firstTimelineRenderer.LineWidth;
                        timeline.Renderer.LineVisible = firstTimelineRenderer.LineVisible;
                    });
                }
            }
            
            EditorGUI.indentLevel--;
        }

        
        #region UNDO
        
        private static string UndoName_TimelineLineChanged => "Timeline Line Changed";
        private static string UndoName_TimelineCheckpointsChanged => "Timeline Checkpoints Changed";

        public Undo.UndoRedoEventCallback UndoRedoEvent => (in UndoRedoInfo undo) =>
        {
            var manager = (SPP_TimelineManager)target;
            if (manager == null || manager.Timelines.IsNullOrEmpty()) return;

            string[] tagSlices = undo.undoName.Split("_");
            string tag = tagSlices[0];
            bool badTag = !int.TryParse(tagSlices[1], out int index);
            if (badTag)
            {
                Debug.LogError($"Bad Tag for Undo (without int): {undo.undoName}");
                return;
            }
            
            if (tag == UndoName_TimelineLineChanged)
                manager.Timelines[index].Renderer.UpdateLineRendererAppearance();
            else if (tag == UndoName_TimelineCheckpointsChanged)
            {
                manager.Timelines[index].GetComponent<AnimalTimelineRenderer>().UpdateCheckPoints();
                manager.Timelines[index].GetComponent<AnimalTimelineRenderer>().UpdateCommonProperties();
            }
        };

        // private void OnEnable() => Undo.undoRedoEvent += UndoRedoEvent;
        // private void OnDisable() => Undo.undoRedoEvent -= UndoRedoEvent;
        
        #endregion
    }
}
