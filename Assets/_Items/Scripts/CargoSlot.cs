using UnityEngine;

public class CargoSlot : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public CargoItem cargoItem;
    public bool isOccupied = false;

    void Awake()
    {
        spriteRenderer = transform.GetChild(1).GetComponent<SpriteRenderer>();
    }
}