using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootGenerator : MonoBehaviour
{
    public Item[] loot;
    public int[] lootMaxCount;
    public float[] lootSpawnChance;

    public ItemStack[] GenerateLoot()
    {
        List<ItemStack> generatedLoot = new List<ItemStack>();

        for (int i = 0; i < loot.Length; i++)
        {
            if (Random.value <= lootSpawnChance[i])
            {
                int itemCount = Random.Range(1, lootMaxCount[i] + 1); // Random count between 1 and lootMaxCount[i]
                ItemStack itemStack = new ItemStack(loot[i], itemCount, i, false);
                generatedLoot.Add(itemStack);

                // Ensure we don't exceed the limit of 9 items
                if (generatedLoot.Count >= 9)
                {
                    break;
                }
            }
        }

        return generatedLoot.ToArray();
    }

    public string GenerateLootState(ItemStack[] generatedLoot)
    {
        string newState = "[";
        for (int i = 0; i < generatedLoot.Length; i++)
        {

            newState += $"[{generatedLoot[i].item.itemListIndex},{generatedLoot[i].count}],";

        }
        newState += "]";

        return newState;
    }
}
