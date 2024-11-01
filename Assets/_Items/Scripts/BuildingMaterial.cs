using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(HealthManager))]
public class BuildingMaterial : Item
{

    /// <summary>
    /// The Game Object that is spawned when this object is destroyed.
    /// </summary>
    public GameObject yieldObject;
    /// <summary>
    /// How many of the yield objects are spawned.
    /// </summary>
    public int yieldQuantity = 0;
    [HideInInspector] public HealthManager healthManager;
    public override void Awake()
    {
        base.Awake();
        healthManager = GetComponent<HealthManager>();
    }
    public void Update()
    {
        //Check for depletion of health points
        HealthCheck();
    }

    /// <summary>
    /// Checks the health of the material and destroys the object is health is less than 0
    /// </summary>
    private void HealthCheck()
    {
        if (healthManager.dead)
        {
            Kill();
        }
    }

    /// <summary>
    /// Destroys objects and spawns yield objects
    /// </summary>
    public void Kill()
    {
        if (yieldObject == null) return;

        // Use the ID as a seed for randomness
        System.Random random = new System.Random(id.GetHashCode());
        int randomInt = yieldQuantity;

        for (int j = 0; j < randomInt; j++)
        {
            GameObject newItem = Instantiate(yieldObject, transform.position + (Vector3.up * 2), Quaternion.identity);
            newItem.GetComponent<Rigidbody>().useGravity = false;
            SpawnMotionDriver spawnMotionDriver = newItem.GetComponent<SpawnMotionDriver>();
            float randX = random.Next(-2, 3);
            float randY = random.Next(-2, 3);
            Item item = newItem.GetComponent<Item>();
            item.spawnId = $"{randX}_{randY}_{itemListIndex}__{j}";
            item.hasLanded = false;
            spawnMotionDriver.Fall(new Vector3(randX, 5f, randY));
        }
        LevelManager.Instance.SaveObject(id, true);
        Destroy(this.gameObject);
    }

    public void ShutOffObject(GameObject _object, bool destroy = false)
    {
        if (_object.TryGetComponent<MeshRenderer>(out var mesh))
        {
            mesh.enabled = false;
        }
        if (_object.TryGetComponent<Collider>(out var col))
        {
            col.enabled = false;
        }
        if (_object.transform.childCount > 0)
        {
            for (int i = 0; i < _object.transform.childCount; i++)
            {
                ShutOffObject(_object.transform.GetChild(i).gameObject);
            }
        }
        LevelManager.Instance.SaveObject(id, destroy);
    }
    public override void OnEquipped(GameObject character)
    {
        base.OnEquipped(character);
    }

    public override void OnUnequipped()
    {
        base.OnUnequipped();
    }
}
