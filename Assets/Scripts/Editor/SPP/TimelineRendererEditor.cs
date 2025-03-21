using System.Linq;
using DavidUtils.Editor.Rendering;
using DavidUtils.ExtensionMethods;
using SILVO.GEO_Tools.SPP;
using UnityEditor;
using UnityEngine;
using Fields = DavidUtils.Editor.DevTools.CustomFields.MyInputFields;

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
            
            EditorGUILayout.Separator();
            
            CommonPropsGUI(serializedObject);
        }


        public static void TimelineInfoGUI(TimelineRenderer renderer)
        {
            EditorGUILayout.LabelField($"{renderer.Timeline.PointCount} Checkpoints", EditorStyles.boldLabel);
        }

        public static void CommonPropsGUI(SerializedObject serializedObject)
        {
            TimelineRenderer[] renderers = serializedObject.targetObjects.Cast<TimelineRenderer>().ToArray();
            if (renderers.IsNullOrEmpty()) return;
            
            Fields.InputField_Multiple<TimelineRenderer>(
                serializedObject,
                "terrainHeightOffset",
                "Height Offset",
                r => r.UpdateTimeline()
            );
        }

        public static void CheckpointsGUI(SerializedObject serializedObject)
        {
            TimelineRenderer[] renderers = serializedObject.targetObjects.Cast<TimelineRenderer>().ToArray();
            if (renderers.IsNullOrEmpty()) return;

            // SHOW CHECKPOINTS
            EditorGUI.BeginChangeCheck();
            
            bool showCheckpoints = EditorGUILayout.Toggle("Show Checkpoints", renderers.Any(r => r.ShowCheckpoints));
            
            if (EditorGUI.EndChangeCheck()) 
                renderers.ForEach(r => r.ShowCheckpoints = showCheckpoints);

            if (!showCheckpoints) return;
            
            EditorGUI.indentLevel++;
            
            Fields.InputField_Multiple<TimelineRenderer>(
                serializedObject,
                "renderMode",
                "Render Mode",
                r => r.UpdateRenderMode());
            
            Fields.InputField_Multiple<TimelineRenderer>(
                serializedObject,
                "radius",
                "Point Radius",
                r => r.UpdateRadius());
            
            Fields.InputField_Multiple<TimelineRenderer>(
                serializedObject,
                "colorPaletteData",
                "Color",
                r => r.UpdateColor());
            
            EditorGUI.indentLevel--;     

            if (Fields.UndoRedoPerformed) renderers.ForEach(r =>
            {
                r.UpdateRenderMode();
                r.UpdateRadius();
                r.UpdateColor();
            });
        }

        public static void LineRendererGUI(SerializedObject serializedObject)
        {
            TimelineRenderer[] renderers = serializedObject.targetObjects.Cast<TimelineRenderer>().ToArray();
            
            timelineFoldout = EditorGUILayout.Foldout(timelineFoldout, "TIMELINE", true, EditorStyles.foldoutHeader);
            if (!timelineFoldout) return;
            
            EditorGUI.indentLevel++;
            
            // TODO Pudiera estar sin inicializar?
            // if (renderers.lrNext == null || renderers.lrPrev == null) renderers.InitializeLineRenderers();
                
            Fields.InputField_Multiple<TimelineRenderer>(serializedObject, "lineVisible", "Visible", r => r.UpdateLineVisible());
            Fields.InputField_Multiple<TimelineRenderer>(serializedObject, "lineColor", "Line Color", r => r.UpdateLineColor());
            Fields.InputField_Multiple<TimelineRenderer>(serializedObject, "lineColorCompleted", "Line Color when Completed", r => r.UpdateLineColor());
            Fields.InputField_Multiple<TimelineRenderer>(serializedObject, "lineWidth", "Line Width", r => r.UpdateLineWidth());
            
            EditorGUI.indentLevel--;

            if (Fields.UndoRedoPerformed) renderers.ForEach(r => r.UpdateLineRendererAppearance());
        }
        
        
    }
}
