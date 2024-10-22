using System;
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
            
            CheckpointsGUI(renderer);
            
            EditorGUILayout.Separator();
            
            LineRendererGUI(renderer);
            
            EditorGUILayout.Separator();
            
            TestingGUI(renderer);
        }


        public static void TimelineInfoGUI(TimelineRenderer renderer)
        {
            EditorGUILayout.LabelField($"{renderer.Timeline.PointCount} Checkpoints", EditorStyles.boldLabel);
        }
        

        public static void CheckpointsGUI(TimelineRenderer renderer)
        {
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

        public static void LineRendererGUI(TimelineRenderer renderer)
        {
            timelineFoldout = EditorGUILayout.Foldout(timelineFoldout, "TIMELINE", true, EditorStyles.foldoutHeader);
            if (!timelineFoldout) return;
            
            EditorGUI.indentLevel++;
            
            if (renderer.lrNext == null || renderer.lrPrev == null) renderer.InitializeLineRenderers();
                
            EditorGUI.BeginChangeCheck();
            bool visible = EditorGUILayout.Toggle("Visible", renderer.LineVisible);
            if (EditorGUI.EndChangeCheck())
            {
                // Undo.RecordObject(renderer, UndoName_LineVisibilityChanged);
                renderer.LineVisible = visible;
            }
                
            // COLOR
            {
                EditorGUI.BeginChangeCheck();
                Color lineColor = EditorGUILayout.ColorField("Line Color", renderer.LineColor);
                Color lineColorCompleted = EditorGUILayout.ColorField("Line Color when Completed", renderer.LineColorCompleted);
                if (EditorGUI.EndChangeCheck())
                {
                    // Undo.RecordObject(renderer, UndoName_LineColorChanged);
                    renderer.LineColor = lineColor;
                    renderer.LineColorCompleted = lineColorCompleted;
                }
            }
                
                
            // WIDTH
            {
                EditorGUI.BeginChangeCheck();
                    
                float lineWidth = EditorGUILayout.Slider(
                    "Line Width",
                    renderer.LineWidth,
                    0.1f, 10f);
                    
                if (EditorGUI.EndChangeCheck())
                {
                    // Undo.RecordObject(renderer, UndoName_LineWidthChanged);
                    renderer.LineWidth = lineWidth;
                }
            }
            
            EditorGUI.indentLevel--;
        }
        
        
        #region UNDO
        
        // private static string UndoName_LineVisibilityChanged => "Line Visibility Changed";
        // private static string UndoName_LineColorChanged => "Line Color Changed";
        // private static string UndoName_LineWidthChanged => "Line Width Changed";
        // protected static string UndoName_ShowCheckpointsChanged => "Show Checkpoints Changed";
        // private static string UndoName_CheckpointsBaseColorChanged => "Checkpoint Colors Changed";
        //
        // public override Undo.UndoRedoEventCallback UndoRedoEvent => delegate (in UndoRedoInfo info)
        // {
        //     base.UndoRedoEvent(info);
        //
        //     var renderer = (TimelineRenderer) target;
        //     if (renderer == null) return;
        //     
        //     // Line Visibility not needed to be updated
        //     if (info.undoName == UndoName_LineColorChanged) renderer.UpdateLineColor();
        //     if (info.undoName == UndoName_LineWidthChanged) renderer.UpdateLineWidth();
        //     if (info.undoName == UndoName_ShowCheckpointsChanged) renderer.UpdateCheckPoints();
        //     if (info.undoName == UndoName_CheckpointsBaseColorChanged) renderer.UpdateCheckPoints();
        // };

        #endregion
    }
}
