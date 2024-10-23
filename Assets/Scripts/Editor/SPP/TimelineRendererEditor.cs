using System;
using System.Linq;
using DavidUtils.Editor.Rendering;
using SILVO.SPP;
using UnityEditor;
using UnityEngine;

namespace SILVO.Editor.SPP
{
    [CustomEditor(typeof(TimelineRenderer), true)]
    public class TimelineRendererEditor: PointsRendererEditor
    {
        private TimelineRenderer _renderer;
        
        private static bool timelineFoldout = true;
        private static bool checkpointsFoldout = true;
        
        
        
        public override void OnInspectorGUI()
        {
            var renderer = (TimelineRenderer) target;
            if (renderer == null) return;
            
            if (renderer.Timeline == null)
            {
                EditorGUILayout.LabelField("Empty Timeline", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            TimelineInfoGUI(renderer);
            
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            
            CheckpointsGUI(serializedObject);
            
            EditorGUILayout.Separator();
            
            LineRendererGUI(serializedObject);
            
            EditorGUILayout.Separator();
            
            TestingGUI(renderer);
        }


        public static void TimelineInfoGUI(TimelineRenderer renderer)
        {
            EditorGUILayout.LabelField($"{renderer.Timeline.PointCount} Checkpoints", EditorStyles.boldLabel);
        }
        

        public static void CheckpointsGUI(SerializedObject serializedObject)
        {
            var renderer = serializedObject.targetObject as TimelineRenderer;
            if (renderer == null) return;
            
            EditorGUI.BeginChangeCheck();
            bool showCheckpoints = EditorGUILayout.ToggleLeft("Show Checkpoints", renderer.ShowCheckpoints);
            if (EditorGUI.EndChangeCheck())
            {
                // Undo.RecordObject(renderer, UndoName_ShowCheckpointsChanged);
                renderer.ShowCheckpoints = showCheckpoints;
            }

            if (!showCheckpoints) return;
            
            EditorGUI.indentLevel++;
            
            // TODO
            // InputField(serializedObject.FindProperty("renderMode"), "Render Mode", _renderer.UpdateRenderMode);
            // InputField(serializedObject.FindProperty("radius"), "Point Radius", _renderer.UpdateRadius);
                
            EditorGUI.BeginChangeCheck();
            var baseColor = EditorGUILayout.ColorField("Points Color", renderer.BaseColor);
            if (EditorGUI.EndChangeCheck())
            {
                renderer.BaseColor = baseColor;
                // Undo.RecordObject(renderer, UndoName_ShowCheckpointsChanged);
            }
            
            EditorGUI.indentLevel--;     

        }

        public static void LineRendererGUI(SerializedObject serializedObject)
        {
            TimelineRenderer[] renderers = serializedObject.targetObjects.Cast<TimelineRenderer>().ToArray();
            
            timelineFoldout = EditorGUILayout.Foldout(timelineFoldout, "TIMELINE", true, EditorStyles.foldoutHeader);
            if (!timelineFoldout) return;
            
            EditorGUI.indentLevel++;
            
            // TODO Pudiera estar sin inicializar?
            // if (renderers.lrNext == null || renderers.lrPrev == null) renderers.InitializeLineRenderers();
                
            InputField_Multiple<TimelineRenderer>(serializedObject, "lineVisible", "Visible", r => r.UpdateLineVisible());
            InputField_Multiple<TimelineRenderer>(serializedObject, "lineColor", "Line Color", r => r.UpdateLineColor());
            InputField_Multiple<TimelineRenderer>(serializedObject, "lineColorCompleted", "Line Color when Completed", r => r.UpdateLineColor());
            InputField_Multiple<TimelineRenderer>(serializedObject, "lineWidth", "Line Width", r => r.UpdateLineWidth());
            
            EditorGUI.indentLevel--;
        }
        
        
    }
}
