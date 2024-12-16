using System;
using SILVO.Terrain;
using UnityEditor;
using DavidUtils.Editor.DevTools.CustomFields;
using UnityEngine;

namespace SILVO.Editor
{
    [CustomEditor(typeof(Shapefile_Component))]
    public class Shapefile_ComponentEditor: UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var shapefile = (Shapefile_Component)target;
            MyInputFields.InputField(serializedObject, "maxSubPolygonCount", "Max Sub Polygon Count");

            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Metadata", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            {
                ShapefileMetaData metadata = shapefile.metaData;
                EditorGUILayout.LabelField("Shapes", metadata.shapeCount.ToString());
                EditorGUILayout.LabelField("Feature of Type", metadata.featureType.ToString());
                EditorGUILayout.LabelField("Projection", metadata.projectionName);
                EditorGUILayout.LabelField("Extension", $"{metadata.min.ToString("F0")} - {metadata.max.ToString("F0")}");
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            
            MyInputFields.InputField(serializedObject, "overTerrain", "Place over Terrain", () => shapefile.UpdateOverTerrain());
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Reload Shape")) shapefile.LoadShapeFile();
        }
    }
}
