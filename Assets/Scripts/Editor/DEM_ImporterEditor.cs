using SILVO.Asset_Importers;
using UnityEditor;
using UnityEngine;
using UnityEditor.AssetImporters;

namespace SILVO.Editor
{
    [CustomEditor(typeof(DEM_Importer))]
    public class DEM_ImporterEditor : ScriptedImporterEditor
    {
        private static Vector2Int _terrainSize = new(0, 0);
        private static Vector2Int _heightRange = new(0, 0);

        protected override void Awake()
        {
            base.Awake();
            if (_terrainSize.x != 0) return;
            UnityEngine.Terrain terrain = UnityEngine.Terrain.activeTerrain;
            _terrainSize = new Vector2Int((int)terrain.terrainData.size.x, (int)terrain.terrainData.size.z);
            _heightRange = new Vector2Int((int)terrain.transform.position.y, (int)(terrain.transform.position.y +
                terrain.terrainData.size.y));
        }

        public override void OnInspectorGUI()
        {
            Vector2Int size = serializedObject.FindProperty("mapSize").vector2IntValue;
            AddLabel("Map Size", $"{size.x} x {size.y}");
            
            int bits = serializedObject.FindProperty("bitsPerSample").intValue;
            string format = serializedObject.FindProperty("format").stringValue;
            AddLabel("Data Format", $"{format} {bits} bits");
            
            float maxHeight = serializedObject.FindProperty("maxHeight").floatValue;
            float minHeight = serializedObject.FindProperty("minHeight").floatValue;
            AddLabel("Height Range", $"{minHeight} - {maxHeight}");
            
            EditorGUILayout.Separator();

            _terrainSize = EditorGUILayout.Vector2IntField(new GUIContent("Terrain Extension"), _terrainSize);
            _heightRange = EditorGUILayout.Vector2IntField(new GUIContent("Height Range"), _heightRange);
            
            // Apply Map to Active Terrain
            if (GUILayout.Button("Apply to Terrain"))
            {
                DEM_Importer importer = (DEM_Importer) target;
                importer.ApplyToTerrain(_terrainSize, _heightRange);
            }
            
            ApplyRevertGUI();
        }
        
        private static void AddLabel(string label, string value) => 
            EditorGUILayout.LabelField(new GUIContent(label), new GUIContent(value));
    }
}
