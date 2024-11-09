using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class SnappingPoint : MonoBehaviour
{
    public HashSet<Collider> overlappingSnapPoints = new HashSet<Collider>();
    MeshRenderer meshRenderer;
    MeshRenderer parentMeshRenderer;
    Collider collider;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        parentMeshRenderer = transform.parent.GetComponent<MeshRenderer>();
        collider = GetComponent<Collider>();
        GetComponent<SphereCollider>().radius = 0.75f;
    }
    void LateUpdate()
    {
        bool shouldRenderSnapPoint = false;
        foreach (ObjectBuildController buildController in GameStateManager.Instance.activeBuildPieces)
        {
            if (buildController.currentSnap != null && buildController.currentSnap.Length == 2)
            {
                if (!buildController.currentSnap.Contains(this))
                {
                    shouldRenderSnapPoint = false;
                    break;
                }
                else
                {
                    shouldRenderSnapPoint = true;
                    break;
                }
            }
            if (Vector3.Distance(this.transform.position, buildController.transform.position) < 7)
            {
                shouldRenderSnapPoint = true;
            }
        }
        if (GameStateManager.Instance.numberOfBuilders > 0 && GameStateManager.Instance.enableBuildSnapping && parentMeshRenderer.enabled && shouldRenderSnapPoint)
        {
            meshRenderer.enabled = true;
            collider.enabled = true;
        }
        else
        {
            meshRenderer.enabled = false;
            collider.enabled = false;
        }
    }
    public bool isOverlapping
    {
        get { return overlappingSnapPoints.Count > 0; }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("SnapPoint") && !overlappingSnapPoints.Contains(other))
        {
            overlappingSnapPoints.Add(other);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (overlappingSnapPoints.Contains(other))
        {
            overlappingSnapPoints.Remove(other);

        }
    }
}