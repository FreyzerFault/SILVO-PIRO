using SILVO.Asset_Importers;
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
                
                EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetRect(128, 128), tex);

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Separator();
            {
                EditorGUILayout.BeginVertical();

                EditorGUILayout.LabelField($"{importer.polygon.VertexCount} vertices");

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Separator();
            
            SerializedProperty maxSubpolygonProp = serializedObject.FindProperty("maxSubPolygonCount");
            SerializedProperty texSizeProp = serializedObject.FindProperty("texSize");
            
            if (maxSubpolygonProp != null) 
                maxSubpolygonProp.intValue = EditorGUILayout.IntField("Max SubPolygons:", maxSubpolygonProp.intValue);
            if (texSizeProp != null)
            {
                int texSize = EditorGUILayout.IntField("Texture Size:", (int)texSizeProp.vector2Value.x);
                texSizeProp.vector2Value = new Vector2(texSize, texSize);
            }
            
            serializedObject.ApplyModifiedProperties();
            
            ApplyRevertGUI();
        }
    }
}
