using System;
using System.Linq;
using UnityEngine;

namespace SILVO.SPP
{
    [RequireComponent(typeof(TimelineRenderer))]
    public abstract class Timeline: MonoBehaviour
    {
        private float progress;

        public float Progress
        {
            get => progress;
            set
            {
                progress = value;
                onProgressChanged?.Invoke();
            }
        }
        
        public Action onProgressChanged;
        
        
        protected virtual void Awake() => renderer = GetComponent<TimelineRenderer>();
        
        
        #region CHECKPOINTS
        
        public virtual Vector3[] Checkpoints { get; }
        public int PointCount => Checkpoints.Length;
        public bool IsEmpty => PointCount == 0;
        
        public abstract void UpdateCheckpoints();
        
        public Action onCheckpointAdded;
        
        public bool OnCheckpoint => PointCount * Progress % 1f == 0f;
        public bool OnStart => Mathf.Approximately(progress, 0f);
        public bool OnEnd => Mathf.Approximately(progress, 1f);
        
        public int PrevCheckpointIndex => OnCheckpoint
            ? CurrentCheckpointIndex - (OnStart ? 0 : 1)
            : Mathf.FloorToInt(PointCount* Progress);
        
        public int CurrentCheckpointIndex => Mathf.RoundToInt(PointCount * Progress);
        
        public int NextCheckpointIndex => OnCheckpoint
            ? CurrentCheckpointIndex + (OnEnd ? 0 : 1)
            : Mathf.CeilToInt(PointCount * Progress);

        public Vector3 PrevCheckpoint => Checkpoints[PrevCheckpointIndex];
        public Vector3 CurrentCheckpoint => Checkpoints[CurrentCheckpointIndex];
        public Vector3 NextCheckpoint => Checkpoints[NextCheckpointIndex];
        
        public Vector3[] CheckpointsCompleted() => 
            IsEmpty
                ? Array.Empty<Vector3>()
                : Checkpoints[..Mathf.FloorToInt(PointCount * Progress)];
        
        public Vector3[] CheckpointsRemaining() => 
            IsEmpty
                ? Array.Empty<Vector3>()
                : Checkpoints[Mathf.FloorToInt(PointCount * Progress)..];


        public Vector3 CurrentPosition => Vector3.Lerp(
            PrevCheckpoint,
            NextCheckpoint,
            Mathf.InverseLerp(
                (float)PrevCheckpointIndex / PointCount,
                (float)NextCheckpointIndex / PointCount,
                progress)
        );
        
        #endregion


        #region MOVE TO CHECKPOINT

        public bool GoTo(int checkpointIndex)
        {
            if (checkpointIndex < 0 || checkpointIndex >= PointCount) return false;
            Progress = checkpointIndex / (PointCount - 1f);
            return true;
        }
        
        public bool GoTo(Vector3 checkpoint) => 
            Checkpoints.Contains(checkpoint)
            && GoTo(Array.IndexOf(Checkpoints, checkpoint));
        
        public bool GoToPrevCheckpoint() => GoTo(PrevCheckpointIndex);
        public bool GoToNextCheckpoint() => GoTo(NextCheckpointIndex);
        public bool GoToNearestCheckpoint() => GoTo(CurrentCheckpointIndex);

        #endregion

        
        #region RENDERING
        
        private TimelineRenderer renderer;

        public TimelineRenderer Renderer => renderer ??= GetComponent<TimelineRenderer>();
        
        public bool RendererVisibility
        {
            get => Renderer.enabled;
            set => Renderer.enabled = value;
        }
        
        #endregion
    }
}
