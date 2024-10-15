using System.Collections.Generic;
using System.Linq;
using DavidUtils.ExtensionMethods;
using DavidUtils.Rendering;
using DavidUtils.Rendering.Extensions;
using UnityEngine;

namespace SILVO.SPP
{
    [ExecuteAlways]
    public class TimelineRenderer: PointsRenderer
    {
        [SerializeField] protected Timeline timeline;

        public Timeline Timeline
        {
            get => timeline ??= GetComponent<Timeline>();
            set
            {
                timeline = value;
                UpdateTimeline();
            }
        }

        
        private Vector3[] LocalCheckpoints => Timeline.Checkpoints.Select(transform.InverseTransformPoint).ToArray();

        protected void Awake()
        {
            Mode = RenderMode.Point;
            
            InitializeLineRenderers();
            
            UpdateColor();
            UpdateLineColor();
            UpdateLineWidth();
        }

        private void OnEnable()
        {
            if (Timeline == null) return;
            Timeline.onCheckpointsUpdated += UpdateCheckPoints;
            Timeline.onCheckpointAdded += AddCheckpoint;
            Timeline.onCheckpointInserted += InsertCheckpoint;
            Timeline.onCheckpointsDeleted += RemoveCheckpoint;
            Timeline.onCheckpointsMoved += MoveCheckpoint;
            Timeline.onProgressChanged += UpdateProgress;
        }
        private void OnDisable()
        {
            if (Timeline == null) return;
            Timeline.onCheckpointsUpdated -= UpdateCheckPoints;
            Timeline.onCheckpointAdded -= AddCheckpoint;
            Timeline.onCheckpointInserted -= InsertCheckpoint;
            Timeline.onCheckpointsDeleted -= RemoveCheckpoint;
            Timeline.onCheckpointsMoved -= MoveCheckpoint;
            Timeline.onProgressChanged -= UpdateProgress;
        }

        private void Start() => UpdateTimeline();

        
        public void UpdateTimeline()
        {
            UpdateLinePoints();
            UpdateCheckPoints();
        }


        #region CRUD CHECKPOINT

        public virtual void AddCheckpoint(Vector3 checkpoint)
        {
            UpdateLinePoints();
            AddObj(transform.InverseTransformPoint(checkpoint));
        }
        
        public virtual void InsertCheckpoint(int index, Vector3 checkpoint)
        {
            UpdateLinePoints();
            InsertObj(index, transform.InverseTransformPoint(checkpoint));
        }
        
        public virtual void RemoveCheckpoint(int index, Vector3 checkpoint)
        {
            UpdateLinePoints();
            RemoveObj(index);
        }
        
        public virtual void MoveCheckpoint(int index, Vector3 checkpoint)
        {
            UpdateLinePoints();
            UpdateObj(index, transform.InverseTransformPoint(checkpoint));
        }

        #endregion
        
        

        public void UpdateProgress(float progress) => UpdateLinePoints();


        #region LINE

        [SerializeField, HideInInspector] public LineRenderer lrNext;
        [SerializeField, HideInInspector] public LineRenderer lrPrev;

        public void InitializeLineRenderers()
        {
            LineRenderer[] lrs = GetComponentsInChildren<LineRenderer>();
            lrPrev = lrs.Length > 0 ? lrs[0] : null;
            lrNext = lrs.Length > 1 ? lrs[1] : null;
            if (lrPrev == null)
            {
                GameObject lrPrevObj = new GameObject("Prev LineRenderer");
                lrPrevObj.transform.SetParent(transform);
                lrPrev = lrPrevObj.AddComponent<LineRenderer>();
            }
            else if (lrNext == null)
            {
                GameObject lrNextObj = new GameObject("Next LineRenderer");
                lrNextObj.transform.SetParent(transform);
                lrNext = lrNextObj.AddComponent<LineRenderer>();
            }
            
            lrNext!.material = lrPrev!.material = Resources.Load<Material>("UI/Materials/Line Material");
            lrNext.useWorldSpace = lrPrev.useWorldSpace = true;
        }

        
        public void UpdateLinePoints()
        {
            List<Vector3> points = Timeline.Checkpoints;
            Vector3[] remaining = Timeline.CheckpointsRemaining();
            Vector3[] completed = Timeline.CheckpointsCompleted();
            
            
            if (Timeline.IsOnStart)
            {
                lrPrev.Clear();
                lrNext.SetPoints(points);
            }
            else if (Timeline.IsOnEnd)
            {
                lrPrev.SetPoints(points);
                lrNext.Clear();
            }
            else if (Timeline.IsOnCheckpoint)
            {
                lrPrev.SetPoints(completed);
                lrNext.SetPoints(remaining);
            }
            else
            {
                lrPrev.SetPoints(completed.Append(Timeline.CurrentPosition));
                lrNext.SetPoints(remaining.Prepend(Timeline.CurrentPosition));
            }
        }

        
        [SerializeField, HideInInspector] private Color lineColor = Color.white;
        [SerializeField, HideInInspector] private Color lineColorCompleted = Color.white.WithAlpha(0.5f);
        [SerializeField, HideInInspector] private float lineWidth = 1f;

        public Color LineColor
        {
            get => lineColor;
            set
            {
                lineColor = value;
                UpdateLineColor();
            }
        }
        public Color LineColorCompleted
        {
            get => lineColorCompleted;
            set
            {
                lineColorCompleted = value;
                UpdateLineColor();
            }
        }
        public float LineWidth
        {
            get => lineWidth;
            set
            {
                lineWidth = value;
                UpdateLineWidth();
            }
        }
        public bool LineVisible
        {
            get => lrPrev.enabled && lrNext.enabled;
            set
            { 
                lrPrev.enabled = value;
                lrNext.enabled = value;
            }
        }
        
        public void UpdateLineRendererAppearance()
        {
            UpdateLineWidth();
            UpdateLineColor();
        }
        
        public void UpdateLineWidth() => lrPrev.widthMultiplier = (lrNext.widthMultiplier = lineWidth) * 0.9f;
        public void UpdateLineColor()
        {
            lrNext.startColor = lrNext.endColor = lineColor;
            lrPrev.startColor = lrPrev.endColor = lineColorCompleted;
        }
        
        #endregion

        
        #region CHECKPOINTS
        
        private bool showCheckpoints = false;
        public bool ShowCheckpoints
        {
            get => showCheckpoints;
            set
            {
                showCheckpoints = value;
                UpdateCheckPoints();
            }
        }
        
        public virtual void UpdateCheckPoints()
        {
            if (!showCheckpoints || Timeline.IsEmpty)
            {
                Clear();
                return;
            }
            
            Vector3[] positions = LocalCheckpoints;
            UpdateAllObj(positions);
        }

        #endregion
    }
}
