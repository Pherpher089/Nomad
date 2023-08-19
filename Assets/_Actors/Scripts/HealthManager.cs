using Photon.Pun;
using UnityEngine;

public class HealthManager : MonoBehaviour, IPunObservable
{
    public float maxHealth;
    public float health;
    public float healthRegenerationValue;
    GameObject shotEffectPrefab;
    public GameObject bleedingEffectPrefab;
    public bool bleed = true;
    [HideInInspector] public bool dead = false;
    Animator animator;
    ActorAudioManager audioManager;
    Rigidbody m_Rigidbody;
    HungerManager m_HungerManager;
    float hungerHitTimer = 5f;
    float hungerHitTimerLength = 10f;
    CharacterStats stats;
    public bool isCharacter;
    public GameStateManager gameController;
    private PhotonView pv;
    ThirdPersonUserControl userControl;

    public void Awake()
    {
        gameController = FindObjectOfType<GameStateManager>();
        userControl = GetComponent<ThirdPersonUserControl>();
        stats = GetComponent<CharacterStats>();
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
        pv = GetComponent<PhotonView>();
        audioManager = GetComponent<ActorAudioManager>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_HungerManager = GetComponent<HungerManager>();
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(health);
        }
        else
        {
            // Network player, receive data
            this.health = (float)stream.ReceiveNext();
        }
    }

    public void SetStats()
    {
        if (stats)
        {

            maxHealth = stats.maxHealth;
            health = stats.health;
            healthRegenerationValue = stats.healthRegenerationRate;
        }
        else
        {
            maxHealth = 1000;
            health = 1;
        }
    }

    void Update()
    {
        if (tag == "Player" && !userControl.initialized) return;
        if (dead && health > 0)
        {
            dead = false;
        }
        Regenerate();
    }

    public void Regenerate()
    {
        if (m_HungerManager != null)
        {
            if (m_HungerManager.m_StomachValue > 0.6f * m_HungerManager.m_StomachCapacity)
            {
                if (health < maxHealth && health > 0)
                {
                    health += healthRegenerationValue * (m_HungerManager.m_StomachValue / m_HungerManager.m_StomachCapacity) * Time.deltaTime;
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
                    hungerHitTimer = hungerHitTimerLength;
                    TakeHit(.3f);
                }
            }
        }
    }
    public void TakeHit(float damage)
    {
        health -= damage;
        if (animator != null && health > 0)
        {
            animator.SetBool("Attacking", false);
            animator.SetBool("TakeHit", true);
            audioManager?.PlayHit();
        }
        if (health <= 0)
        {
            health = 0;
            dead = true;
            audioManager?.PlayDeath();
        }
    }
    [PunRPC]
    public void TakeHitRPC(float damage, int toolType, Vector3 hitPos, string attackerPhotonViewID)
    {
        GameObject attacker = PhotonView.Find(int.Parse(attackerPhotonViewID)).gameObject;
        //Check for friendly  fire and return if setting is false
        if (gameObject.tag == "Player" && attacker.tag == "Player" && !gameController.friendlyFire)
        {
            return;
        }
        //Check if player is attacking the beast
        if (gameObject.tag == "Beast" && attacker.tag == "Player")
        {
            return;
        }
        //Is the player blocking?
        if (gameObject.tag == "Player" && animator.GetLayerWeight(1) > 0.1f)
        {
            audioManager.PlayBlockedHit();
        }
        else
        {
            if (bleed)
            {
                Instantiate(shotEffectPrefab, hitPos, transform.rotation);
            }
            float finalDamage = stats && damage - stats.defense > 0 ? damage - stats.defense : damage;
            health -= finalDamage;
            if (health <= 0)
            {
                health = 0;
                CharacterStats attackerStats = attacker.GetComponent<CharacterStats>();
                if (attackerStats != null)
                {
                    attackerStats.experiencePoints += 25;
                }
                dead = true;
                audioManager.PlayDeath();
            }
            else
            {
                audioManager.PlayImpact();
            }
        }

        if (animator != null && health > 0)
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
    public void Hit(int damage, ToolType toolType, Vector3 hitPos, GameObject attacker)
    {
        pv.RPC("TakeHitRPC", RpcTarget.All, (float)(1 + stats.attack), (int)toolType, transform.position, attacker.GetComponent<PhotonView>().ViewID.ToString());
    }

    public void TakeHit(float damage, ToolType toolType, Vector3 hitPos, GameObject attacker)
    {
        if (gameObject.tag == "Player" && attacker.tag == "Player" && !gameController.friendlyFire)
        {
            return;
        }
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
            float finalDamage = stats && damage - stats.defense > 0 ? damage - stats.defense : damage;
            health -= finalDamage;
            if (health <= 0)
            {
                health = 0;
                CharacterStats attackerStats = attacker.GetComponent<CharacterStats>();
                if (attackerStats != null)
                {
                    attackerStats.experiencePoints += 25;
                }
                dead = true;
                audioManager.PlayDeath();

            }
            else
            {
                audioManager.PlayImpact();
            }

        }

        if (animator != null && health > 0)
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
        };
    }
}
