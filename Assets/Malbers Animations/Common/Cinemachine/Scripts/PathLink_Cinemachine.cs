using Cinemachine;
using UnityEngine; 

namespace MalbersAnimations.PathCreation
{
    [AddComponentMenu("Malbers/Animal Controller/Path Link (Cinemachine Path)")]
    public class PathLink_Cinemachine : MonoBehaviour, IPath
    {
        [RequiredField] public CinemachinePathBase m_Path;
        [Tooltip("Resolution to find the closest point on the path")]
        [Min(1)]  public int m_SearchResolution = 50;

        private static readonly CinemachinePathBase.PositionUnits Normalized = CinemachinePathBase.PositionUnits.Normalized;

        public Vector3 StartPath => GetPointAtTime(0);

        public Vector3 EndPath => GetPointAtTime(1);

        public bool IsClosed => m_Path.Looped;

        public Bounds bounds => CalculateBounds();

        public float GetClosestTimeOnPath(Vector3 position)
        {
            return FindClosestPoint(position,  m_SearchResolution);
        }

     

        public Quaternion GetPathRotation(float NormalizedTime)
        {
            //HERE IS THE CALCULATION OF THE PATH
            return m_Path.EvaluateOrientationAtUnit(NormalizedTime, Normalized);
        }

        public Vector3 GetPointAtTime(float NormalizedTime)
        {
            //HERE IS THE CALCULATION OF THE PATH
            return m_Path.EvaluatePositionAtUnit(NormalizedTime, Normalized);
        }


        private float FindClosestPoint(Vector3 p, int stepsPerSegment)
        {
            stepsPerSegment = Mathf.RoundToInt(Mathf.Clamp(stepsPerSegment, 1f, 100f));
            float stepSize = 1f / stepsPerSegment;
            float start = 0;
            float end = 1;
            float bestPos = 0;
            float bestDistance = float.MaxValue;
            int iterations = m_Path.DistanceCacheSampleStepsPerSegment;
            stepSize /= iterations;

            Vector3 v0 = m_Path.EvaluatePosition(0);

            for (float f = start; f <= end; f += stepSize)
            {
                float m_Position = m_Path.StandardizeUnit(f, Normalized);

                Vector3 v = m_Path.EvaluatePositionAtUnit(m_Position, Normalized);
                float t = p.ClosestTimeOnSegment(v0, v);
                float d = Vector3.SqrMagnitude(p - Vector3.Lerp(v0, v, t));

                if (d < bestDistance)
                {
                    bestDistance = d;
                    bestPos = f - (1 - t) * stepSize;
                }
                v0 = v;
            }


            return bestPos;
        }



        internal Bounds CalculateBounds()
        {
            Bounds bounds = new Bounds();

            for (int i = 0; i <= 50; i++)
            {
                var pos = m_Path.EvaluatePositionAtUnit(i / 10f, Normalized);
                bounds.Encapsulate(transform.InverseTransformPoint(pos));
            }
            return bounds;
        }

        private void Reset()
        {
            var p = GetComponent<CinemachineSmoothPath>();

            if (p == null)
                m_Path = gameObject.AddComponent<CinemachineSmoothPath>();
            else
            {
                m_Path = p;
            }
        }
    }
}
