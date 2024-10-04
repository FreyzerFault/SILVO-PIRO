using System.Linq;
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
        bool invalidFoldout = true;
        int maxInvalidLinesShown = 10;

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
                EditorGUILayout.LabelField($"No CSV {(csv.IsEmpty ? "data in file" : "created")}. Try to Reimport this.", EditorStyles.boldLabel);
                return;
            }

            
            EditorGUILayout.Separator();
            
            // SHAPE INFO
            InfoGUI(csv);
            
            
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            
            // SETTINGS
            SettingsGUI();

            serializedObject.ApplyModifiedProperties();
            
            ApplyRevertGUI();
        }

        private void InfoGUI(SPP_CSV csv)
        {
            var invalidLines = csv.invalidLogs;
            var invalidCount = invalidLines.Length;
            
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.LabelField($"{csv.signals.Length} Valid Signals", EditorStyles.boldLabel);
            
            EditorGUILayout.Separator();
            
            
            invalidFoldout = EditorGUILayout.Foldout(invalidFoldout, $"{invalidCount} Invalid Rows:", true);
            
            
            if (invalidFoldout)
            {
                EditorGUILayout.BeginHorizontal(new GUIStyle() {alignment = TextAnchor.MiddleLeft}, GUILayout.ExpandWidth(false), GUILayout.MinWidth(0));
                {
                    if (GUILayout.Button("-", GUILayout.MaxWidth(20))) maxInvalidLinesShown -= 10;
                    if (GUILayout.Button("+", GUILayout.MaxWidth(20))) maxInvalidLinesShown += 10;
                    EditorGUILayout.LabelField($"{maxInvalidLinesShown} / {invalidCount}", GUILayout.ExpandWidth(false), GUILayout.MinWidth(0));
                    
                    maxInvalidLinesShown = Mathf.Clamp(maxInvalidLinesShown, 0, invalidCount);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Separator();
                
                csv.invalidLogs.Take(maxInvalidLinesShown).ForEach(line => EditorGUILayout.LabelField($"{line}", monoStyle));
            }

            EditorGUILayout.EndVertical();
        }

        private void SettingsGUI()
        {
            
        }
    }
}
