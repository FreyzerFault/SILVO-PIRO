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
       

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EditorGUILayout.Separator();
            
            TimelineRenderer renderer = (TimelineRenderer) target;
            if (renderer == null) return;
            
            timelineFoldout = EditorGUILayout.Foldout(timelineFoldout, "TIMELINE", true, EditorStyles.foldoutHeader);
            EditorGUI.indentLevel++;
            if (timelineFoldout) LineRendererGUI(renderer);
            EditorGUI.indentLevel--;
        }

        public static void LineRendererGUI(TimelineRenderer renderer)
        {
            if (renderer.lrNext == null || renderer.lrPrev == null) renderer.InitializeLineRenderers();
            
            EditorGUI.BeginChangeCheck();
            bool visible = EditorGUILayout.Toggle("Visible", renderer.LineVisible);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(renderer, "Line Visibility Changed");
                renderer.LineVisible = visible;
            }
            
            // COLOR
            {
                EditorGUI.BeginChangeCheck();
                Color lineColor = EditorGUILayout.ColorField("Line Color", renderer.LineColor);
                Color lineColorCompleted = EditorGUILayout.ColorField("Line Color when Completed", renderer.LineColorCompleted);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(renderer, "Line Color Changed");
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
                    Undo.RecordObject(renderer, "Line Width Changed");
                    renderer.LineWidth = lineWidth;
                }
            }
        }
        
        
        #region UNDO

        public override Undo.UndoRedoEventCallback UndoRedoEvent => delegate (in UndoRedoInfo info)
        {
            base.UndoRedoEvent(info);

            TimelineRenderer renderer = (TimelineRenderer) target;
            if (renderer == null) return;
            
            if (info.undoName == "Line Color Changed") renderer.UpdateLineColor();
            if (info.undoName == "Line Width Changed") renderer.UpdateLineWidth();
        };

        #endregion
    }
}
