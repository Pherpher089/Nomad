using UnityEngine;


/// <summary>
/// This script controls the source objects from which players will gather resources from.
/// </summary>
public class SourceObject : MonoBehaviour
{

    public int hitPoints;     //the objects Hit points
    public int maxHitPoints;
    public GameObject yieldedRes;          //the resource object that is dropped
    public Vector2 yieldRange = new Vector2(0, 0);
    public ToolType properTool = ToolType.Default;
    public int prefabIndex;
    public GameObject shotEffectPrefab;

    void Start()
    {
        hitPoints = maxHitPoints;
        if (!shotEffectPrefab)
        {
            shotEffectPrefab = Resources.Load("BleedingEffect") as GameObject;
        }
    }

    // public void TakeDamage(int damage, ToolType toolType)
    // {
    //     if (toolType == properTool && properTool != ToolType.Default)
    //     {
    //         hitPoints -= damage * 2;

    //     }
    //     else
    //     {
    //         hitPoints -= damage;
    //     }
    // }
    public void TakeDamage(int damage, ToolType toolType, Vector3 hitPos)
    {

        Instantiate(shotEffectPrefab, hitPos, transform.rotation);

        if (toolType == properTool && properTool != ToolType.Default)
        {
            hitPoints -= damage * 2;

        }
        else
        {
            hitPoints -= damage;
        }
        if (hitPoints <= 0)
        {
            YieldAndDie();
        }
    }

    void YieldAndDie()
    {
        int randomInt = 1;
        if (yieldRange != Vector2.zero)
        {
            randomInt = Random.Range((int)yieldRange.x, (int)yieldRange.y);
        }

        for (int i = 0; i < randomInt; i++)
        {
            Instantiate(yieldedRes, transform.position + (Vector3.up * .1f), Quaternion.identity);
        }
        GameObject parent = transform.parent.gameObject;
        this.transform.parent = null;
        parent.GetComponent<TerrainChunkRef>().terrainChunk.SaveChunk();
        Destroy(this.gameObject);
    }
}
