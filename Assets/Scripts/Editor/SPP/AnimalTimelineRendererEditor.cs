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
        public override void OnInspectorGUI()
        {
            var renderer = (AnimalTimelineRenderer) target;
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

        public static void CheckpointsGUI(AnimalTimelineRenderer renderer)
        {
            EditorGUI.BeginChangeCheck();
            
            bool showCheckpoints = EditorGUILayout.ToggleLeft("Show Checkpoints", renderer.ShowCheckpoints);
            
            if (EditorGUI.EndChangeCheck())
            {
                // Undo.RecordObject(renderer, UndoName_ShowCheckpointsChanged);
                renderer.ShowCheckpoints = showCheckpoints;
            }

            if (!showCheckpoints) return;
            
            EditorGUILayout.Separator();
            
            EditorGUI.indentLevel++;
            
            CheckpointVisibilityGUI(renderer);
                
            EditorGUILayout.Separator();
                
            // TODO
            // InputField(serializedObject.FindProperty("renderMode"), "Render Mode", _renderer.UpdateRenderMode);
            // InputField(serializedObject.FindProperty("radius"), "Point Radius", _renderer.UpdateRadius);
            
            EditorGUILayout.Separator();
                
            CheckpointColorBySignalGUI(renderer);
            
            EditorGUI.indentLevel--;
        }

        public static void CheckpointVisibilityGUI(AnimalTimelineRenderer renderer)
        {
            EditorGUILayout.LabelField("Signal Type:", EditorStyles.boldLabel);
                
            EditorGUILayout.Separator();
                
            EditorGUI.BeginChangeCheck();
            Dictionary<SPP_Signal.SignalType, bool> visibleChanges = new (AnimalTimelineRenderer.checkpointTypeVisibility);
            AnimalTimelineRenderer.checkpointTypeVisibility.ForEach((pair) =>
            {
                var type = pair.Key;
                var visible = pair.Value;
                visibleChanges[type] = EditorGUILayout.ToggleLeft(type.ToString(), visible);
            });
            if (EditorGUI.EndChangeCheck())
            {
                AnimalTimelineRenderer.checkpointTypeVisibility = visibleChanges;
                renderer.UpdateCheckPoints();
            }
        }

        public static void CheckpointColorBySignalGUI(AnimalTimelineRenderer renderer)
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
                    // Undo.RecordObject(renderer, UndoName_CheckpointsSignalColorChanged);
                    SPP_Signal.SetSignalColor(type, color);
                    renderer.UpdateColor();
                }
            });
                
            EditorGUI.indentLevel--;
        }

        
        #region UNDO
        
        // protected static string UndoName_CheckpointsSignalColorChanged => "Checkpoint Colors Changed";
        // protected static string UndoName_CheckpointsVisibilityChanged => "Checkpoint Colors Changed";
        //
        // public override Undo.UndoRedoEventCallback UndoRedoEvent => delegate (in UndoRedoInfo info)
        // {
        //     base.UndoRedoEvent(info);
        //
        //     var renderer = (AnimalTimelineRenderer) target;
        //     if (renderer == null) return;
        //     
        //     // Line Visibility not needed to be updated
        //     if (info.undoName == UndoName_CheckpointsVisibilityChanged) renderer.UpdateCheckPoints();
        //     if (info.undoName == UndoName_CheckpointsSignalColorChanged) renderer.UpdateColorsByType();
        // };
        
        #endregion
    }
}
