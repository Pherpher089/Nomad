using UnityEngine;
using TMPro;
using System.Collections.Generic;
public class SpawnMotionDriver : MonoBehaviour
{
    private Vector3 initialVelocity;
    public float gravity = 9f;
    bool isFalling = false;
    private float time = .5f;
    private Vector3 startPos;
    public bool hasSaved = true;
    private string fallType = "default";

    private void Start()
    {
        startPos = transform.position;
        // if (!GetComponent<Item>().hasLanded && !hasSaved)
        // {
        //     Fall(Vector3.zero);
        // }
    }

    public void Fall(Vector3 _initialVelocity, string _fallType = "default")
    {
        isFalling = true;
        hasSaved = false;
        initialVelocity = _initialVelocity;
        fallType = _fallType;
        GetComponent<Collider>().isTrigger = true;
    }

    private void Update()
    {
        if (isFalling)
        {
            time += Time.deltaTime;

            switch (fallType.ToLower())
            {
                case "tree":
                    // Emulating a tree falling, by rotating the tree along the x-axis as it falls
                    transform.Rotate(Vector3.right, Time.deltaTime * 50);
                    if (transform.rotation.eulerAngles.x > 90)
                    {
                        transform.rotation = Quaternion.Euler(Vector3.right * 90);
                    }
                    FallWithGravity();
                    break;

                default:
                    FallWithGravity();
                    break;
            }
        }
    }

    private void FallWithGravity()
    {
        float y = startPos.y + initialVelocity.y * time - 0.5f * gravity * Mathf.Pow(time, 2);
        float x = startPos.x + initialVelocity.x * time;
        float z = startPos.z + initialVelocity.z * time;
        transform.position = new Vector3(x, y, z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!hasSaved && isFalling && time > 0.5f)
        {
            if (other.tag != "Player" && !other.gameObject.name.Contains("Grass") && other.GetComponent<SpawnMotionDriver>() == false && other.tag != "DoNotLand" && other.tag != "MainPortal" && other.tag != "Tool" && other.tag != "TheseHands" && (other.tag != "Beast") && other.tag != "Enemy")
            {
                Land();
            }
        }
    }

    public void Land(bool parent = true)
    {
        isFalling = false;
        Item item = GetComponent<Item>();
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        if (fallType == "tree") transform.rotation = Quaternion.Euler(Vector3.right * 90);
        PackableItem packable = GetComponent<PackableItem>();
        string stateData = "";
        if (packable != null)
        {
            GetComponent<MeshCollider>().isTrigger = false;
            if (packable.packed)
            {
                stateData = "Packed";
            }
        }
        float roundedX = (float)System.Math.Round(transform.position.x, 1);
        float roundedY = (float)System.Math.Round(transform.position.y, 1);
        if (roundedY < 0) roundedY = 0.1f;
        float roundedZ = (float)System.Math.Round(transform.position.z, 1);

        transform.position = new Vector3(roundedX, roundedY, roundedZ);
        //item.id = $"{ItemManager.Instance.GetItemIndex(item)}_{transform.position.x}_{transform.position.z}_{0}_{true}_{stateData}";
        item.id = GenerateObjectId.GenerateItemId(item);
        if (parent) item.transform.parent = GameObject.FindGameObjectWithTag("WorldTerrain").transform;
        item.hasLanded = true;
        hasSaved = true;
    }
}

