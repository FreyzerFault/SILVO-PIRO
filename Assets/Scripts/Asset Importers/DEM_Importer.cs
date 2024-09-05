using System;
using System.Linq;
using BitMiracle.LibTiff.Classic;
using TMPro;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace SILVO.Asset_Importers
{
    [ScriptedImporter(1, "tifdem")]
    public class DEM_Importer : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            // Create PREFAB with Sprite
            var obj = new GameObject();
            var imageSR = obj.AddComponent<SpriteRenderer>();
            Sprite sprite = GetDefaultSprite();
            imageSR.sprite = sprite;
            
            
            // Read TIFF file directly to Tiff Object
            string assetPath = ctx.assetPath;
            Debug.Log("Importing DEM at path: " + assetPath);
            
            // OPEN TIFF
            Tiff tiff = Tiff.Open(assetPath, "r");
            if (tiff == null)
            {
                Debug.LogError("Failed to open TIFF file");
            }
            else
            {
                // READ TIFF 
                // TODO: NO TIENE VALORES EN EL ROJO APARENTEMENTE
                int[] raster = new int[tiff.ScanlineSize() * tiff.ScanlineSize()];
                tiff.ReadRGBAImage(10, 10, raster);
                Texture2D tex = new Texture2D(tiff.ScanlineSize(), tiff.ScanlineSize());
                tex.SetPixels(raster.Select(p => new Color(p, 0, 0, 1)).ToArray());
                sprite = Sprite.Create(
                    tex,
                    new Rect(0, 0, 1, 1),
                    new Vector2(0.5f, 0.5f)
                );
                imageSR.sprite = sprite;
                
                // TODO: NO ME DA NADA
                Debug.Log($"First 10 pixels: {String.Join(",", raster.TakeLast(10))}");
            }
            
            // READ TIFF by Bytes STREAM
            // FileStream stream = File.OpenRead(assetPath);
            // byte[] data = new byte[stream.Length];
            // int byteCount = stream.Read(data, 0, (int)stream.Length);
            // Debug.Log($"Read {byteCount} bytes from file {assetPath}");
            // var tiff = Tiff.ClientOpen("in-memory", "r", new MemoryStream(data), new TiffStream());
            // stream.Close();
            
            
            // var material = new Material(Shader.Find("Standard"));
            // material.color = Color.red;

            // Add Main OBJECT and secondary ASSETS
            ctx.AddObjectToAsset("main obj", obj);
            ctx.AddObjectToAsset("sprite", sprite, sprite.texture);
            ctx.SetMainObject(obj);

            // Assets that are not passed into the context as import outputs must be destroyed
            // var tempMesh = new Mesh();
            // DestroyImmediate(tempMesh);
        }
        
        private Sprite GetDefaultSprite()
        {
            Texture2D texture = Texture2D.blackTexture;
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f)
            );
            return sprite;
        }
    }
}
