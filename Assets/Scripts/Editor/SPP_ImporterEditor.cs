using System;
using System.Collections.Generic;
using System.Linq;
using DavidUtils.Editor.DevTools.CustomFields;
using DavidUtils.ExtensionMethods;
using SILVO.GEO_Tools.Asset_Importers;
using SILVO.GEO_Tools.SPP;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using Table = DavidUtils.Editor.DevTools.Table.TableFields;

namespace SILVO.Editor
{
    [CustomEditor(typeof(SPP_Importer))]
    public class SPP_ImporterEditor: ScriptedImporterEditor
    {
        private static int _maxCsvLinesInTable = 10;
        private static Vector2 _scrollPos = Vector2.zero;
        
        private enum TableDisplayOptions
        {
            ValidRows,
            InvalidRows,
            AllRows,
            Hide
        }

        private static TableDisplayOptions _displayOption = TableDisplayOptions.ValidRows;
        
        // CACHE TABLE CONTENTS by CSV
        private static Dictionary<SPP_CSV, Dictionary<TableDisplayOptions, IEnumerable<string>>> _cachedTableContents = new();
        
        private SPP_Importer _importer;
        private SPP_CSV CSV => _importer.timelineManager.csv;
        
        private SerializedProperty _maxCsvLines;
        private SerializedProperty _freeMemoryWhenParsed;

        public override void OnEnable()
        {
            base.OnEnable();
            _importer = (SPP_Importer)serializedObject.targetObject;
            _maxCsvLines = serializedObject.FindProperty("maxLines");
            _freeMemoryWhenParsed = serializedObject.FindProperty("freeMemoryWhenParsed");
        }

        public override void OnInspectorGUI()
        {
            if (_importer == null) return;

            SPP_TimelineManager timelineManager = _importer.timelineManager;
            SPP_CSV csv = timelineManager?.csv;
            if (csv == null || csv.IsEmpty)
            {
                EditorGUILayout.LabelField($"No CSV {(csv == null ? "created" : "data in file")}. Try to Reimport this.", EditorStyles.boldLabel);
                ApplyRevertGUI();
                return;
            }
            
            // SETTINGS
            MyInputFields.InputField(_maxCsvLines, "Max CSV Lines Parsed");
            MyInputFields.InputField(_freeMemoryWhenParsed, "Free CSV Data after Parse");
            serializedObject.ApplyModifiedProperties();
            ApplyRevertGUI();
            
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            
            if (csv.signals.IsNullOrEmpty())
            {
                if (GUILayout.Button("Parse CSV"))
                {
                    EditorGUILayout.Separator();
                    csv.ParseAllSignals(_maxCsvLines.intValue);
                    ClearCache();
                }
                else return;
            }
            
            EditorGUILayout.Separator();
            
            if (!_freeMemoryWhenParsed.boolValue && csv.LineCount != 0)
                InfoCSVGUI(csv, _maxCsvLines.intValue);
            
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
        }

        private static void InfoCSVGUI(SPP_CSV csv, int maxCsvLines = 1000)
        {
            EditorGUILayout.LabelField($"{csv.csvLines.Length} Rows of Data", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical();
            
            // TOGGLES
            {
                bool showValid = EditorGUILayout.ToggleLeft("VALID Signals", _displayOption is TableDisplayOptions.ValidRows or TableDisplayOptions.AllRows);
                bool showInvalid = EditorGUILayout.ToggleLeft("INVALID Signals", _displayOption is TableDisplayOptions.InvalidRows or TableDisplayOptions.AllRows);
                
                _displayOption = showValid && showInvalid 
                    ? TableDisplayOptions.AllRows
                    : showValid 
                        ? TableDisplayOptions.ValidRows
                        : showInvalid 
                            ? TableDisplayOptions.InvalidRows
                            : TableDisplayOptions.Hide;
            }

            // LIST
            if (_displayOption != TableDisplayOptions.Hide)
            {
                if (!_cachedTableContents.ContainsKey(csv)) 
                    _cachedTableContents.Add(csv, new Dictionary<TableDisplayOptions, IEnumerable<string>>());
            
                if (!_cachedTableContents[csv].ContainsKey(_displayOption)) 
                    _cachedTableContents[csv].Add(_displayOption, GetTableLines(csv, _displayOption, maxCsvLines).Select(l => l.Colored("white")));
                
                var log = _cachedTableContents[csv][_displayOption];
                    
                Table.ExpandableTable(log, ref _maxCsvLinesInTable, ref _scrollPos, csv.HeaderLineColored);
            }
            
            EditorGUILayout.EndVertical();
        }

        private static string[] GetTableLines(SPP_CSV csv, TableDisplayOptions displayOption, int numLines = -1)
        {
            if (numLines == -1) numLines = csv.LineCount;
            return displayOption switch
            {
                TableDisplayOptions.AllRows => csv.GetTable(numLines),
                TableDisplayOptions.ValidRows => csv.GetValidTable(numLines),
                TableDisplayOptions.InvalidRows => csv.GetInvalidTable(numLines),
                _ => Array.Empty<string>()
            };
        }


        private void ClearCache() => _cachedTableContents.Clear();
    }
}
