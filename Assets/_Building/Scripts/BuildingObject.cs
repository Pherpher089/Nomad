using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public enum BuildingObjectType { Wall = 0, Floor = 1, Default = 2, Block = 3 }

public class BuildingObject : MonoBehaviour
{
    public bool isPlaced = false;
    public BuildingObjectType buildingPieceType;
    List<BuildingObject> neighborPieces;
    MeshCollider col;
    Renderer meshRenderer;
    //TODO pull from resource
    public Material goodPlacementMat;
    public Material badPlacementMat;

    private Material[] originalMaterials;

    public bool isValidPlacement = false;
    public List<Collider> validCollisionObjects;
    public void Awake()
    {
        if (transform.parent != null && transform.parent.tag == "WorldTerrain")
        {
            isPlaced = true;
        }
        col = GetComponent<MeshCollider>();

        col.convex = true;
        col.isTrigger = true;
        meshRenderer = GetComponent<Renderer>();
        originalMaterials = meshRenderer.materials;

    }


    void Update()
    {
        if (TryGetComponent<Item>(out Item _item1) && _item1.isEquipped)
        {
            // Material[] materials = new Material[originalMaterials.Length];
            // for (int i = 0; i < materials.Length; i++)
            // {
            //     materials[i] = originalMaterials[i];
            // }
            return;
        }
        if (isPlaced == false && transform.parent.tag == "WorldTerrain")
        {

            isPlaced = true;
            // Make sure if it has an item script and it is placed, 
            //it can not be picked up.
            if (TryGetComponent<Item>(out Item _item2))
            {
                _item2.isEquipable = false;
            }
        }
        if (isPlaced)
        {
            if (col.isTrigger == true)
            {
                col.isTrigger = false;
                if (transform.gameObject.name.Contains("DoorFrame") || transform.gameObject.name.Contains("SpellCircle"))
                {
                    col.convex = false;
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
        if (other.gameObject.tag == "WorldTerrain")
        {
            if (isPlaced && transform.parent == null)
            {
                transform.parent = other.transform;
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
    }

    void OnTriggerExit(Collider other)
    {
        validCollisionObjects.Remove(other);
        if (validCollisionObjects.Count == 0)
        {
            isValidPlacement = false;
        }
    }

    //if it is a floor or roof piece, it needs to find all of its bretherren
    //if it is a wall piece, it should find the same. 
    //This means that 
}
