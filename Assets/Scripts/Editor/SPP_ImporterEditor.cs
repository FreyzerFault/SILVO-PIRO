using System.Linq;
using Csv;
using DavidUtils.ExtensionMethods;
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
        private GUIStyle monoStyle;
        bool showInvalidRows = false;
        int maxInvalidLinesShown = 10;
        int maxCsvLinesShown = 10;

        protected override void Awake()
        {
            base.Awake();
            int standardFontSize = Mathf.RoundToInt(EditorStyles.label.fontSize * 0.8f);
            Font font = Font.CreateDynamicFontFromOSFont(
                Font.GetOSInstalledFontNames().First(f => f.ToLower().Contains("mono")), standardFontSize); 
            monoStyle = new GUIStyle() { richText = true, font = font};
            
        }

        public override void OnInspectorGUI()
        {
            SPP_Importer importer = (SPP_Importer) serializedObject.targetObject;
            if (importer == null) return;

            SPP_CSV csv = importer.csv;
            if (csv == null || csv.IsEmpty)
            {
                EditorGUILayout.LabelField($"No CSV {(csv!.IsEmpty ? "data in file" : "created")}. Try to Reimport this.", EditorStyles.boldLabel);
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

            maxCsvLinesShown = ExpandableList(csv.csvLines.ToArray(), maxCsvLinesShown);
            
            if (!showInvalidRows)
                showInvalidRows = GUILayout.Button("Analise for Invalid Rows");
            else
            {
                csv.ParseSignals();
                
                var invalidLines = csv.invalidLogs;
                var invalidCount = invalidLines.Count;

                EditorGUILayout.LabelField($"{csv.signals.Count} Valid Signals", EditorStyles.boldLabel);

                EditorGUILayout.Separator();
                
                EditorGUILayout.LabelField($"{invalidCount} Invalid Rows:");

                maxInvalidLinesShown = ExpandableList<string>(invalidLines.ToArray(), maxInvalidLinesShown);
            }

            EditorGUILayout.EndVertical();
        }

        private int ExpandableList<T>(T[] list, int numVisible)
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

            list.Take(numVisible).ForEach(line => EditorGUILayout.LabelField($"{line}", monoStyle));

            return numVisible;
        }
    }
}
