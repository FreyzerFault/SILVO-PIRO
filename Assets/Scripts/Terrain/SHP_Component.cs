using System;
using DotSpatial.Data;
using UnityEngine;

namespace SILVO.Terrain
{
    [ExecuteAlways] [Serializable]
    public class SHP_Component: MonoBehaviour
    {
        public struct ShapeMetaData
        {
            public Vector2 origin;
            public Vector2 size;

            public string projectionStr;
        }

        public ShapeMetaData metaData;
        
        [SerializeField]
        protected Shape shape;

        public virtual Shape Shape
        {
            get => shape;
            set => shape = value;
        }

        protected virtual void UpdateShape()
        {
            ExtractMetaData();
        }


        #region METADATA

        private void ExtractMetaData()
        {
            metaData.origin = new Vector2((float)shape.Range.Extent.X, (float)shape.Range.Extent.Y);
            metaData.size = new Vector2((float)shape.Range.Extent.Width, (float)shape.Range.Extent.Height);
        }

        #endregion

        #region TEXTURE

        public Vector2Int TexSize
        {
            get { return texSize; }
            set
            {
                texSize = value;
                UpdateTexture();
            }
        }

        public Texture2D texture;
        protected Vector2Int texSize = new(128, 128);
        protected virtual void UpdateTexture() => texture = GetTexture();
        public virtual Texture2D GetTexture() => Texture2D.grayTexture;

        #endregion
        
        
    }
}
