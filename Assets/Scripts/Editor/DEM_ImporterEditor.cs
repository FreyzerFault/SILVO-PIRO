using System.IO;
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
            
            DEM_GUI(dem, importer.texture);
            
            EditorGUILayout.Separator();
            
            // Apply Map to Active Terrain
            if (!dem.IsEmpty && GUILayout.Button("Apply to Terrain")) 
                importer.ApplyDEM();
        }

        private static void AddLabeledValue(string label, string value) => 
            EditorGUILayout.LabelField(new GUIContent(label), new GUIContent(value));
        
        private static void AddFloatField(string label, ref float value) => 
            value = EditorGUILayout.FloatField(new GUIContent(label), value);


        private static void DEM_TextureGUI(Texture2D tex)
        {
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(80));
            
            if (tex == null)
            {
                EditorGUILayout.LabelField("No texture. Reimport pls", EditorStyles.boldLabel);
                tex = Texture2D.blackTexture;
            }
                
            EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetRect(80, 80), tex);

            EditorGUILayout.EndVertical();
        }

        private static void DEM_HeightMapDataInfo(DEM dem)
        {
            TiffReader.TiffMetaData metaData = dem.metaData;
            
            EditorGUILayout.BeginVertical();


            EditorGUILayout.LabelField($"{Path.GetFileNameWithoutExtension(dem.tiffPath)}", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField($"{metaData.width} x {metaData.height}");
            EditorGUILayout.LabelField($"{metaData.format} {metaData.bitsPerSample} bits");
            EditorGUILayout.LabelField($"Height: {dem.minHeight:F1} - {dem.maxHeight:F1} m");

            EditorGUI.indentLevel--;
            
            EditorGUILayout.EndVertical();
        }
        
        private static void DEM_GeoreferenceDataGUI(DEM dem)
        {
            TiffReader.TiffMetaData metaData = dem.metaData;
            
            EditorGUILayout.LabelField("Geographical Data", EditorStyles.boldLabel);
            
            EditorGUI.indentLevel++;

            AddLabeledValue("Projection", metaData.projectionStr);
            AddLabeledValue("Origin Coordinates", new Vector2Int(Mathf.FloorToInt(metaData.originWorld.x), Mathf.FloorToInt(metaData.originWorld.y)).ToString());
            AddLabeledValue("Sample Scale", $"{metaData.sampleScale.x} x {metaData.sampleScale.y} m");

            Vector2 worldSize = metaData.WorldSize;
            AddLabeledValue("World Size", $"{worldSize.x} x {worldSize.y}");

            EditorGUI.indentLevel--;
        }
        
        /// <summary>
        /// DEM Info GUI
        ///     Shows DEM Metadata, Map Size, Height dimensions, Georeference Data...
        /// </summary>
        /// <param name="dem">DEM</param>
        /// <param name="previewTexture">OPTIONAL pregenerated TEXTURE</param>
        public static void DEM_GUI(DEM dem, Texture2D previewTexture = null)
        {
            if (previewTexture == null)
            {
                DEM_HeightMapDataInfo(dem);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
            
                // Show texture preview in Low Res
                DEM_TextureGUI(previewTexture);
            
                DEM_HeightMapDataInfo(dem);
            
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Separator();
            DEM_GeoreferenceDataGUI(dem);
        }
    }
}
