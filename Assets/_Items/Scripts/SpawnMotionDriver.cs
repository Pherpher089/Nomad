using UnityEngine;
using TMPro;
using System.Collections.Generic;
public class SpawnMotionDriver : MonoBehaviour
{
    private Vector3 initialVelocity;
    public float gravity = 40f;
    bool isFalling = false;
    private float time = 1f;
    private Vector3 startPos;
    public bool hasSaved = false;

    private void Start()
    {
        startPos = transform.position;
    }
    public void Fall(Vector3 _initialVelocity)
    {
        isFalling = true;
        initialVelocity = _initialVelocity;
    }

    private void Update()
    {
        if (isFalling)
        {
            time += Time.deltaTime;
            float y = startPos.y + initialVelocity.y * time - 0.5f * gravity * Mathf.Pow(time, 2);
            float x = startPos.x + initialVelocity.x * time;
            float z = startPos.z + initialVelocity.z * time;
            transform.position = new Vector3(x, y, z);
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("WorldTerrain") && hasSaved == false) // Replace "Ground" with the tag you use for the ground.
        {
            TerrainChunk chunk = GetComponent<Item>().parentChunk;
            isFalling = false;
            Item item = GetComponent<Item>();
            GetComponent<Rigidbody>().isKinematic = true;
            item.hasLanded = true;
            item.id = $"{(int)chunk.coord.x}{(int)chunk.coord.y}_{ItemManager.Instance.GetItemIndex(item)}_{(int)transform.position.x}_{(int)transform.position.z}_{(int)0}";
            item.SaveItem(chunk, false);
            hasSaved = true;
        }
    }
    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("WorldTerrain") && hasSaved == false) // Replace "Ground" with the tag you use for the ground.
        {
            TerrainChunk chunk = GetComponent<Item>().parentChunk;
            isFalling = false;
            Item item = GetComponent<Item>();
            GetComponent<Rigidbody>().isKinematic = true;
            item.id = $"{(int)chunk.coord.x}{(int)chunk.coord.y}_{ItemManager.Instance.GetItemIndex(item)}_{(int)transform.position.x}_{(int)transform.position.z}_{(int)0}";
            item.SaveItem(chunk, false);
            item.hasLanded = true;
            hasSaved = true;
        }
    }
}