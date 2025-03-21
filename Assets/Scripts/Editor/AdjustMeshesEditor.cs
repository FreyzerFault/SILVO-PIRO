using UnityEditor;
using UnityEngine;

namespace SILVO.Editor
{
    [CustomEditor(typeof(AdjustMeshes))]
    public class AdjustMeshesEditor: UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if (GUILayout.Button("Adjust Meshes")) 
                ((AdjustMeshes) target).AdjustMeshesToOriginUpwards();
        }
    }
}
