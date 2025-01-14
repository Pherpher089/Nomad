using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BuildingObjectType { Wall = 0, Floor = 1, Default = 2, Block = 3, Roof = 4 }

public class BuildingObject : MonoBehaviour
{
    public BuildingObjectType buildingPieceType;
    //TODO pull from resource
    Material goodPlacementMat;
    Material badPlacementMat;
    Material cursorSelected;
    [HideInInspector] public bool isPlaced = false;
    [HideInInspector] public bool isValidPlacement = false;
    [HideInInspector] public bool isSelected = false;
    [HideInInspector] public List<Collider> validCollisionObjects;
    List<BuildingObject> neighborPieces;
    MeshCollider col;
    List<Renderer> meshRenderers;
    List<List<Material>> originalMaterials;
    [HideInInspector] public List<GameObject> objectsInCursor;
    int currentSelectionIndex = 0;
    TransparentObject transparentObject;
    List<SnappingPoint> snappingPoints = new();
    [HideInInspector] public bool isSnapped = false;

    public void Awake()
    {
        transparentObject = GetComponent<TransparentObject>();
        if (transform.parent != null && transform.parent.tag == "WorldTerrain")
        {
            isPlaced = true;
        }
        col = GetComponent<MeshCollider>();
        goodPlacementMat = Resources.Load<Material>("Materials/GoodPosition");
        badPlacementMat = Resources.Load<Material>("Materials/BadPosition");
        cursorSelected = Resources.Load<Material>("Materials/Selected");
        col.convex = true;
        col.isTrigger = true;

        meshRenderers = GetComponentsInChildren<Renderer>(true).ToList();
        if (TryGetComponent<Renderer>(out var rootRenderer))
        {
            meshRenderers.Add(rootRenderer);
        }
        originalMaterials = new List<List<Material>>();

        foreach (Renderer renderer in meshRenderers)
        {
            originalMaterials.Add(renderer.materials.ToList());
        }

        objectsInCursor = new List<GameObject>();
        snappingPoints.AddRange(GetComponentsInChildren<SnappingPoint>());
    }

    public SnappingPoint[] GetOverlappingSnappingPoint()
    {
        foreach (SnappingPoint snapPoint in snappingPoints)
        {
            if (snapPoint.isOverlapping)
            {
                Collider[] collidersArray = snapPoint.overlappingSnapPoints.ToArray();
                Collider firstCollider = collidersArray.Length > 0 ? collidersArray[0] : null;
                if (firstCollider != null)
                {
                    SnappingPoint[] snappingPoints = new SnappingPoint[2];
                    snappingPoints[0] = firstCollider.GetComponent<SnappingPoint>();
                    snappingPoints[1] = snapPoint;
                    return snappingPoints;
                }

            }
        }
        return null;
    }

    void HighlightSelectedObject()
    {
        if (objectsInCursor.Count > 0 && name.Contains("BuilderCursor"))
        {
            if (objectsInCursor.Count - 1 < currentSelectionIndex)
            {
                currentSelectionIndex = objectsInCursor.Count - 1;
            }
            objectsInCursor[currentSelectionIndex].GetComponent<BuildingObject>().isSelected = true;
        }

    }

    public void CycleSelectedPiece()
    {
        if (objectsInCursor.Count > 0 && name.Contains("BuilderCursor"))
        {
            objectsInCursor[currentSelectionIndex].GetComponent<BuildingObject>().isSelected = false;
            if (objectsInCursor.Count - 1 < currentSelectionIndex + 1)
            {
                currentSelectionIndex = 0;
            }
            else
            {
                currentSelectionIndex++;
            }
            objectsInCursor[currentSelectionIndex].GetComponent<BuildingObject>().isSelected = true;
        }

    }
    public GameObject GetSelectedObject()
    {
        if (objectsInCursor.Count > 0 && currentSelectionIndex < objectsInCursor.Count && name.Contains("BuilderCursor"))
        {
            return objectsInCursor[currentSelectionIndex];
        }
        return null;
    }


    void Update()
    {
        if (name.Contains("BuilderCursor"))
        {
            HighlightSelectedObject();
            return;
        }
        if (isPlaced == false && transform.parent.tag == "WorldTerrain")
        {

            isPlaced = true;
            // Make sure if it has an item script and it is placed, 
            //it can not be picked up.
            if (TryGetComponent(out Item _item2))
            {
                _item2.isEquipable = false;
            }
        }
        if (isSelected)
        {
            foreach (Renderer renderer in meshRenderers)
            {
                Material[] mats = new Material[renderer.materials.Length];
                for (int j = 0; j < mats.Length; j++)
                {
                    mats[j] = cursorSelected;
                }
                renderer.materials = mats;
            }
        }
        else if (isPlaced)
        {
            if (col.isTrigger == true)
            {
                col.isTrigger = false;
                if (transform.gameObject.name.Contains("DoorFrame") || transform.gameObject.name.Contains("RealmwalkerDesk") || transform.gameObject.name.Contains("SpellCircle") || transform.gameObject.name.Contains("Stable") || transform.gameObject.name.Contains("ApothecaryStation") || transform.gameObject.name.Contains("BlacksmithHut") || transform.gameObject.name.Contains("CookStation") || transform.gameObject.name.Contains("ProvisionsCraftingDesk"))
                {
                    col.convex = false;
                }
                if (transform.gameObject.name.Contains("SpikeBarrier"))
                {
                    col.convex = true;
                    col.isTrigger = true;

                }
            }
            if (transparentObject != null && transparentObject.isTransparent) return;
            for (int i = 0; i < meshRenderers.Count; i++)
            {
                meshRenderers[i].materials = originalMaterials[i].ToArray();
            }
            // Make sure if it has an item script and it is placed, 
            //it can not be picked up.
            if (TryGetComponent<Item>(out Item _item))
            {
                _item.isEquipable = false;
            }
        }
        else
        {
            foreach (Renderer renderer in meshRenderers)
            {
                Material[] mats = new Material[renderer.materials.Length];
                for (int j = 0; j < mats.Length; j++)
                {
                    mats[j] = isValidPlacement ? goodPlacementMat : badPlacementMat;
                }
                renderer.materials = mats;
            }
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Player" && other.gameObject.tag != "Enemy" && other.gameObject.tag != "Beast" && other.gameObject.tag != "TentBounds" && !other.name.Contains("SnapPoint"))
        {
            if (isPlaced && transform.parent == null)
            {
                transform.parent = GameObject.FindGameObjectWithTag("WorldTerrain").transform;
            }
            isValidPlacement = true;
            if (!validCollisionObjects.Contains(other))
            {
                validCollisionObjects.Add(other);

            }
        }
        BuildingObject bo = other.GetComponent<BuildingObject>();
        if (bo == null)
        {
            bo = other.GetComponentInParent<BuildingObject>();
        }
        if (bo != null && name.Contains("BuilderCursor") && !objectsInCursor.Contains(bo.gameObject))
        {
            objectsInCursor.Add(bo.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        validCollisionObjects.Remove(other);
        if (validCollisionObjects.Count == 0)
        {
            isValidPlacement = false;
        }
        BuildingObject bo = other.GetComponent<BuildingObject>();
        if (bo == null)
        {
            bo = other.GetComponentInParent<BuildingObject>();
        }
        if (bo != null && name.Contains("BuilderCursor") && objectsInCursor.Contains(bo.gameObject))
        {
            bo.isSelected = false;
            objectsInCursor.Remove(bo.gameObject);
        }
    }

    public void OnDestroy()
    {
        foreach (GameObject cursorGameObject in objectsInCursor)
        {
            cursorGameObject.GetComponent<BuildingObject>().isSelected = false;
        }
    }
    //if it is a floor or roof piece, it needs to find all of its bretherren
    //if it is a wall piece, it should find the same. 
    //This means that 
}
