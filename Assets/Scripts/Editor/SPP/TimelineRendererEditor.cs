using System;
using DavidUtils.Editor.Rendering;
using SILVO.SPP;
using UnityEditor;
using UnityEngine;

namespace SILVO.Editor.SPP
{
    [CustomEditor(typeof(TimelineRenderer), true)]
    public class TimelineRendererEditor: DynamicRendererEditor, IUndoableEditor
    {
        private bool timelineFoldout = true;
        private bool checkpointsFoldout = true;
       

        public override void OnInspectorGUI()
        {
            var renderer = (TimelineRenderer) target;
            if (renderer == null) return;
            
            if (renderer.Timeline == null)
            {
                EditorGUILayout.LabelField("Empty Timeline", EditorStyles.centeredGreyMiniLabel);
                return;
            }
            
            EditorGUILayout.LabelField($"{renderer.Timeline.PointCount} Checkpoints", EditorStyles.boldLabel);
            
            EditorGUILayout.Separator();
            
            checkpointsFoldout = EditorGUILayout.Foldout(checkpointsFoldout, "CHECKPOINTS", true, EditorStyles.foldoutHeader);
            if (checkpointsFoldout)
            {
                EditorGUI.indentLevel++;
                CheckpointsGUI(renderer);
                EditorGUI.indentLevel--;                
            }
            
            timelineFoldout = EditorGUILayout.Foldout(timelineFoldout, "TIMELINE", true, EditorStyles.foldoutHeader);
            if (timelineFoldout)
            {
                EditorGUI.indentLevel++;
                LineRendererGUI(renderer);
                EditorGUI.indentLevel--;
            }
        }

        public virtual void CheckpointsGUI(TimelineRenderer renderer)
        {
            EditorGUI.BeginChangeCheck();
            bool showCheckpoints = EditorGUILayout.Toggle("Show Checkpoints", renderer.ShowCheckpoints);
            if (showCheckpoints)
            {
                EditorGUI.BeginChangeCheck();
                var baseColor = EditorGUILayout.ColorField("Points Color", renderer.BaseColor);
                if (EditorGUI.EndChangeCheck())
                {
                    renderer.BaseColor = baseColor;
                    Undo.RecordObject(renderer, UndoName_ShowCheckpointsChanged);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                renderer.ShowCheckpoints = showCheckpoints;
                Undo.RecordObject(renderer, UndoName_ShowCheckpointsChanged);
            }
        }

        public static void LineRendererGUI(TimelineRenderer renderer)
        {
            if (renderer.lrNext == null || renderer.lrPrev == null) renderer.InitializeLineRenderers();
            
            EditorGUI.BeginChangeCheck();
            bool visible = EditorGUILayout.Toggle("Visible", renderer.LineVisible);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(renderer, UndoName_LineVisibilityChanged);
                renderer.LineVisible = visible;
            }
            
            // COLOR
            {
                EditorGUI.BeginChangeCheck();
                Color lineColor = EditorGUILayout.ColorField("Line Color", renderer.LineColor);
                Color lineColorCompleted = EditorGUILayout.ColorField("Line Color when Completed", renderer.LineColorCompleted);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(renderer, UndoName_LineColorChanged);
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
                    Undo.RecordObject(renderer, UndoName_LineWidthChanged);
                    renderer.LineWidth = lineWidth;
                }
            }
        }
        
        
        #region UNDO
        
        private static string UndoName_LineVisibilityChanged => "Line Visibility Changed";
        private static string UndoName_LineColorChanged => "Line Color Changed";
        private static string UndoName_LineWidthChanged => "Line Width Changed";
        protected static string UndoName_ShowCheckpointsChanged => "Show Checkpoints Changed";
        private static string UndoName_CheckpointsBaseColorChanged => "Checkpoint Colors Changed";
        protected static string UndoName_CheckpointsSignalColorChanged => "Checkpoint Colors Changed";
        protected static string UndoName_CheckpointsVisibilityChanged => "Checkpoint Colors Changed";

        public override Undo.UndoRedoEventCallback UndoRedoEvent => delegate (in UndoRedoInfo info)
        {
            base.UndoRedoEvent(info);

            var renderer = (TimelineRenderer) target;
            if (renderer == null) return;
            
            // Line Visibility not needed to be updated
            if (info.undoName == UndoName_LineColorChanged) renderer.UpdateLineColor();
            if (info.undoName == UndoName_LineWidthChanged) renderer.UpdateLineWidth();
            if (info.undoName == UndoName_ShowCheckpointsChanged) renderer.UpdateCheckPoints();
            if (info.undoName == UndoName_CheckpointsBaseColorChanged) renderer.UpdateCheckPoints();
        };

        #endregion
    }
}
