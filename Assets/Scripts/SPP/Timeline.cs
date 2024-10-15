using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SILVO.SPP
{
    public abstract class Timeline: MonoBehaviour
    {
        // CHECKPOINTS
        [SerializeField] protected List<Vector3> checkpoints;
        public virtual List<Vector3> Checkpoints
        {
            get => checkpoints;
            set
            {
                checkpoints = value;
                onCheckpointsUpdated?.Invoke();
            }
        }
        public int PointCount => Checkpoints.Count;
        public bool IsEmpty => PointCount == 0;

        public Action<Vector3> onCheckpointAdded;
        public Action<int, Vector3> onCheckpointInserted;
        public Action<int, Vector3> onCheckpointsDeleted;
        public Action<int, Vector3> onCheckpointsMoved;
        public Action onCheckpointsUpdated;
        
        // PROGRESS
        private float progress;
        public float Progress
        {
            get => progress;
            set
            {
                progress = value;
                onProgressChanged?.Invoke(value);
            }
        }
        
        public Action<float> onProgressChanged;
        
        
        protected virtual void OnEnable()
        {
            renderer = GetComponent<TimelineRenderer>() ?? gameObject.AddComponent<TimelineRenderer>();
            renderer.Timeline = this;
        }


        #region CHECKPOINTS
        
        public void AddCheckpoint(Vector3 position)
        {
            checkpoints.Add(position);
            onCheckpointAdded?.Invoke(position);
        }
        
        public void InsertCheckpoint(int index, Vector3 position)
        {
            if (index == -1) AddCheckpoint(position);
            if (index < 0 || index >= PointCount) return;
            checkpoints.Insert(index, position);
            onCheckpointInserted?.Invoke(index, position);
        }

        public void RemoveCheckpoint(int index = -1)
        {
            if (index == -1) RemoveCheckpoint(Checkpoints.Count - 1);
            if (index < 0 || index >= PointCount) return;
            Vector3 pointRemoved = checkpoints[index];
            checkpoints.RemoveAt(index);
            onCheckpointsDeleted?.Invoke(index, pointRemoved);
        }
        
        public void MoveCheckpoint(Vector3 position, int index = -1)
        {
            if (index == -1) MoveCheckpoint(position, Checkpoints.Count - 1);
            if (index < 0 || index >= PointCount) return;
            checkpoints[index] = position;
            onCheckpointsMoved?.Invoke(index, position);
        }
        
        #endregion

        
        #region CHECKPOINT PROGRESS

        public bool IsOnCheckpoint => PointCount * Progress % 1f == 0f;
        public bool IsOnStart => Mathf.Approximately(progress, 0f);
        public bool IsOnEnd => Mathf.Approximately(progress, 1f);
        
        public int PrevCheckpointIndex => IsOnCheckpoint
            ? CurrentCheckpointIndex - (IsOnStart ? 0 : 1)
            : Mathf.FloorToInt(PointCount* Progress);
        
        public int CurrentCheckpointIndex => Mathf.RoundToInt(PointCount * Progress);
        
        public int NextCheckpointIndex => IsOnCheckpoint
            ? CurrentCheckpointIndex + (IsOnEnd ? 0 : 1)
            : Mathf.CeilToInt(PointCount * Progress);

        public Vector3 PrevCheckpoint => Checkpoints[PrevCheckpointIndex];
        public Vector3 CurrentCheckpoint => Checkpoints[CurrentCheckpointIndex];
        public Vector3 NextCheckpoint => Checkpoints[NextCheckpointIndex];
        
        public Vector3[] CheckpointsCompleted() => 
            IsEmpty
                ? Array.Empty<Vector3>()
                : Checkpoints.ToArray()[..Mathf.FloorToInt(PointCount * Progress)];
        
        public Vector3[] CheckpointsRemaining() => 
            IsEmpty
                ? Array.Empty<Vector3>()
                : Checkpoints.ToArray()[Mathf.FloorToInt(PointCount * Progress)..];


        public Vector3 CurrentPosition => Vector3.Lerp(
            PrevCheckpoint,
            NextCheckpoint,
            Mathf.InverseLerp(
                (float)PrevCheckpointIndex / PointCount,
                (float)NextCheckpointIndex / PointCount,
                progress)
        );

        #endregion

        
        #region MOVE PROGRESS

        public bool GoTo(int checkpointIndex)
        {
            if (checkpointIndex < 0 || checkpointIndex >= PointCount) return false;
            Progress = checkpointIndex / (PointCount - 1f);
            return true;
        }
        
        public bool GoTo(Vector3 checkpoint) => 
            Checkpoints.Contains(checkpoint)
            && GoTo(checkpoints.IndexOf(checkpoint));
        
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
