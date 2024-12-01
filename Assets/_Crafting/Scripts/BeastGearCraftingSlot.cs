using TMPro;
using UnityEngine;

public class BeastCraftingSlot : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public BeastGearStack beastGearStack;
    public bool isOccupied = false;

    void Awake()
    {
        spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        beastGearStack = new BeastGearStack(null, 0, true);
    }
}