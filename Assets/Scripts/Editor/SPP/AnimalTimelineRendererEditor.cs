using System;
using System.Collections.Generic;
using System.Linq;
using DavidUtils.ExtensionMethods;
using SILVO.SPP;
using UnityEditor;
using UnityEngine;

namespace SILVO.Editor.SPP
{
    [CustomEditor(typeof(AnimalTimelineRenderer))]
    public class AnimalTimelineRendererEditor: TimelineRendererEditor
    {

        public override void CheckpointsGUI(TimelineRenderer renderer)
        {
            var aRenderer = (AnimalTimelineRenderer) renderer;
            if (aRenderer == null) return;
            
            EditorGUI.BeginChangeCheck();
            bool showCheckpoints = EditorGUILayout.Toggle("Show Checkpoints", renderer.ShowCheckpoints);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(renderer, UndoName_ShowCheckpointsChanged);
                renderer.ShowCheckpoints = showCheckpoints;
            }
            
            if (showCheckpoints)
            {
                if (!aRenderer.ShowCheckpoints) return;
            
                // VISIBILITY
                {
                    EditorGUI.BeginChangeCheck();
                    Dictionary<SPP_Signal.SignalType, bool> visibleChanges = new (aRenderer.checkpointTypeVisibility);
                    aRenderer.checkpointTypeVisibility.ForEach((pair) =>
                    {
                        var type = pair.Key;
                        var visible = pair.Value;
                        visibleChanges[type] = EditorGUILayout.ToggleLeft(type.ToString(), visible);
                    });
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(renderer, UndoName_CheckpointsVisibilityChanged);
                        aRenderer.checkpointTypeVisibility = visibleChanges;
                        aRenderer.UpdateCheckPoints();
                    }
                }
            
                EditorGUILayout.Separator();
                
                // COLORS
                {
                    colorFoldout = EditorGUILayout.Foldout(colorFoldout, "Signal Colors", true, EditorStyles.foldoutHeader);

                    if (!colorFoldout) return;

                    EditorGUI.indentLevel++;
                
                    SPP_Signal.SignalType[] signalTypes = SPP_Signal.GetTypes;
                
                    signalTypes.ForEach(type =>
                    {
                        var color = SPP_Signal.GetSignalColor(type);
                    
                        EditorGUI.BeginChangeCheck();
                        color = EditorGUILayout.ColorField(type.ToString(), color);
                
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(renderer, UndoName_CheckpointsSignalColorChanged);
                            SPP_Signal.SetSignalColor(type, color);
                            aRenderer.UpdateColor();
                        }
                    });
                
                    EditorGUI.indentLevel--;
                }
            }

        }
    }
}
