using UnityEngine;


/// <summary>
/// This script controls the source objects from which players will gather resources from.
/// </summary>
public class SourceObject : MonoBehaviour
{

    public int hitPoints;     //the objects Hit points
    public int maxHitPoints;
    public GameObject yieldedRes;          //the resource object that is droped
    public Vector2 yieldRange = new Vector2(0, 0);
    public ToolType properTool = ToolType.Default;
    public int prefabIndex;
    void Start()
    {
        hitPoints = maxHitPoints;
    }
    public void Update()
    {
        if (hitPoints <= 0)
            YieldAndDie();
    }

    public void OnTriggerEnter(Collider other)
    {
        // if (other.gameObject.tag == "Tool")
        // {
        //     //TODO add is swinging criteria
        //     if (other.gameObject.GetComponent<Tool>().isAttacking)
        //     {
        //         TakeDamage(1);
        //     }
        // }
    }

    public void TakeDamage(int damage, ToolType toolType)
    {
        if (toolType == properTool && properTool != ToolType.Default)
        {
            hitPoints -= damage * 2;

        }
        else
        {
            hitPoints -= damage;
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
            Instantiate(yieldedRes, transform.position, transform.rotation);
        }
        GameObject parent = transform.parent.gameObject;
        this.transform.parent = null;
        parent.GetComponent<TerrainChunkRef>().terrainChunk.SaveChunk();
        Destroy(this.gameObject);
    }
}
