using UnityEngine;

public class HealthManager : MonoBehaviour
{

    public float maxHealth = 5;
    public float health = 5;
    public float regenVlaue;
    bool isRegenerating;
    GameObject shotEffectPrefab;
    public GameObject bleedingEffectPrefab;
    public bool bleed = true;
    [HideInInspector] public bool dead = false;
    Animator animator;
    ActorAudioManager audioManager;
    Rigidbody m_Rigidbody;
    HungerManager m_HungerManager;
    public float hungerHitTimer = 30f;
    public float hungerHitTimerLength = 30f;

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
        m_Rigidbody = GetComponent<Rigidbody>();
        m_HungerManager = GetComponent<HungerManager>();
    }

    public void Start()
    {
        health = maxHealth;
    }
    void Update()
    {
        if (m_HungerManager != null)
        {
            if (m_HungerManager.m_StomachValue > 0.6f * m_HungerManager.m_StomachCapacity)
            {
                if (health < maxHealth)
                {
                    health += regenVlaue * (m_HungerManager.m_StomachValue / m_HungerManager.m_StomachCapacity) * Time.deltaTime;
                }
            }
            if (m_HungerManager.m_StomachValue < 0.1f * m_HungerManager.m_StomachCapacity)
            {
                if (hungerHitTimer > 0)
                {
                    hungerHitTimer -= 0.1f;
                }
                else
                {
                    //TODO: need to create an overload for this kind of damage
                    hungerHitTimer = hungerHitTimerLength;
                    TakeHit(0.1f, ToolType.Default, transform.position, null);
                }
            }
        }
    }

    public void Regenerate()
    {

    }

    public void TakeHit(float damage, ToolType toolType, Vector3 hitPos, GameObject attacker)
    {
        Debug.Log("### Taking Hit");
        if (gameObject.tag == "Player" && animator.GetLayerWeight(1) > 0.1f)
        {

            audioManager.PlayBlockedHit();
        }
        else
        {
            if (bleed)
            {
                Instantiate(shotEffectPrefab, hitPos, transform.rotation);
                Instantiate(bleedingEffectPrefab, hitPos, transform.rotation, transform);
            }
            health -= damage;
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
            TakeHit(1, other.GetComponent<Tool>().toolType, other.transform.position, other.gameObject.GetComponent<Tool>().m_OwnerObject);
        }
    }
}
