using System;
using System.Linq;
using DavidUtils.ExtensionMethods;
using SILVO.GEO_Tools.SPP;
using UnityEditor;
using Fields = DavidUtils.Editor.DevTools.CustomFields.MyInputFields;

namespace SILVO.Editor.SPP
{
    [CustomEditor(typeof(AnimalTimelineRenderer))]
    public class AnimalTimelineRendererEditor: TimelineRendererEditor
    {
        [CustomPropertyDrawer(typeof(AnimalTimelineRenderer.SignalTypeBoolDictionary))]
        public class AnySerializableDictionaryPropertyDrawer :
            SerializableDictionaryPropertyDrawer {}
        
        
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

            CheckpointsGUI(serializedObject);
            
            EditorGUILayout.Separator();
            
            LineRendererGUI(serializedObject);
            
            EditorGUILayout.Separator();
            
            CommonPropsGUI(serializedObject);
            
            EditorGUILayout.Separator();
            
            TestingGUI(renderer);
        }

        public new static void CheckpointsGUI(SerializedObject serializedObject)
        {
            var renderers = serializedObject.targetObjects.Cast<AnimalTimelineRenderer>().ToArray();
            if (renderers.IsNullOrEmpty()) return;
            
            Fields.InputField_Multiple<AnimalTimelineRenderer>(
                serializedObject,
                "showCheckpoints",
                "Show Checkpoints",
                r => r.UpdateCheckPoints());

            if (renderers.All(r => !r.ShowCheckpoints)) return;
            
            EditorGUILayout.Separator();

            EditorGUI.indentLevel++;
            
            Fields.InputField_Multiple<AnimalTimelineRenderer>(
                serializedObject,
                "renderMode",
                "Point Renderer",
                r => r.UpdateRenderMode());
            Fields.InputField_Multiple<AnimalTimelineRenderer>(
                serializedObject,
                "radius",
                "Point Radius",
                r => r.UpdateRadius());

            EditorGUILayout.Separator();
            
            EditorGUI.BeginChangeCheck();
            Fields.InputField_Multiple<AnimalTimelineRenderer>(
                serializedObject,
                "checkpointTypeVisibility",
                "Checkpoint Visibility",
                r => r.UpdateCheckPoints(),
                Fields.FieldOptions.ToggleLeft);
            if (EditorGUI.EndChangeCheck())
                renderers.ForEach(r => r.UpdateCheckPoints());
            
            EditorGUILayout.Separator();

            CheckpointColorBySignalGUI(() => renderers.ForEach(r => r.UpdateColorsByType()) );

            EditorGUI.indentLevel--;
        }

        public static void CheckpointColorBySignalGUI(Action onChanged)
        {
            colorFoldout = EditorGUILayout.Foldout(colorFoldout, "Signal Colors", true, EditorStyles.foldoutHeader);
            if (!colorFoldout) return;

            EditorGUI.indentLevel++;
                
            SPP_Signal.SignalType[] signalTypes = SPP_Signal.Types;
                
            signalTypes.ForEach(type =>
            {
                var color = AnimalTimelineRenderer.GetSignalColor(type);
                    
                EditorGUI.BeginChangeCheck();
                color = EditorGUILayout.ColorField(type.ToString(), color);
                
                if (EditorGUI.EndChangeCheck())
                {
                    // Undo.RecordObject(renderer, UndoName_CheckpointsSignalColorChanged);
                    AnimalTimelineRenderer.SetSignalColor(type, color);
                    onChanged();
                }
            });
                
            EditorGUI.indentLevel--;
        }
    }
}
