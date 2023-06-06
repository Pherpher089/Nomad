using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public enum BuildingObjectType { Wall = 0, Floor = 1, Default = 2 }

public class BuildingObject : MonoBehaviour
{
    public bool isPlaced = false;
    public BuildingObjectType buildingPieceType;
    List<BuildingObject> neighborPieces;
    MeshCollider col;
    Renderer meshRenderer;
    //TODO pull from recourse
    public Material goodPlacementMat;
    public Material badPlacementMat;

    private Material[] originalMaterials;

    public bool isValidPlacement = false;
    public List<Collider> validCollisionObjects;
    NavmeshCut navCut;
    NavmeshAdd navAdd;
    public void Start()
    {
        navCut = GetComponent<NavmeshCut>();
        navAdd = GetComponent<NavmeshAdd>();
        if (navCut != null && buildingPieceType == BuildingObjectType.Wall)
        {
            navCut.enabled = false;
        }
        if (navAdd != null && buildingPieceType == BuildingObjectType.Floor)
        {
            navAdd.enabled = false;
        }
        if (transform.parent.tag == "WorldTerrain")
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
        if (isPlaced == false && transform.parent.tag == "WorldTerrain")
        {
            if (buildingPieceType == BuildingObjectType.Wall)
            {
                navCut.enabled = true;
            }
            if (navAdd != null)
            {
                navAdd.enabled = false;
            }
            isPlaced = true;
        }
        if (isPlaced)
        {
            if (col.isTrigger == true)
            {
                col.isTrigger = false;
                if (transform.gameObject.name.Contains("DoorFrame"))
                {
                    col.convex = false;
                }
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
