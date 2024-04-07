using UnityEngine;

namespace MalbersAnimations.PathCreation
{
    [AddComponentMenu("Malbers/Animal Controller/Path Link (Path Creator)")]
    public class PathLink_PathCreator : MonoBehaviour, IPath
    {
        [RequiredField] public PathCreator m_Path;

        public Vector3 StartPath => transform.TransformPoint( m_Path.bezierPath.GetPoint(0));

        public Vector3 EndPath => transform.TransformPoint(m_Path.bezierPath.GetPoint(m_Path.bezierPath.NumPoints - 1));

        public bool IsClosed => m_Path.path.isClosedLoop;

        public Bounds bounds => m_Path.path.bounds;

        public float GetClosestTimeOnPath(Vector3 position) => m_Path.path.GetClosestTimeOnPath(position);

        public Quaternion GetPathRotation(float NormalizedTime)
        {
            //HERE IS THE CALCULATION OF THE PATH
            var Closed = IsClosed ? EndOfPathInstruction.Loop : EndOfPathInstruction.Stop;
            return m_Path.path.GetRotation(NormalizedTime, Closed);
        }

        public Vector3 GetPointAtTime(float NormalizedTime)
        {
            //HERE IS THE CALCULATION OF THE PATH
            var Closed = IsClosed ? EndOfPathInstruction.Loop : EndOfPathInstruction.Stop;
            return m_Path.path.GetPointAtTime(NormalizedTime, Closed);
        }

        private void Reset()
        {
            var p = GetComponent<PathCreator>();

            if (p == null)
                m_Path = gameObject.AddComponent<PathCreator>();
            else
            {
                m_Path = p;
            }
        }
    }
}
