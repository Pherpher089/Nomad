using UnityEngine;

///THIS CLASS IS FROM SEBASTIAN LAGE Free Spline Tool
///https://www.youtube.com/@SebastianLague
///Using it only for the Path constraint Feature (Until AC is updraded to 2022.2+ So I can use the Unity New Spline Module)

namespace MalbersAnimations.PathCreation
{
    [AddComponentMenu("Malbers/Animal Controller/Path Creator (SL)")]
    [HelpURL("https://youtu.be/saAQNRSYU9k")]
    public class PathCreator : MonoBehaviour
    {

        /// This class stores data for the path editor, and provides accessors to get the current vertex and bezier path.
        /// Attach to a GameObject to create a new path editor.

        public event System.Action pathUpdated;

        [SerializeField, HideInInspector]
        PathCreatorData editorData;
        [SerializeField, HideInInspector]
        bool initialized;

        GlobalDisplaySettings globalEditorDisplaySettings;

        // Vertex path created from the current bezier path
        public VertexPath path
        {
            get
            {
                if (!initialized)
                {
                    InitializeEditorData(false);
                }
                return editorData.GetVertexPath(transform);
            }
        }

        // The bezier path created in the editor
        public BezierPath bezierPath
        {
            get
            {
                if (!initialized)
                {
                    InitializeEditorData(false);
                }
                return editorData.bezierPath;
            }
            set
            {
                if (!initialized)
                {
                    InitializeEditorData(false);
                }
                editorData.bezierPath = value;
            }
        }

        #region Internal methods

        /// Used by the path editor to initialise some data
        public void InitializeEditorData(bool in2DMode)
        {
            if (editorData == null)
            {
                editorData = new PathCreatorData();
            }
            editorData.bezierOrVertexPathModified -= TriggerPathUpdate;
            editorData.bezierOrVertexPathModified += TriggerPathUpdate;

            editorData.Initialize(in2DMode);
            initialized = true;
        }

        public PathCreatorData EditorData
        {
            get
            {
                return editorData;
            }

        }

        public void TriggerPathUpdate()
        {
            if (pathUpdated != null)
            {
                pathUpdated();
            }
        }

#if UNITY_EDITOR

        // Draw the path when path objected is not selected (if enabled in settings)
        void OnDrawGizmos()
        {

            // Only draw path gizmo if the path object is not selected
            // (editor script is resposible for drawing when selected)
            GameObject selectedObj = UnityEditor.Selection.activeGameObject;
            if (selectedObj != gameObject)
            {

                if (path != null)
                {
                    path.UpdateTransform(transform);

                    if (globalEditorDisplaySettings == null)
                    {
                        globalEditorDisplaySettings = GlobalDisplaySettings.Load();
                    }

                    if (globalEditorDisplaySettings.visibleWhenNotSelected)
                    {

                        Gizmos.color = globalEditorDisplaySettings.bezierPath;

                        for (int i = 0; i < path.NumPoints; i++)
                        {
                            int nextI = i + 1;
                            if (nextI >= path.NumPoints)
                            {
                                if (path.isClosedLoop)
                                {
                                    nextI %= path.NumPoints;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            Gizmos.DrawLine(path.GetPoint(i), path.GetPoint(nextI));
                        }
                    }
                }
            }
        }
#endif

        #endregion
    }

    public class MinMax3D
    {
        public Vector3 Min { get; private set; }
        public Vector3 Max { get; private set; }

        public MinMax3D()
        {
            Min = Vector3.one * float.MaxValue;
            Max = Vector3.one * float.MinValue;
        }

        public void AddValue(Vector3 v)
        {
            Min = new Vector3(Mathf.Min(Min.x, v.x), Mathf.Min(Min.y, v.y), Mathf.Min(Min.z, v.z));
            Max = new Vector3(Mathf.Max(Max.x, v.x), Mathf.Max(Max.y, v.y), Mathf.Max(Max.z, v.z));
        }
    }

    public enum PathSpace { xyz, xy, xz };
    public enum EndOfPathInstruction { Loop, Reverse, Stop };
    public enum HandleType { Sphere, Circle, Square };
}