using System;
using SILVO.Asset_Importers;
using UnityEngine;

namespace SILVO.SPP
{
    public class Animal: MonoBehaviour
    {
        public AnimalTimeline timeline;
        
        public float animationSpeed = 10;
        
        public bool playOnStart;
        public bool playing;
        
        private int _curSegment;
        private float _t;
        
        private int CheckPointCount => timeline.PointCount;
        
        public SPP_Signal PrevSignal => timeline[_curSegment];
        public SPP_Signal NextSignal => IsEnded ? PrevSignal : timeline[_curSegment + 1];
        public Vector3 PrevPoint => timeline.Checkpoints[_curSegment];
        public Vector3 NextPoint => IsEnded ? PrevPoint : timeline.Checkpoints[_curSegment + 1];
        
        public bool IsEnded => _curSegment >= timeline.PointCount - 1;

        public Action onMoved;
        public Action onReset;
        
        private void Start()
        {
            if (timeline == null || timeline.IsEmpty) return;
            
            Reset();

            if (playOnStart) playing = true;
        }

        private void Update()
        {
            if (playing) Move(Time.deltaTime);
        }

        private void Reset()
        {
            _curSegment = 0;
            _t = 0;
            SetPosition();
            LookTowardsNext();
            onReset?.Invoke();
        }

        private void Move(float time)
        {
            _t += animationSpeed * time;
            
            if (_t > 1)
            {
                int segmentsPast = Mathf.FloorToInt(_t);
                _t -= Mathf.Floor(_t);
                _curSegment += segmentsPast;
            }
            
            SetPosition();
            LookTowardsNext();
            
            onMoved?.Invoke();
        }

        private void SetPosition()
        {
            Vector3 pos = Vector3.Lerp(PrevPoint, NextPoint, _t);
            transform.position = pos;
        }

        private void LookTowardsNext()
        {
            Vector3 forward = (NextPoint - PrevPoint).normalized;
            if (forward == Vector3.zero) return;
            transform.rotation = Quaternion.LookRotation(forward);
        }

        private Vector3 GetCheckpointPos(int checkpointIndex) =>
            timeline.Checkpoints[checkpointIndex];
    }
}
