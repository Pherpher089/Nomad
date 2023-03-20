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
    public int yeildQuantity = 0;
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
            if (yeildQuantity != 0)
            {
                Kill();
            }
        }
    }

    /// <summary>
    /// Destroys objects and spawns yield objects
    /// </summary>
    public void Kill()
    {
        Vector3 dropPos = Vector3.forward;
        for (int i = 0; i < yeildQuantity; i++)
        {
            Instantiate(yieldObject, transform.position + dropPos, transform.rotation, null);
            dropPos += -Vector3.forward;
        }

        GameObject.Destroy(this.gameObject);

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
