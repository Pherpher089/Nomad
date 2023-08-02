using UnityEngine;

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
    HealthManager healthManager;
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
        // Create a System.Random instance with the shared seed
        System.Random random = new System.Random(LevelManager.Instance.seed);
        int randomInt = 1;
        randomInt = yieldQuantity;
        int index = ItemManager.Instance.GetItemIndex(yieldObject);
        for (int j = 0; j < randomInt; j++)
        {
            GameObject newItem = Instantiate(yieldObject, transform.position + (Vector3.up * 2), Quaternion.identity);
            newItem.GetComponent<Rigidbody>().useGravity = false;
            SpawnMotionDriver spawnMotionDriver = newItem.GetComponent<SpawnMotionDriver>();
            float randX = random.Next(-2, 3);
            float randY = random.Next(-2, 3);
            Item item = newItem.GetComponent<Item>();
            item.parentChunk = parentChunk;
            item.hasLanded = false;
            spawnMotionDriver.Fall(new Vector3(randX, 5f, randY));
        }
        LevelManager.Instance.UpdateSaveData(parentChunk, index, id, true, transform.position, transform.rotation.eulerAngles, true);
        Destroy(this.gameObject);

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
