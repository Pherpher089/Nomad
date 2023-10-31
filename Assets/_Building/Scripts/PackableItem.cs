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
        currentMesh = meshFilter.sharedMesh;
        _collider = GetComponent<MeshCollider>();
        initialized = true;
    }

    public void OnEnable()
    {
        OnInteract += PackRPC;
    }

    public void OnDisable()
    {
        OnInteract -= PackRPC;
    }
    public bool PackRPC(GameObject i)
    {
        LevelManager.Instance.CallPackItem(this.GetComponent<Item>().id);
        return true;
    }


    //Packs or unpacks a packable item. It also adjusts the save data for the new state
    public bool PackAndSave(GameObject i)
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
            BuildingMaterial item = i.GetComponent<BuildingMaterial>();
            //item.id = LevelManager.Instance.UpdateSavedItemState(item.id, "", item.parentChunk);
            return false;
        }
        else
        {
            meshFilter.sharedMesh = packedMesh;
            _collider.sharedMesh = packedMesh;
            packed = true;
            Item item = i.GetComponent<Item>();
            //item.id = LevelManager.Instance.UpdateSavedItemState(item.id, "Packed", item.parentChunk);
            GetComponent<BuildingMaterial>().isEquipable = true;
            return true;
        }
    }

    //This is called when a packed bench is equipped. No need to save the level
    public void JustPack()
    {
        if (!initialized) initialize();
        meshFilter.sharedMesh = packedMesh;
        packed = true;
        _collider.sharedMesh = packedMesh;
        GetComponent<BuildingMaterial>().isEquipable = true;
    }
}
