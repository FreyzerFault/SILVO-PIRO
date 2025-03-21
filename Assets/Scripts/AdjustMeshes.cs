using System.Linq;
using DavidUtils.ExtensionMethods;
using DavidUtils.Geometry.MeshExtensions;
using UnityEngine;

namespace SILVO
{
    public class AdjustMeshes : MonoBehaviour
    {
        /// <summary>
        ///     Coloca una mesh Boca Arriba y en el origen
        /// </summary>
        public void AdjustMeshesToOriginUpwards()
        {
            var meshFilters = GetComponentsInChildren<MeshFilter>();
            var meshes = meshFilters.Select(mf => mf.mesh).ToArray();
            
            // Rotar antes de mover para ponerlo boca arriba
            meshes.ForEach(m => m.Rotate(Quaternion.Euler(-90,180,0)));
            meshes.TranslateToOrigin();

            for (var i = 0; i < meshFilters.Length; i++)
                meshFilters[i].mesh = meshes[i];
        }
    }
}
