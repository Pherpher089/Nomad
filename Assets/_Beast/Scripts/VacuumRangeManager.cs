using UnityEngine;

public class VacuumRangeManager : MonoBehaviour
{
    public VacuumGearController vacuumController;
    void Start()
    {
        vacuumController = transform.parent.GetComponent<VacuumGearController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            Item item = other.GetComponent<Item>();
            if (item.isEquipped) return;
            if (LevelManager.Instance.allItems.Contains(item))
            {
                BeastManager.Instance.objectsInVauumRange.Add(other.GetComponent<Item>());
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            BeastManager.Instance.objectsInVauumRange.Remove(other.GetComponent<Item>());
        }
    }
}
