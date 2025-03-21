using SILVO.GEO_Tools.DEM;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.UI;

namespace SILVO.GEO_Tools.Asset_Importers
{
    [ScriptedImporter(1, "geotif")]
    public class DEM_Importer : ScriptedImporter
    {
        [SerializeField] public DEM.DEM dem;
        [SerializeField] public Texture2D texture;
        
        public float sampleDist = 1;
        
        public float maxHeight = 0;
        public float minHeight = 0;
        
        public void ApplyDEM() => TerrainManager.Instance.DEM = dem;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            // Create PREFAB with Sprite
            GameObject obj = new GameObject();
            Image image = obj.AddComponent<Image>();
            texture = Texture2D.grayTexture;
            image.sprite = CreateSprite(texture);
            
            // TIFF -> float[]
            dem = new DEM.DEM(ctx.assetPath);
            
            if (dem == null || dem.IsEmpty)
            {
                Debug.LogError($"Failed to open TIFF file in {assetPath}");
                ctx.AddObjectToAsset("main obj", obj);
            }
            else
            {
                // Data to Texture
                texture = dem.CreateLowResGreyTexture(256);
                image.sprite = CreateSprite(texture);
                
                // Add Main OBJECT and secondary ASSETS
                image.rectTransform.rect.Set(0, 0, texture.width, texture.height);
                ctx.AddObjectToAsset("main obj", obj);
                ctx.AddObjectToAsset("texture", texture, texture);
                ctx.AddObjectToAsset("sprite", image.sprite, texture);
                // ctx.AddObjectToAsset("dem", dem, texture);
                ctx.SetMainObject(obj);
                
                Debug.Log($"<color=lime>IMPORTED {dem}</color>");
            }
        }
        
        private static Sprite CreateSprite(Texture2D texture) => 
            Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

    }
}
