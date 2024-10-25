using System;
using System.Collections.Generic;
using System.Linq;
using DavidUtils.ExtensionMethods;
using DavidUtils.UI.Fonts;
using SILVO.Asset_Importers;
using SILVO.SPP;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using Table = DavidUtils.Editor.DevTools.InspectorUtilities.TableFields;

namespace SILVO.Editor
{
    [CustomEditor(typeof(SPP_Importer))]
    public class SPP_ImporterEditor: ScriptedImporterEditor
    {
        private static bool _showData = true;
        private static int _maxCsvLinesShown = 10;
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

        public override void OnEnable()
        {
            base.OnEnable();
            _importer = (SPP_Importer)serializedObject.targetObject;
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
            
            EditorGUILayout.Separator();
            
            // SHAPE INFO
            InfoGUI(csv);
            
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            
            // serializedObject.ApplyModifiedProperties();
            
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
                {
                    EditorGUILayout.EndVertical();
                    return;
                }
            }
            
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
                if (!_cachedTableContents.ContainsKey(CSV)) 
                    _cachedTableContents.Add(CSV, new Dictionary<TableDisplayOptions, IEnumerable<string>>());
            
                if (!_cachedTableContents[CSV].ContainsKey(_displayOption)) 
                    _cachedTableContents[CSV].Add(
                        _displayOption,
                        _displayOption switch
                        {
                            TableDisplayOptions.AllRows => csv.allLog,
                            TableDisplayOptions.ValidRows => csv.validLogs,
                            TableDisplayOptions.InvalidRows => csv.invalidLogs,
                            _ => new List<string>()
                        });

                var log = _cachedTableContents[CSV][_displayOption];
                    
                Table.ExpandableTable(log, ref _maxCsvLinesShown, ref _scrollPos, csv.headerLog);
            }
            
            EditorGUILayout.EndVertical();
        }

        
        
        private void ClearCache() => _cachedTableContents.Clear();
    }
}
