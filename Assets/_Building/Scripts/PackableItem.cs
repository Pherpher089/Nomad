using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackableItem : InteractionManager
{
    public MeshFilter meshFilter;
    public Mesh packedMesh;
    public Mesh currentMesh;
    public MeshCollider _collider;
    public GameObject packEffect;
    public bool packed = false;
    bool initialized = false;
    // Start is called before the first frame update
    void initialize()
    {
        meshFilter = GetComponent<MeshFilter>();
        currentMesh = meshFilter.mesh;
        _collider = GetComponent<MeshCollider>();
        initialized = true;
    }

    public void OnEnable()
    {
        OnInteract += Pack;
    }

    public void OnDisable()
    {
        OnInteract -= Pack;
    }

    public bool Pack(GameObject i)
    {
        GameObject effect = Instantiate(packEffect, transform.position, Quaternion.identity);
        effect.GetComponent<ParticleSystem>().Play();
        if (!initialized) initialize();
        if (packed)
        {
            meshFilter.sharedMesh = currentMesh;
            packed = false;
            _collider.sharedMesh = currentMesh;
            GetComponent<BuildingMaterial>().isEquipable = false;
            return false;
        }
        else
        {
            meshFilter.sharedMesh = packedMesh;
            _collider.sharedMesh = packedMesh;
            packed = true;
            GetComponent<BuildingMaterial>().isEquipable = true;
            return true;
        }
    }
}
