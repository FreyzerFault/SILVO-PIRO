using System;
using System.Collections.Generic;
using SILVO.Terrain;
using UnityEditor;
using UnityEngine;

namespace SILVO.Editor
{
    [CustomEditor(typeof(TerrainManager))]
    public class TerrainManagerEditor: UnityEditor.Editor
    {
        private const int PREVIEW_TEXTURE_SIZE = 128;
        private static Dictionary<DEM, Texture2D> previewTextureCache = new();
        
        private TerrainManager _manager;
        
        private SerializedProperty _dem;

        private void OnEnable()
        {
            _manager = target as TerrainManager;
            _dem = serializedObject.FindProperty("dem");
        }

        public override void OnInspectorGUI()
        {
            if (_manager == null) return;
            
            EditorGUILayout.LabelField("DEM Info", EditorStyles.boldLabel);

            if (_dem.boxedValue is not DEM dem) return;
            
            string filePath = dem.tiffPath;
            if (string.IsNullOrEmpty(filePath))
            {
                EditorGUILayout.LabelField("No DEM loaded", EditorStyles.centeredGreyMiniLabel);
                return;
            }
            
            DEM_ImporterEditor.DEM_GUI(dem);
            
            // TEXTURE
            if (previewTextureCache.TryGetValue(dem, out Texture2D previewTex))
            {
                Debug.Log("FOUND");
                DEM_ImporterEditor.DEM_GUI(dem, previewTex);
            }
            else
            {
                Debug.Log("NOT FOUND");
                DEM_ImporterEditor.DEM_GUI(dem);
                
                EditorGUILayout.Separator();
                
                EditorGUILayout.LabelField("Loading Preview Texture...", EditorStyles.centeredGreyMiniLabel);
                
                CacheTexture(dem);
            }
            
        }

        private static void CacheTexture(DEM dem) => 
            previewTextureCache[dem] = dem.CreateLowResGreyTexture(PREVIEW_TEXTURE_SIZE);
    }
}
