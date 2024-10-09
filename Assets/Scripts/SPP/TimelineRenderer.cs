using System.Linq;
using DavidUtils.ExtensionMethods;
using DavidUtils.Rendering;
using DavidUtils.Rendering.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace SILVO.SPP
{
    [ExecuteAlways]
    public class TimelineRenderer: PointsRenderer
    {
        [SerializeField] protected Timeline timeline;

        public Timeline Timeline
        {
            get => timeline;
            set
            {
                timeline = value;
                UpdateTimeline();
            }
        }


        protected void Awake()
        {
            Mode = RenderMode.Sphere;
            
            InitializeLineRenderers();
            
            UpdateColor();
            UpdateLineColor();
            UpdateLineWidth();
        }

        private void OnEnable()
        {
            if (timeline == null) return;
            timeline.onCheckpointAdded += UpdateTimeline;
            timeline.onProgressChanged += UpdateTimeline;
        }
        private void OnDisable()
        {
            if (timeline == null) return;
            timeline.onCheckpointAdded -= UpdateTimeline;
        }

        private void Start() => UpdateTimeline();

        
        public void UpdateTimeline()
        {
            UpdateLinePoints();
            UpdateCheckPoints();
        }

        public void UpdateProgress() => UpdateLinePoints();


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
            Vector3[] points = timeline.Checkpoints;
            Vector3[] remaining = timeline.CheckpointsRemaining();
            Vector3[] completed = timeline.CheckpointsCompleted();
            
            
            if (timeline.OnStart)
            {
                lrPrev.Clear();
                lrNext.SetPoints(points);
            }
            else if (timeline.OnEnd)
            {
                lrPrev.SetPoints(points);
                lrNext.Clear();
            }
            else if (timeline.OnCheckpoint)
            {
                lrPrev.SetPoints(completed);
                lrNext.SetPoints(remaining);
            }
            else
            {
                lrPrev.SetPoints(completed.Append(timeline.CurrentPosition));
                lrNext.SetPoints(remaining.Prepend(timeline.CurrentPosition));
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
        
        private void UpdateCheckPoints()
        {
            Vector3[] positions = timeline.Checkpoints.Select(transform.InverseTransformPoint).ToArray();
            UpdateAllObj(positions);
        }

        #endregion
    }
}
