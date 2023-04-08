using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingObjectType { Wall = 0, Floor = 0 }

public class BuildingObject : MonoBehaviour
{
    public bool isPlaced = false;
    public BuildingObjectType buildingPieceType;
    List<BuildingObject> neighborPieces;
    Collider col;
    Renderer meshRenderer;
    //TODO pull from recourse
    public Material goodPlacementMat;
    public Material badPlacementMat;

    private Material[] originalMaterials;

    public bool isValidPlacement = false;
    public List<Collider> validCollisionObjects;
    public void Start()
    {
        if (transform.parent.tag == "WorldTerrain")
        {
            isPlaced = true;
        }
        col = GetComponent<Collider>();
        col.isTrigger = true;
        meshRenderer = GetComponent<Renderer>();
        originalMaterials = meshRenderer.materials;

    }

    void Update()
    {
        if (isPlaced == false && transform.parent.tag == "WorldTerrain")
        {
            isPlaced = true;
        }
        if (isPlaced)
        {
            if (col.isTrigger == true)
            {
                col.isTrigger = false;
                Material[] materials = new Material[originalMaterials.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = originalMaterials[i];
                }
                GetComponent<Renderer>().materials = materials;
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
