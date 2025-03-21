using System.Collections.Generic;
using SILVO.GEO_Tools.DEM;
using UnityEditor;
using UnityEngine;

namespace SILVO.Editor
{
    [CustomEditor(typeof(TerrainManager))]
    public class TerrainManagerEditor: UnityEditor.Editor
    {
        private const int PREVIEW_TEXTURE_SIZE = 128;
        private static readonly Dictionary<DEM, Texture2D> PreviewTextureCache = new();
        
        private TerrainManager _manager;
        
        private DEM _dem;

        private void OnEnable()
        {
            _manager = target as TerrainManager;
            _dem = _manager.DEM;
        }

        public override void OnInspectorGUI()
        {
            if (_manager == null) return;
            
            EditorGUILayout.LabelField("DEM Info", EditorStyles.boldLabel);
            
            string filePath = _dem.tiffPath;
            if (string.IsNullOrEmpty(filePath))
            {
                EditorGUILayout.LabelField("No DEM loaded", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            EditorGUI.indentLevel++;
            {
                if (PreviewTextureCache.TryGetValue(_dem, out Texture2D previewTex))
                {
                    DEM_ImporterEditor.DEM_GUI(_dem, previewTex);
                }
                else
                {
                    DEM_ImporterEditor.DEM_GUI(_dem);

                    EditorGUILayout.Separator();

                    EditorGUILayout.LabelField("Loading Preview Texture...", EditorStyles.centeredGreyMiniLabel);

                    CacheTexture(_dem);
                }
            }
            EditorGUI.indentLevel--;
        }

        private static void CacheTexture(DEM dem) => 
            PreviewTextureCache[dem] = dem.CreateLowResGreyTexture(PREVIEW_TEXTURE_SIZE);
    }
}
