using System.Collections;
using DavidUtils.Geometry;
using DavidUtils.Rendering;
using UnityEngine;

namespace SILVO.Tests
{
    [RequireComponent(typeof(PolygonRenderer))]
    public class PolygonSegmentationTest : MonoBehaviour
    {
        private PolygonRenderer polyRenderer;

        private int MaxSubPolygons
        {
            get => polyRenderer.maxSubPolygons;
            set => polyRenderer.maxSubPolygons = value;
        }
        
        private Polygon[] subPolygons => polyRenderer.subPolygons;
        private int subPolygonCount => polyRenderer.SubPolygonCount;
        
        private int lastSubPolygonCount = 0;
        
        private void Awake()
        {
            polyRenderer = GetComponent<PolygonRenderer>();
        }

        private void Start()
        {
            MaxSubPolygons = 1;
            polyRenderer.UpdatePolygon();
        }
        
        public void StartSubPolygonsIncreaser()
        {
            StartCoroutine(SubPolygonsIncreaser());
        }

        private IEnumerator SubPolygonsIncreaser()
        {
            while (lastSubPolygonCount != subPolygonCount)
            {
                lastSubPolygonCount = subPolygonCount;
                IncreaseSubPolygons();
                
                yield return new WaitForSeconds(0.1f);
            }

            yield return null;
        }
        
        public void IncreaseSubPolygons()
        {
            MaxSubPolygons++;
            polyRenderer.UpdatePolygon();
        }
    }
}
