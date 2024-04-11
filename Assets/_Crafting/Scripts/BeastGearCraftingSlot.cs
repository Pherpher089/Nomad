using TMPro;
using UnityEngine;

public class BeastCraftingSlot : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public BeastGearStack beastGearStack;
    public bool isOccupied = false;
    public TextMeshPro quantText;

    void Awake()
    {
        spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        quantText = transform.GetChild(1).GetComponent<TextMeshPro>();
        beastGearStack = new BeastGearStack(null, 0, 0, true);
    }
}