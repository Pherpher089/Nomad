using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    // Start is called before the first frame update

    public float bullet_speed = 25;
    public Rigidbody projectileType;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Rigidbody instantiateProjectile = Instantiate(projectileType, transform.position, transform.rotation) as Rigidbody;
            instantiateProjectile.velocity = transform.TransformDirection(new Vector3(0, 9, bullet_speed));
        }
    }
}
