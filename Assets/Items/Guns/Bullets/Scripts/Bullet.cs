using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public int bounceThreshold = 1;
    int bounceCount = 0;

    public void Update()
    {
        CheckBulletStatus();
    }

    private void CheckBulletStatus()
    {
        if (bounceCount >= bounceThreshold)
        {
            GameObject.Destroy(this.gameObject);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.collider.gameObject.transform.tag == "Enemy")
        {
            other.collider.gameObject.GetComponent<HealthManager>().TakeDamage(3);//TODO change the damage to a spesific bullet damage
        }
        bounceCount++;
    }
}
