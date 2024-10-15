using System;
using System.Linq;
using DavidUtils.Editor.Rendering;
using DavidUtils.ExtensionMethods;
using SILVO.SPP;
using UnityEditor;
using UnityEngine;

namespace SILVO.Editor.SPP
{
    [CustomEditor(typeof(SPP_TimelineManager))]
    public class TimelineManagerEditor: UnityEditor.Editor, IUndoableEditor
    {
        bool foldoutRendering = true;

        public override void OnInspectorGUI()
        {
            var manager = (SPP_TimelineManager) target;
            if (manager == null) return;
            
            manager.animalTimelinePrefab = (GameObject)EditorGUILayout.ObjectField("Timeline Prefab", manager.animalTimelinePrefab, typeof(GameObject), true);
            
            EditorGUILayout.Separator();

            if (manager.Signals.IsNullOrEmpty())
            {
                if (GUILayout.Button("Load Timelines"))
                    manager.ParseCSVFileAsync();
                
                return;
            }
            
            SignalsInfoGUI(manager.csv);
            
            EditorGUILayout.Separator();
            
            EditorGUILayout.LabelField($"{manager.TimelineCount} Timelines Loaded", EditorStyles.largeLabel);
            
            EditorGUILayout.Separator();
            
            if (GUILayout.Button("Update Timelines")) manager.UpdateAnimalTimelines();
            if (GUILayout.Button("Clear Timelines")) manager.Clear();
            
            EditorGUILayout.Separator();
            
            foldoutRendering = EditorGUILayout.Foldout(foldoutRendering, "Rendering", true, EditorStyles.foldoutHeader);
            if (foldoutRendering) RenderingGUI(manager);
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

            var prefabRenderer = manager.animalTimelinePrefab.GetComponent<TimelineRenderer>();
            if (prefabRenderer == null)
            {
                EditorGUILayout.HelpBox("No TimelineRenderer found in AnimalTimeline Prefab", MessageType.Error);
                return;
            }
            
            EditorGUI.BeginChangeCheck();
            TimelineRendererEditor.LineRendererGUI(prefabRenderer);
            if (EditorGUI.EndChangeCheck())
            {
                manager.Timelines.ForEach((timeline, i) =>
                {
                    Undo.RecordObject(timeline, $"Timeline {i} Changed");
                    timeline.Renderer.LineColor = prefabRenderer.LineColor;
                    timeline.Renderer.LineColorCompleted = prefabRenderer.LineColorCompleted;
                    timeline.Renderer.LineWidth = prefabRenderer.LineWidth;
                    timeline.Renderer.LineVisible = prefabRenderer.LineVisible;
                });
            }
            
            EditorGUI.indentLevel--;
        }

        
        #region UNDO

        public Undo.UndoRedoEventCallback UndoRedoEvent => (in UndoRedoInfo undo) =>
        {
            var manager = (SPP_TimelineManager)target;
            if (manager == null || manager.Timelines.IsNullOrEmpty()) return;

            bool badTag = !int.TryParse(undo.undoName.Split(" ")[1], out int index);
            if (badTag)
            {
                Debug.LogError($"Bad Tag for Undo (without int): {undo.undoName}");
                return;
            }
            
            manager.Timelines[index].Renderer.UpdateLineRendererAppearance();
        };

        private void OnEnable() => Undo.undoRedoEvent += UndoRedoEvent;
        private void OnDisable() => Undo.undoRedoEvent -= UndoRedoEvent;
        
        #endregion
    }
}
