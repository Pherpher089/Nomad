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
        if (other.gameObject.tag == "Item")
        {
            vacuumController.objectsInVauumRange.Add(other.GetComponent<Item>());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Item")
        {
            vacuumController.objectsInVauumRange.Remove(other.GetComponent<Item>());
        }
    }
}
