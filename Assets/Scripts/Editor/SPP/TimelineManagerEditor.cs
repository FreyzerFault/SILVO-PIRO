using DavidUtils.ExtensionMethods;
using SILVO.SPP;
using UnityEditor;
using UnityEngine;

namespace SILVO.Editor.SPP
{
    [CustomEditor(typeof(SPP_TimelineManager))]
    public class TimelineManagerEditor: UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var timelineManager = (SPP_TimelineManager) target;
            if (timelineManager == null) return;
            
            if (GUILayout.Button("Clear Timelines")) timelineManager.Clear();
        }
    }
}
