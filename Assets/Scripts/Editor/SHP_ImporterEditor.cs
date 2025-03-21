using DavidUtils.Editor.DevTools.CustomFields;
using DavidUtils.ExtensionMethods;
using SILVO.GEO_Tools.Asset_Importers;
using SILVO.GEO_Tools.SHP;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace SILVO.Editor
{
    [CustomEditor(typeof(SHP_Importer))]
    public class SHP_ImporterEditor: ScriptedImporterEditor
    {
        public override void OnInspectorGUI()
        {
            var importer = (SHP_Importer) target;
            if (importer == null) return;

            Shapefile_Component shpfileComp = importer.shpfileComponent;
            
            if (shpfileComp == null)
            {
                EditorGUILayout.LabelField("No SHP file imported. Try to Reimport this.", EditorStyles.boldLabel);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            
            TextureGUI(shpfileComp.previewTexture);
            
            EditorGUILayout.Separator();
            
            // SHAPE INFO
            InfoGUI(shpfileComp);
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            
            // SETTINGS
            SettingsGUI(shpfileComp);

            serializedObject.ApplyModifiedProperties();
            
            ApplyRevertGUI();
        }
        
        
        private void TextureGUI(Texture2D tex)
        {
            float maxSize = 80;
            Vector2 aspectRatio = new Vector2(tex.width, tex.height).AspectRatioUnder1();
            float width = maxSize * aspectRatio.x;
            float height = maxSize * aspectRatio.y;
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(width), GUILayout.MaxHeight(height));
                
            if (tex == null)
            {
                EditorGUILayout.LabelField("No texture. Reimport pls", EditorStyles.boldLabel);
                tex = Texture2D.redTexture;
            }

            EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetRect(width, height), tex);

            EditorGUILayout.EndVertical();
        }
        
        private void InfoGUI(Shapefile_Component shpfileComp)
        {
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.LabelField($"{shpfileComp.ProjectionName}"); // Projection
            EditorGUILayout.LabelField($"{shpfileComp.ShapeCount} " + // Nº Shapes
                                       $"{shpfileComp.FeatureType}" + // Feature Type
                                       $"{(shpfileComp.ShapeCount > 1 ? "s" : "")}", EditorStyles.boldLabel); // Plural o Singular
            EditorGUILayout.LabelField($"AABB:", EditorStyles.boldLabel); // Min Max
            EditorGUILayout.LabelField($"Min: {shpfileComp.Min}");
            EditorGUILayout.LabelField($"Max: {shpfileComp.Max}");
            EditorGUILayout.LabelField($"Width: {shpfileComp.Max.x - shpfileComp.Min.x}"); // Width, Height
            EditorGUILayout.LabelField($"Height: {shpfileComp.Max.y - shpfileComp.Min.y}"); // Width, Height

            EditorGUILayout.EndVertical();
        }
        
        private void SettingsGUI(Shapefile_Component shpfileComp)
        {
            EditorGUILayout.BeginVertical();
            
            // TEXTURE SIZE
            {
                SerializedProperty resProp = serializedObject.FindProperty("previewTextureResolution");
                if (resProp != null)
                    resProp.intValue = EditorGUILayout.IntField("Preview Resolution:", resProp.intValue);
            }
            
            // MAX SUB POLYGON COUNT
            {
                SerializedProperty maxSubpolygonProp = serializedObject.FindProperty("maxSubPolygonCount");
                if (maxSubpolygonProp != null)
                    maxSubpolygonProp.intValue =
                        EditorGUILayout.IntField("Max SubPolygons:", maxSubpolygonProp.intValue);
            }

            // TERRAIN OVERLAPING
            {
                SerializedProperty onTerrainProp = serializedObject.FindProperty("onTerrain");
                if (onTerrainProp != null)
                {
                    onTerrainProp.boolValue = EditorGUILayout.Toggle("Render On Terrain", onTerrainProp.boolValue);

                    if (onTerrainProp.boolValue)
                    {
                        SerializedProperty terrainOffsetProp = serializedObject.FindProperty("terrainOffset");
                        if (terrainOffsetProp != null)
                            terrainOffsetProp.intValue =
                                EditorGUILayout.IntField("Height Offset", terrainOffsetProp.intValue);
                    }
                }
            }
            
            EditorGUILayout.EndVertical();
        }
    }
}
