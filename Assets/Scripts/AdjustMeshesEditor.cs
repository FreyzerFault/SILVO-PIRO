using UnityEditor;
using UnityEngine;

namespace SILVO
{
    [CustomEditor(typeof(AdjustMeshes))]
    public class AdjustMeshesEditor: Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if (GUILayout.Button("Adjust Meshes")) 
                ((AdjustMeshes) target).AdjustMeshesToOrigin();
        }
    }
}
