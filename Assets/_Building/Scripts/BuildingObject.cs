using System.Collections;
using System.Collections.Generic;
using Mono.Cecil;
using Unity.AI.Navigation;
using UnityEngine;

public enum BuildingObjectType { Wall = 0, Floor = 1, Default = 2, Block = 3, Roof = 4 }

public class BuildingObject : MonoBehaviour
{
    public BuildingObjectType buildingPieceType;
    //TODO pull from resource
    Material goodPlacementMat;
    Material badPlacementMat;
    Material cursorSelected;
    public bool isPlaced = false;
    public bool isValidPlacement = false;
    public bool isSelected = false;
    [HideInInspector] public List<Collider> validCollisionObjects;
    List<BuildingObject> neighborPieces;
    MeshCollider col;
    Renderer meshRenderer;
    Material[] originalMaterials;
    List<GameObject> objectsInCursor;
    int currentSelectionIndex = 0;

    public void Awake()
    {
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
        meshRenderer = GetComponent<Renderer>();
        originalMaterials = meshRenderer.materials;
        objectsInCursor = new List<GameObject>();
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
            Material[] materials = new Material[originalMaterials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = cursorSelected;
            }
            GetComponent<Renderer>().materials = materials;
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
            Material[] materials = new Material[originalMaterials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = originalMaterials[i];
            }
            GetComponent<Renderer>().materials = materials;
            // Make sure if it has an item script and it is placed, 
            //it can not be picked up.
            if (TryGetComponent<Item>(out Item _item))
            {
                _item.isEquipable = false;
            }
        }
        else
        {
            if (isValidPlacement)
            {
                Material[] materials = new Material[originalMaterials.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = goodPlacementMat;
                }
                GetComponent<Renderer>().materials = materials;
            }
            else
            {
                Material[] materials = new Material[originalMaterials.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = badPlacementMat;
                }
                GetComponent<Renderer>().materials = materials;
            }
        }


    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Player" && other.gameObject.tag != "Enemy" && other.gameObject.tag != "Beast" && other.gameObject.tag != "TentBounds")
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
        if (other.gameObject.tag == "BuildingPiece")
        {
            isValidPlacement = true;
            if (!validCollisionObjects.Contains(other))
            {
                validCollisionObjects.Add(other);
            }
        }
        if (name.Contains("BuilderCursor") && other.TryGetComponent<BuildingObject>(out var buildingObject) && !objectsInCursor.Contains(other.gameObject))
        {
            objectsInCursor.Add(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        validCollisionObjects.Remove(other);
        if (validCollisionObjects.Count == 0)
        {
            isValidPlacement = false;
        }
        if (name.Contains("BuilderCursor") && objectsInCursor.Contains(other.gameObject))
        {
            Debug.Log($"### {other.gameObject.name} is leaving the trigger");
            other.GetComponent<BuildingObject>().isSelected = false;
            objectsInCursor.Remove(other.gameObject);
        }
    }

    //if it is a floor or roof piece, it needs to find all of its bretherren
    //if it is a wall piece, it should find the same. 
    //This means that 
}
