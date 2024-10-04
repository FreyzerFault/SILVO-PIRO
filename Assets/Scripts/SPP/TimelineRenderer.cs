using System.Linq;
using DavidUtils.Rendering;
using DavidUtils.Rendering.Extensions;
using UnityEngine;

namespace SILVO.SPP
{
    [RequireComponent(typeof(LineRenderer)), ExecuteAlways]
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


        protected override void Awake()
        {
            Mode = RenderMode.Sphere;
            base.Awake();
            
            _lr = gameObject.GetComponent<LineRenderer>();
            _lr.useWorldSpace = true;
            
            UpdateColor();
        }

        private void OnEnable()
        {
            if (timeline == null) return;
            timeline.onCheckpointAdded += UpdateTimeline;
        }
        private void OnDisable()
        {
            if (timeline == null) return;
            timeline.onCheckpointAdded -= UpdateTimeline;
        }

        private void Start() => UpdateTimeline();

        
        public void UpdateTimeline()
        {
            UpdateLineRenderer();
            UpdateCheckPoints();
        }
        

        #region LINE

        private LineRenderer _lr;
        [HideInInspector] public Color lineColor = Color.white;
        
        public void UpdateLineRenderer() => _lr.SetPoints(timeline.Checkpoints);
        public void UpdateLineColor() => _lr.startColor = _lr.endColor = lineColor;

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
