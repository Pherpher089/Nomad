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
    EnemyStats enemyStats;
    public bool isCharacter;
    public GameStateManager gameController;
    private PhotonView pv;
    ThirdPersonUserControl userControl;
    public ToolType properTool = ToolType.Default;
    public GameObject damagePopup;

    public void Awake()
    {
        gameController = FindObjectOfType<GameStateManager>();
        userControl = GetComponent<ThirdPersonUserControl>();
        stats = GetComponent<CharacterStats>();
        if (stats == null && TryGetComponent<StateController>(out var stateController))
        {
            enemyStats = stateController.enemyStats;
        }
        damagePopup = Resources.Load("Prefabs/DamagePopup") as GameObject;
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
    private void ShowDamagePopup(float damageAmount, Vector3 position)
    {
        GameObject popup = Instantiate(damagePopup, position + (Vector3.up * 2), Quaternion.identity);
        popup.GetComponent<DamagePopup>().Setup(damageAmount);
    }
    public void SetStats()
    {
        if (stats)
        {

            maxHealth = stats.maxHealth;
            health = stats.health;
            healthRegenerationValue = stats.healthRegenerationRate;
        }
        else if (enemyStats)
        {
            //TODO add health to enemy stats and set it here
            //maxHealth = 1000;
            health = maxHealth;
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

    /// <summary>
    /// Used by the hunger hit and for the dev keys
    /// </summary>
    /// <param name="damage">How much damage will be dealt.</param>
    public void TakeHit(float damage)
    {
        health -= damage;
        ShowDamagePopup(damage, transform.position);

        if (animator != null && health > 0)
        {
            animator.SetBool("Attacking", false);
            animator.SetBool("TakeHit", true);
            if (audioManager) audioManager?.PlayHit();
        }
        if (health <= 0)
        {
            health = 0;
            dead = true;
            if (audioManager) audioManager?.PlayDeath();
        }
    }
    [PunRPC]
    public void TakeHitRPC(float damage, int toolType, Vector3 hitPos, string attackerPhotonViewID)
    {
        if (TryGetComponent<Item>(out var item))
        {
            if (item.isEquipped) return;
        }
        GameObject attacker = PhotonView.Find(int.Parse(attackerPhotonViewID)).gameObject;
        if (attacker.GetComponent<HealthManager>().health <= 0) return;
        //Check for friendly  fire and return if setting is false
        if (gameObject.tag == "Player" && attacker.tag == "Player" && !gameController.friendlyFire)
        {
            return;
        }
        if (gameObject.tag == "MainPortal" && attacker.tag == "Player" && !gameController.friendlyFire)
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
            if (audioManager) audioManager.PlayBlockedHit();
        }
        else if (gameObject.tag == "Enemy" && attacker.tag == "Enemy")
        {
            return;
        }
        else
        {
            if (bleed)
            {
                Instantiate(shotEffectPrefab, hitPos, transform.rotation);
            }
            float _damage = damage;
            if (toolType == (int)properTool && properTool != ToolType.Default)
            {
                _damage = damage * 3;
            }
            else if (attacker.TryGetComponent<BuildingMaterial>(out BuildingMaterial buildMat))
            {
                if (toolType == (int)ToolType.Arrow)
                {
                    _damage = damage / 3;
                }
            }
            float defenseValue = 0;
            if (TryGetComponent<ActorEquipment>(out var ac))
            {
                defenseValue = ac.GetArmorBonus();
            }
            if (stats)
            {
                defenseValue += stats.defense;
            }
            else if (enemyStats)
            {
                //TODO need to add base defense value as well
            }

            float damageReduction = defenseValue / (5 + defenseValue);
            float finalDamage = _damage * (1 - damageReduction);
            health -= finalDamage;
            if (audioManager) audioManager?.PlayHit();
            ShowDamagePopup(finalDamage, transform.position);
            if (TryGetComponent<StateController>(out var controller) && gameObject.tag == "Enemy" && attacker.tag != "Enemy")
            {
                controller.target = attacker.transform;
            }
            if (health <= 0 && !dead)
            {
                health = 0;
                CharacterStats attackerStats = attacker.GetComponent<CharacterStats>();
                if (attackerStats != null)
                {
                    attackerStats.experiencePoints += 25;
                }
                dead = true;
                if (audioManager) audioManager.PlayDeath();
            }
            else
            {
                if (audioManager) audioManager.PlayImpact();
            }
        }

        if (animator != null && health > 0)
        {
            if (CompareTag("Enemy") && !animator.GetBool("Attacking") || !CompareTag("Enemy"))
            {
                animator.SetBool("Attacking", false);
                animator.SetBool("TakeHit", true);
            }
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
        pv.RPC("TakeHitRPC", RpcTarget.All, (float)(damage), (int)toolType, transform.position, attacker.GetComponent<PhotonView>().ViewID.ToString());
    }

    public void TakeHit(float damage, ToolType toolType, Vector3 hitPos, GameObject attacker)
    {
        if (attacker.GetComponent<HealthManager>().health <= 0) return;

        if (TryGetComponent<Item>(out var item))
        {
            if (item.isEquipped) return;
        }
        if (gameObject.tag == "MainPortal" && attacker.tag == "Player" && !gameController.friendlyFire)
        {
            return;
        }
        if (gameObject.tag == "Player" && attacker.tag == "Player" && !gameController.friendlyFire)
        {
            return;
        }
        if (gameObject.tag == "Player" && animator.GetLayerWeight(1) > 0.1f)
        {
            if (audioManager) audioManager.PlayBlockedHit();
        }
        else if (gameObject.tag == "Enemy" && attacker.tag == "Enemy")
        {
            return;
        }
        else
        {
            if (bleed)
            {
                Instantiate(shotEffectPrefab, hitPos, transform.rotation);
                Instantiate(bleedingEffectPrefab, hitPos, transform.rotation, transform);
            }
            float _damage = damage;
            if (toolType == properTool && properTool != ToolType.Default)
            {
                _damage = damage * 3;
            }
            else if (attacker.TryGetComponent<BuildingMaterial>(out BuildingMaterial buildMat))
            {
                if (toolType == ToolType.Arrow)
                {
                    _damage = damage / 3;
                }
            }
            float defenseValue = 0;
            if (TryGetComponent<ActorEquipment>(out var ac))
            {
                defenseValue = ac.GetArmorBonus();
            }
            if (stats)
            {
                defenseValue += stats.defense;
            }
            else if (enemyStats)
            {
                //TODO need to add base defense value as well
            }
            float damageReduction = defenseValue / (5 + defenseValue);
            float finalDamage = _damage * (1 - damageReduction);
            health -= finalDamage;
            if (audioManager) audioManager?.PlayHit();
            ShowDamagePopup(finalDamage, transform.position);
            if (TryGetComponent<StateController>(out var controller) && gameObject.tag == "Enemy" && attacker.tag != "Enemy")
            {
                controller.target = attacker.transform;
            }
            if (health <= 0)
            {
                health = 0;
                CharacterStats attackerStats = attacker.GetComponent<CharacterStats>();
                if (attackerStats != null)
                {
                    attackerStats.experiencePoints += 25;
                }
                dead = true;
                if (audioManager) audioManager.PlayDeath();

            }
            else
            {
                if (audioManager) audioManager.PlayImpact();
            }

        }

        if (animator != null && health > 0)
        {
            if (CompareTag("Enemy") && !animator.GetBool("Attacking") || !CompareTag("Enemy"))
            {
                animator.SetBool("Attacking", false);
                animator.SetBool("TakeHit", true);
            }
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
