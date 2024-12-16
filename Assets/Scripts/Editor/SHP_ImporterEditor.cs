using DavidUtils.Editor.DevTools.CustomFields;
using SILVO.Asset_Importers;
using SILVO.Terrain;
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
            
            TextureGUI(shpfileComp);
            
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
        
        
        private void TextureGUI(Shapefile_Component shpfileComp)
        {
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(80));

            // Show texture preview in Low Res
            Texture2D tex = shpfileComp.texture;
                
            if (tex == null)
            {
                EditorGUILayout.LabelField("No texture. Reimport pls", EditorStyles.boldLabel);
                tex = Texture2D.blackTexture;
            }

            EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetRect(128, 128), tex);

            EditorGUILayout.EndVertical();
        }
        
        private void InfoGUI(Shapefile_Component shpfileComp)
        {
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.LabelField($"{shpfileComp.ProjectionName}"); // Projection
            EditorGUILayout.LabelField($"{shpfileComp.ShapeCount} " + // Nº Shapes
                                       $"{shpfileComp.FeatureType}" + // Feature Type
                                       $"{(shpfileComp.ShapeCount > 1 ? "s" : "")}"); // Plural o Singular
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
                // MyInputFields.InputField(serializedObject, "shpfileComponent.texSize", "Texture Size",
                //     () => ((SHP_Importer)target).shpfileComponent.UpdateTexture());
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
