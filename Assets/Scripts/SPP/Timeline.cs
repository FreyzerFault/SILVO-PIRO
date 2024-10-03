using System;
using NetTopologySuite.GeometriesGraph;
using UnityEngine;

namespace SILVO.SPP
{
    [RequireComponent(typeof(TimelineRenderer))]
    public abstract class Timeline: MonoBehaviour
    {
        public Action onCheckpointAdded;
        
        public virtual Vector3[] Checkpoints { get; }
        public int PointCount => Checkpoints.Length;
        public bool IsEmpty => PointCount == 0;
        
        public TimelineRenderer renderer;
        
        protected virtual void Awake() => renderer = GetComponent<TimelineRenderer>();
    }
}
