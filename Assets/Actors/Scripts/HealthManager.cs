using UnityEngine;

public class HealthManager : MonoBehaviour
{

    public int maxHealth = 5;
    public int health = 5;
    GameObject shotEffectPrefab;
    public GameObject bleedingEffectPrefab;
    public bool bleed = true;
    [HideInInspector] public bool dead = false;
    Collider col;
    GenerateLevel levelMaster;

    public void Awake()
    {
        shotEffectPrefab = Resources.Load("ShotEffect") as GameObject;
        if (!bleedingEffectPrefab)
        {
            bleedingEffectPrefab = Resources.Load("BleedingEffect") as GameObject;
        }
        col = GetComponent<Collider>();
        levelMaster = GameObject.FindWithTag("GameController").GetComponent<GenerateLevel>();

    }

    public void Start()
    {
        health = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (bleed)
        {
            Instantiate(shotEffectPrefab, transform.position, transform.rotation);
            Instantiate(bleedingEffectPrefab, transform.position, transform.rotation, transform);
        }
        health -= damage;

        if (health <= 0)
        {
            health = 0;
            dead = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        float pushForce = 100000000f;
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 forceDirection = -transform.forward;
        forceDirection = forceDirection.normalized;
        float forceMagnitude = pushForce / rb.mass;

        // if (other.gameObject.tag == "Tool")
        // {
        //     if (other.gameObject.GetComponent<Tool>().isAttacking)
        //     {
        //         TakeDamage(1);
        //         if (gameObject.transform.tag == "Enemy")
        //         {
        //             //rb.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
        //         }

        //     }
        // }

        if (other.gameObject.tag == "Bullet")
        {
            TakeDamage(1);
        }
    }

    //private void OnCollisionEnter(Collision other)
    //{
    //    if (other.collider.gameObject.tag == "Tool")
    //    {
    //        TakeDamage(1);
    //    }

    //    if (other.collider.gameObject.tag == "Bullet")
    //    {
    //        TakeDamage(1);
    //    }

    //}
}
