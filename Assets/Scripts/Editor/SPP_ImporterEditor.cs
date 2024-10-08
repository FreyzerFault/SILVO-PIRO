using System;
using System.Linq;
using Csv;
using DavidUtils.ExtensionMethods;
using SILVO.Asset_Importers;
using SILVO.Editor.SPP;
using SILVO.SPP;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace SILVO.Editor
{
    public static class EditorCustomStyles
    {
        private static int smallFontSize => Mathf.RoundToInt(EditorStyles.label.fontSize * 0.8f);
        private static Font monoFont => Font.CreateDynamicFontFromOSFont(
            Font.GetOSInstalledFontNames().First(f => f.ToLower().Contains("mono")), smallFontSize);
        
        public static GUIStyle monoStyle => new() { richText = true, font = monoFont };
    }
    
    
    [CustomEditor(typeof(SPP_Importer))]
    public class SPP_ImporterEditor: ScriptedImporterEditor
    {
        private static GUIStyle monoStyle;
        
        static bool showData = true;
        static bool showValidRows = false;
        static bool showInvalidRows = false;
        static int maxInvalidLinesShown = 10;
        static int maxCsvLinesShown = 10;

        protected override void Awake()
        {
            base.Awake();
            monoStyle = EditorCustomStyles.monoStyle;
        }

        public override void OnInspectorGUI()
        {
            var importer = (SPP_Importer) serializedObject.targetObject;
            if (importer == null) return;

            SPP_TimelineManager timelineManager = importer.timelineManager;
            SPP_CSV csv = timelineManager?.csv;
            if (csv == null || csv.IsEmpty)
            {
                EditorGUILayout.LabelField($"No CSV {(csv == null ? "created" : "data in file")}. Try to Reimport this.", EditorStyles.boldLabel);
                ApplyRevertGUI();
                return;
            }
            
            EditorGUILayout.Separator();
            
            // SHAPE INFO
            InfoGUI(csv);
            
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            
            serializedObject.ApplyModifiedProperties();
            
            ApplyRevertGUI();
        }

        private void InfoGUI(SPP_CSV csv)
        {
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.LabelField($"{csv.csvLines.Count} Rows of Data", EditorStyles.boldLabel);
            
            EditorGUILayout.Separator();

            if (csv.signals.IsNullOrEmpty())
            {
                if (GUILayout.Button("Load CSV"))
                    csv.ParseAllSignals();
                else
                    return;
            }
            
            // TOGGLES
            {
                showData = EditorGUILayout.BeginToggleGroup("Show Data", showData);

                if (showData)
                {
                    if (!showValidRows && !showInvalidRows) showValidRows = true;
                    showValidRows = EditorGUILayout.Toggle("VALID Signals", showValidRows);
                    showInvalidRows = EditorGUILayout.Toggle("INVALID Signals", showInvalidRows);
                    showData = showValidRows || showInvalidRows;
                }
                
                EditorGUILayout.EndToggleGroup();
            }

            // LIST
            if (showData)
            {
                maxCsvLinesShown = ExpandableList(
                    showValidRows && showInvalidRows 
                        ? csv.allLog.ToArray()
                        : showValidRows 
                            ? csv.validLogs.ToArray()
                            : csv.invalidLogs.ToArray(),
                    maxCsvLinesShown,
                    csv.headerLog
                );
            }
            
            EditorGUILayout.EndVertical();
        }

        private int ExpandableList<T>(T[] list, int numVisible, string header = null)
        {
            EditorGUILayout.BeginHorizontal(new GUIStyle() { alignment = TextAnchor.MiddleLeft },
                GUILayout.ExpandWidth(false), GUILayout.MinWidth(0));
            {
                if (GUILayout.Button("-", GUILayout.MaxWidth(20))) numVisible -= 10;
                if (GUILayout.Button("+", GUILayout.MaxWidth(20))) numVisible += 10;
                EditorGUILayout.LabelField($"{numVisible} / {list.Length}",
                    GUILayout.ExpandWidth(false), GUILayout.MinWidth(0));

                numVisible = Mathf.Clamp(numVisible, 0, list.Length);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            if (header != null)
                EditorGUILayout.LabelField(header, monoStyle);
            
            list.Take(numVisible).ForEach(line => EditorGUILayout.LabelField($"{line}", monoStyle));

            return numVisible;
        }
    }
}
