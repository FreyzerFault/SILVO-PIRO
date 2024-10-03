using System;
using System.Linq;
using SILVO.Asset_Importers;
using SILVO.SPP;
using UnityEditor;
using UnityEngine;

namespace SILVO.Editor.SPP
{
    [CustomEditor(typeof(Timeline))]
    public class TimelineEditor: UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var timeline = (Timeline) target;
            if (timeline == null) return;
            
            if (timeline.IsEmpty)
            {
                EditorGUILayout.LabelField("Empty Timeline", EditorStyles.centeredGreyMiniLabel);
                return;
            }
            
            EditorGUILayout.LabelField($"{timeline.PointCount} Checkpoints", EditorStyles.boldLabel);
            
            EditorGUILayout.Separator();

            EditorGUI.BeginChangeCheck();
            timeline.renderer.lineColor = EditorGUILayout.ColorField("Line Color", timeline.renderer.lineColor);
            if (EditorGUI.EndChangeCheck()) 
                timeline.renderer.UpdateLineColor();
        }
    }

    [CustomEditor(typeof(AnimalTimeline), true)]
    public class AnimalTimelineEditor : TimelineEditor
    {
        private bool colorFoldout = true;
        
        public override void OnInspectorGUI()
        {
            var timeline = (AnimalTimeline) target;
            if (timeline == null) return;
            
            base.OnInspectorGUI();
            
            if (timeline.IsEmpty) return;
            
            EditorGUILayout.Separator();
            
            EditorGUILayout.LabelField($"ANIMAL TIMELINE", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField($"ID {timeline.ID}");
            EditorGUILayout.LabelField($"{timeline.TimesStamps.First()} - {timeline.TimesStamps.Last()}");
            
            EditorGUILayout.Separator();

            colorFoldout = EditorGUILayout.Foldout(colorFoldout, "Signal Color", true);

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.ColorField("Sequence", SPP_Signal.GetSignalColor(SPP_Signal.SignalType.Seq));
                EditorGUILayout.ColorField("Poll", SPP_Signal.GetSignalColor(SPP_Signal.SignalType.Poll));
                EditorGUILayout.ColorField("Warning", SPP_Signal.GetSignalColor(SPP_Signal.SignalType.Warn));
                EditorGUILayout.ColorField("Pulse", SPP_Signal.GetSignalColor(SPP_Signal.SignalType.Pulse));
            }
            if (EditorGUI.EndChangeCheck()) 
                timeline.UpdateCheckpointColors();
        }
    }
}
