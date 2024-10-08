using System;
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
        
        private TimelineRenderer renderer;

        public TimelineRenderer Renderer => renderer ??= GetComponent<TimelineRenderer>();
        
        protected virtual void Awake() => renderer = GetComponent<TimelineRenderer>();

        public abstract void UpdateCheckpoints();
    }
}
