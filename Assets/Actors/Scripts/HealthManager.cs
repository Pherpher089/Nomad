using UnityEngine;

public class HealthManager : MonoBehaviour
{

    public int maxHealth = 5;
    public int health = 5;
    GameObject shotEffectPrefab;
    public GameObject bleedingEffectPrefab;
    public bool bleed = true;
    [HideInInspector] public bool dead = false;
    Animator animator;
    ActorAudioManager audioManager;
    Rigidbody rigidbody;

    public void Awake()
    {

        if (transform.childCount > 0)
        {
            animator = transform.GetChild(0).gameObject.GetComponent<Animator>();
        }
        if (!bleedingEffectPrefab)
        {
            shotEffectPrefab = bleedingEffectPrefab = Resources.Load("BleedingEffect") as GameObject;
        }
        else
        {
            shotEffectPrefab = bleedingEffectPrefab;
        }

        audioManager = GetComponent<ActorAudioManager>();
        rigidbody = GetComponent<Rigidbody>();
    }

    public void Start()
    {
        health = maxHealth;
    }

    public void TakeDamage(int damage, ToolType toolType, Vector3 hitPos, GameObject attacker)
    {
        if (bleed)
        {
            Instantiate(shotEffectPrefab, hitPos, transform.rotation);
            Instantiate(bleedingEffectPrefab, hitPos, transform.rotation, transform);
        }

        health -= damage;
        if (animator != null)
        {
            animator.SetBool("Attacking", false);
            animator.SetBool("TakeHit", true);
            ThirdPersonCharacter playerCharacter = GetComponent<ThirdPersonCharacter>();
            AIMover aiCharacter = GetComponent<AIMover>();
            if (playerCharacter != null)
            {
                playerCharacter.UpdateAnimatorHit(transform.position - attacker.transform.position);
            }
            if (aiCharacter != null)
            {
                aiCharacter.UpdateAnimatorHit(transform.position - attacker.transform.position);
            }
        }

        if (health <= 0)
        {
            health = 0;
            dead = true;
            audioManager.PlayDeath();

        }
        else
        {
            audioManager.PlayImpact();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Tool")
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            Vector3 forceDir = transform.position - other.transform.position;
        }
        if (other.gameObject.tag == "Arrow")
        {
            TakeDamage(1, other.GetComponent<Tool>().toolType, other.transform.position, other.gameObject.GetComponent<Tool>().m_OwnerObject);
        }
    }
}
