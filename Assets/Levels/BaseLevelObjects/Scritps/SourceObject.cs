using System.Collections;
using System.Collections.Generic;
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
    public int prefabIndex;
    GenerateLevel levelMaster;

    void Start()
    {
        levelMaster = GameObject.FindWithTag("LevelMaster").GetComponent<GenerateLevel>();
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

    public void TakeDamage(int damage)
    {
        hitPoints -= damage;
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
        levelMaster.UpdateObjects(this.gameObject, true);
        Destroy(this.gameObject);
    }
}
