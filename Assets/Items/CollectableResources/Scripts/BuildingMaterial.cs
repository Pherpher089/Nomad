using UnityEngine;

[RequireComponent(typeof(HealthManager))]
public class BuildingMaterial : Item
{

    public GameObject yeildObj;
    HealthManager healthManager;
    public int yeildQuantity = 0;
    public void Start()
    {
        healthManager = GetComponent<HealthManager>();
    }
    public void Update()
    {
        if (healthManager.dead)
        {
            if (yeildQuantity != 0)
            {
                Kill();
            }
        }
    }

    public void Kill()
    {
        Vector3 dropPos = Vector3.forward;
        for (int i = 0; i < yeildQuantity; i++)
        {
            Instantiate(yeildObj, transform.position + dropPos, transform.rotation, null);
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
