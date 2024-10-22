using System.Collections.Generic;
using System.Linq;
using DavidUtils.ExtensionMethods;
using DavidUtils.UI.Fonts;
using SILVO.Asset_Importers;
using SILVO.SPP;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace SILVO.Editor
{
    [CustomEditor(typeof(SPP_Importer))]
    public class SPP_ImporterEditor: ScriptedImporterEditor
    {
        private static bool _showData = true;
        private static bool _showValidRows = false;
        private static bool _showInvalidRows = false;
        private static int _maxCsvLinesShown = 10;

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
            
            // serializedObject.ApplyModifiedProperties();
            
            ApplyRevertGUI();
        }

        private void InfoGUI(SPP_CSV csv)
        {
            EditorGUILayout.BeginVertical(  );
            
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
                _showData = EditorGUILayout.BeginToggleGroup("Show Data", _showData);

                if (_showData)
                {
                    if (!_showValidRows && !_showInvalidRows) _showValidRows = true;
                    _showValidRows = EditorGUILayout.Toggle("VALID Signals", _showValidRows);
                    _showInvalidRows = EditorGUILayout.Toggle("INVALID Signals", _showInvalidRows);
                    _showData = _showValidRows || _showInvalidRows;
                }
                
                EditorGUILayout.EndToggleGroup();
            }

            // LIST
            if (_showData)
            {
                _maxCsvLinesShown = ExpandableList(
                    _showValidRows && _showInvalidRows 
                        ? csv.allLog.ToArray()
                        : _showValidRows 
                            ? csv.validLogs.ToArray()
                            : csv.invalidLogs.ToArray(),
                    _maxCsvLinesShown,
                    csv.headerLog
                );
            }
            
            EditorGUILayout.EndVertical();
        }

        private static int ExpandableList<T>(IEnumerable<T> list, int numVisible, string header = null)
        {
            EditorGUILayout.BeginHorizontal(new GUIStyle { alignment = TextAnchor.MiddleLeft },
                GUILayout.ExpandWidth(false), GUILayout.MinWidth(0));

            numVisible = CounterGUI(numVisible, 10, list.Count());
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            GUIContent tableContent = new GUIContent($"{header ?? ""}\n" +
                                                     string.Join("", '-'.ToFilledArray(header?.Length ?? 20)) + "\n" +
                                                     $"{string.Join("\n", list.Take(numVisible))}");
            
            float width = EditorGUIUtility.currentViewWidth - 50;
            float size = MyFonts.SmallMonoStyle.CalcHeight(tableContent, width);

            EditorGUILayout.LabelField(tableContent, MyFonts.SmallMonoStyle, GUILayout.MaxHeight(size));

            return numVisible;
        }

        private static int CounterGUI(int counter, int increment, int max)
        {
            if (GUILayout.Button("-", GUILayout.MaxWidth(20))) counter -= increment;
            if (GUILayout.Button("+", GUILayout.MaxWidth(20))) counter += increment;
            EditorGUILayout.LabelField($"{counter} / {max}",
                GUILayout.ExpandWidth(false), GUILayout.MinWidth(0));

            return Mathf.Clamp(counter, 0, max);
        }
    }
}
