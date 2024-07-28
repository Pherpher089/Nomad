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
    [HideInInspector] public bool isPlaced = false;
    [HideInInspector] public bool isValidPlacement = false;
    [HideInInspector] public List<Collider> validCollisionObjects;
    List<BuildingObject> neighborPieces;
    MeshCollider col;
    Renderer meshRenderer;
    Material[] originalMaterials;
    public void Awake()
    {
        if (transform.parent != null && transform.parent.tag == "WorldTerrain")
        {
            isPlaced = true;
        }
        col = GetComponent<MeshCollider>();
        goodPlacementMat = Resources.Load<Material>("Materials/GoodPosition");
        badPlacementMat = Resources.Load<Material>("Materials/BadPosition");
        col.convex = true;
        col.isTrigger = true;
        meshRenderer = GetComponent<Renderer>();
        originalMaterials = meshRenderer.materials;

    }


    void Update()
    {
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
        if (isPlaced)
        {
            if (col.isTrigger == true)
            {
                col.isTrigger = false;
                if (transform.gameObject.name.Contains("DoorFrame") || transform.gameObject.name.Contains("SpellCircle") || transform.gameObject.name.Contains("Stable") || transform.gameObject.name.Contains("ApothecaryStation") || transform.gameObject.name.Contains("BlacksmithHut") || transform.gameObject.name.Contains("CookStation"))
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
