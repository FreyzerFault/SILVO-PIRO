using SILVO.Asset_Importers;
using SILVO.Misc_Utils;
using SILVO.Terrain;
using UnityEditor;
using UnityEngine;
using UnityEditor.AssetImporters;

namespace SILVO.Editor
{
    [CustomEditor(typeof(DEM_Importer))]
    public class DEM_ImporterEditor : ScriptedImporterEditor
    {
        protected override bool needsApplyRevert => false;

        public override void OnInspectorGUI()
        {
            DEM_Importer importer = (DEM_Importer)target;
            DEM dem = importer.dem;
            if (dem == null || dem.IsEmpty)
            {
                EditorGUILayout.LabelField("No DEM data. Reimport pls", EditorStyles.boldLabel);
                return;
            }
            TiffReader.TiffMetaData metaData = dem.metaData;
            
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(80));

                // Show texture preview in Low Res
                Texture2D tex = importer.texture;
                
                if (tex == null)
                {
                    EditorGUILayout.LabelField("No texture. Reimport pls", EditorStyles.boldLabel);
                    tex = Texture2D.blackTexture;
                }
                
                EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetRect(80, 80), tex);

                EditorGUILayout.EndVertical();
            }
            {
                EditorGUILayout.BeginVertical();

                EditorGUILayout.LabelField($"{metaData.width} x {metaData.height}");
                EditorGUILayout.LabelField($"{metaData.format} {metaData.bitsPerSample} bits");
                EditorGUILayout.LabelField($"Height: {importer.minHeight:F1} - {importer.maxHeight:F1} m");

                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Separator();

            {
                EditorGUILayout.BeginHorizontal();

                SerializedProperty sampleDistProp = serializedObject.FindProperty("sampleDist");
                if (sampleDistProp != null)
                {

                    GUILayout.Label("Distance sample <-> sample:", EditorStyles.boldLabel,
                        GUILayout.ExpandWidth(false));
                    sampleDistProp.floatValue = EditorGUILayout.FloatField(sampleDistProp.floatValue);
                    serializedObject.ApplyModifiedProperties();
                }

                EditorGUILayout.EndHorizontal();
            }
            
            AddLabeledValue("Terrain Real Size", importer.dem.TerrainSizeBySampleDistance(importer.sampleDist).ToString("F1"));
            
            EditorGUILayout.Separator();
            
            // Apply Map to Active Terrain
            if (dem.heightData.Length > 0 && GUILayout.Button("Apply to Terrain")) 
                dem.ApplyToActiveTerrain(importer.sampleDist);
        }

        private static void AddLabeledValue(string label, string value) => 
            EditorGUILayout.LabelField(new GUIContent(label), new GUIContent(value));
        
        private static void AddFloatField(string label, ref float value) => 
            value = EditorGUILayout.FloatField(new GUIContent(label), value);
    }
}
