using TMPro;
using UnityEngine;

public class CraftingSlot : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public ItemStack currentItemStack;
    public bool isOccupied = false;
    public TextMeshPro quantText;

    void Awake()
    {
        spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        quantText = transform.GetChild(1).GetComponent<TextMeshPro>();
        currentItemStack = new ItemStack(null, 0, 0, true);
    }
}

