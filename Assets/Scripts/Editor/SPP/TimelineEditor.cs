using System;
using System.Linq;
using DavidUtils.ExtensionMethods;
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
            timeline.Renderer.LineColor = EditorGUILayout.ColorField("Line Color", timeline.Renderer.LineColor);
            if (EditorGUI.EndChangeCheck()) 
                timeline.Renderer.UpdateLineColor();
        }
    }

    [CustomEditor(typeof(AnimalTimeline), true)]
    public class AnimalTimelineEditor : TimelineEditor
    {
        private bool signalsFoldout = false;
        private bool colorFoldout = true;
        
        public override void OnInspectorGUI()
        {
            var timeline = (AnimalTimeline) target;
            if (timeline == null) return;
            
            base.OnInspectorGUI();
            
            if (timeline.IsEmpty) return;
            
            EditorGUILayout.Separator();
            
            // TIMELINE
            {
                EditorGUILayout.LabelField($"ANIMAL TIMELINE [ID: {timeline.ID}]", EditorStyles.boldLabel);
                
                EditorGUILayout.Separator();

                EditorGUI.indentLevel++;
                
                EditorGUILayout.LabelField($"{timeline.TimesStamps.First()} - {timeline.TimesStamps.Last()}");
                
                signalsFoldout = EditorGUILayout.Foldout(signalsFoldout, "Signals", true);
                if (signalsFoldout)
                    timeline.GetSignalsLog().ForEach(log => EditorGUILayout.LabelField(log, EditorStyles.miniLabel));

                EditorGUI.indentLevel--;
            }
            
        }
    }
}
