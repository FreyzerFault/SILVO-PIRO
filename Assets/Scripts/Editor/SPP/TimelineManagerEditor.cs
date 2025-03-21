using System;
using System.Linq;
using DavidUtils.Editor.DevTools.CustomFields;
using DavidUtils.Editor.Rendering;
using DavidUtils.ExtensionMethods;
using JetBrains.Annotations;
using SILVO.GEO_Tools.SPP;
using UnityEditor;
using UnityEngine;

namespace SILVO.Editor.SPP
{
    [CustomEditor(typeof(SPP_TimelineManager)), CanEditMultipleObjects]
    public class TimelineManagerEditor: UnityEditor.Editor
    {
        SPP_TimelineManager _manager;
        SPP_TimelineManager[] _managers;
        
        SerializedProperty _animalTimelinePrefab;
        
        bool _foldoutRendering = true;

        private void OnEnable()
        {
            _manager = (SPP_TimelineManager)target;
            _managers = targets.Cast<SPP_TimelineManager>().ToArray();
            _animalTimelinePrefab = serializedObject.FindProperty("animalTimelinePrefab");
        }

        public override void OnInspectorGUI()
        {
            if (_manager == null) return;
            
            MyInputFields.InputField_Multiple<SPP_TimelineManager>(
                serializedObject,
                "animalTimelinePrefab",
                "Timeline Prefab",
                manager => manager.Reset());
            
            EditorGUILayout.Separator();
            
            // Check if SIGNALS are Loaded => if not => BUTTON to Load SIGNALS
            {
                if (serializedObject.isEditingMultipleObjects && _managers.Any(m => m.Signals.IsNullOrEmpty()))
                {
                    if (GUILayout.Button("Load Timelines"))
                        _managers.ForEach(m => m.ParseCSVFileAsync());
                    else
                        return;
                }
                else if (_manager.Signals.IsNullOrEmpty())
                {
                    if (GUILayout.Button("Load Timelines"))
                        _manager.ParseCSVFileAsync();
                    else
                        return;
                }
            }
            
            if (!serializedObject.isEditingMultipleObjects)
                SignalsInfoGUI(_manager);
            else
                SignalsInfoGUI(_managers);

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            // BUTTONS
            {
                if (GUILayout.Button("Update Timelines"))
                {
                    if (serializedObject.isEditingMultipleObjects)
                        _managers.ForEach(m => m.Reset());
                    else
                        _manager.Reset();
                }

                if (GUILayout.Button("Clear Timelines"))
                {
                    if (serializedObject.isEditingMultipleObjects)
                        _managers.ForEach(m => m.Clear());
                    else
                        _manager.Reset();
                }
            }
            EditorGUILayout.Separator();
            
            _foldoutRendering = EditorGUILayout.Foldout(_foldoutRendering, "RENDERING", true, EditorStyles.foldoutHeader);
            EditorGUILayout.Separator();
            if (_foldoutRendering) 
                RenderingGUI(serializedObject);
        }


        private static void SignalsInfoGUI(SPP_TimelineManager[] managers) => 
            EditorGUILayout.LabelField($"{managers.Sum(m => m.TimelineCount)} Timelines Loaded",
                EditorStyles.largeLabel);

        private static void SignalsInfoGUI(SPP_TimelineManager manager)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.LabelField($"{manager.TimelineCount} Timelines Loaded", EditorStyles.largeLabel);
            
            EditorGUILayout.Separator();
            
            manager.Timelines.ForEach(tl =>
            {
                EditorGUILayout.LabelField($"Timeline {tl.ID} - {tl.Signals.Length} Signals");
                if (tl.Signals.NotNullOrEmpty())
                    EditorGUILayout.LabelField($"[{tl.Signals.First().SentDateTime} - {tl.Signals.Last().SentDateTime}]");
            });
            
            EditorGUI.indentLevel--;
        }

        private static void RenderingGUI(SerializedObject serializedObject)
        {
            var manager = serializedObject.targetObject as SPP_TimelineManager;
            if (manager == null) return;
            
            EditorGUI.indentLevel++;
            if (manager.TimelineCount == 0)
            {
                EditorGUILayout.HelpBox("No Timelines Loaded. Update Them", MessageType.Warning);
                return;
            }

            var prefabRenderer = manager.animalTimelinePrefab.GetComponent<AnimalTimelineRenderer>();
            if (prefabRenderer == null)
            {
                EditorGUILayout.HelpBox("No TimelineRenderer found in AnimalTimeline Prefab", MessageType.Error);
                return;
            }
            

            if (manager.TimelineCount > 0)
            {
                var timelineSerializedObj = new SerializedObject(manager.Renderers.Cast<UnityEngine.Object>().ToArray());
                
                AnimalTimelineRendererEditor.CheckpointsGUI(timelineSerializedObj);
                EditorGUILayout.Separator();
                TimelineRendererEditor.LineRendererGUI(timelineSerializedObj);
                EditorGUILayout.Separator();
                TimelineRendererEditor.CommonPropsGUI(timelineSerializedObj);
            }
            
            EditorGUI.indentLevel--;
        }
    }
}
