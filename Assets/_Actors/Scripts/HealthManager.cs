using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ArchieAndrews.PrefabBrush;
using Photon.Pun;
using TMPro;
using UnityEngine;
public class HealthManager : MonoBehaviour, IPunObservable
{
    [Tooltip("Maximum health value.")]
    public float maxHealth;
    [Tooltip("The rate of health regeneration when hunger is half full")]
    public float healthRegenerationValue;
    [Tooltip("The particle effect that plays on hit. This is blood by default.")]
    public GameObject bleedingEffectPrefab;
    [Tooltip("Should the blood effect play on hit?")]
    public bool bleed = true;
    [Tooltip("What tool type should do more damage?")]
    public ToolType properTool = ToolType.Default;
    Animator animator;
    ActorAudioManager audioManager;
    HungerManager m_HungerManager;
    GameObject shotEffectPrefab;
    float hungerHitTimer = 5f;
    float hungerHitTimerLength = 10f;
    CharacterStats stats;
    EnemyStats enemyStats;
    [HideInInspector] public float health;
    [HideInInspector] public bool dead = false;
    [HideInInspector] public bool isCharacter;
    [HideInInspector] public GameStateManager gameController;
    [HideInInspector] public PhotonView pv;
    ThirdPersonUserControl userControl;
    [HideInInspector] public GameObject damagePopup;
    ThirdPersonCharacter character;
    [HideInInspector] public StatusEffectController statusEffects;
    List<Renderer> renderers;
    List<Material> originalMaterials;
    Material hitFlashMaterial;
    public void Awake()
    {
        health = maxHealth;
        pv = GetComponent<PhotonView>();
        gameController = FindObjectOfType<GameStateManager>();
        userControl = GetComponent<ThirdPersonUserControl>();
        stats = GetComponent<CharacterStats>();
        if (stats == null && TryGetComponent<StateController>(out var stateController))
        {
            enemyStats = stateController.enemyStats;
        }
        if (CompareTag("Player"))
        {
            character = GetComponent<ThirdPersonCharacter>();
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

        renderers = new List<Renderer>();
        if (TryGetComponent<Renderer>(out var ren))
        {
            renderers.Add(ren);
        }

        renderers = GetComponentsInChildren<Renderer>().ToList<Renderer>();
        originalMaterials = new List<Material>();
        foreach (Renderer _ren in renderers)
        {
            originalMaterials.Add(_ren.material);
        }
        hitFlashMaterial = Resources.Load<Material>("Materials/HitFlash");
        audioManager = GetComponent<ActorAudioManager>();
        m_HungerManager = GetComponent<HungerManager>();
        statusEffects = GetComponentInChildren<StatusEffectController>();
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
    private void ShowDamagePopup(float damageAmount, Vector3 position, Color color)
    {
        GameObject popup = Instantiate(damagePopup, position + (Vector3.up * 2), Quaternion.identity);
        popup.GetComponent<TMP_Text>().color = color;
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
    }

    void Update()
    {
        if (tag == "Player" && !userControl.initialized) return;
        if (dead && health > 0)
        {
            dead = false;
        }
        else if (health <= 0 && !dead)
        {
            dead = true;
            if (audioManager) audioManager.PlayDeath();
        }

        Regenerate();
    }

    public void Regenerate()
    {
        if (m_HungerManager != null)
        {
            if (m_HungerManager.stats.stomachValue > 0.6f * m_HungerManager.stats.stomachCapacity)
            {
                if (health < maxHealth && health > 0)
                {
                    health += healthRegenerationValue * (m_HungerManager.stats.stomachValue / m_HungerManager.stats.stomachCapacity) * Time.deltaTime;
                }
            }
            if (m_HungerManager.stats.stomachValue < 0.1f * m_HungerManager.stats.stomachCapacity)
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
        ShowDamagePopup(damage, transform.position, Color.red);

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
    public void TakeHitRPC(float damage, int toolType, Vector3 hitPos, string attackerPhotonViewID, float knockBackForce)
    {
        if (TryGetComponent<Item>(out var item))
        {
            if (item.isEquipped) return;
        }
        float finalDamage = 0;
        GameObject attacker = PhotonView.Find(int.Parse(attackerPhotonViewID)).gameObject;
        if (gameObject.tag == "Player" && attacker.tag == "Beast" && !gameController.friendlyFire)
        {
            return;
        }
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
            return;
        }
        if (gameObject.tag == "Enemy" && attacker.tag == "Enemy" && attackerPhotonViewID != GetComponent<PhotonView>().ViewID.ToString())
        {
            return;
        }
        if (gameObject.tag == "Player" && !character.canTakeDamage)
        {
            return;
        }
        if (TryGetComponent<BuildingMaterial>(out var bm))
        {
            if (bm.yieldObject == null)
            {
                return;
            }
        }

        if (bleed)
        {
            Instantiate(shotEffectPrefab, hitPos, transform.rotation);
        }
        float _damage = damage;
        if (toolType == (int)properTool && properTool != ToolType.Default)
        {
            _damage = damage * 3;
        }
        else if (attacker.TryGetComponent(out BuildingMaterial buildMat))
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
        finalDamage = _damage * (1 - damageReduction);
        health -= finalDamage;
        if (audioManager) audioManager?.PlayHit();
        ShowDamagePopup(finalDamage, transform.position, Color.red);
        StartCoroutine(HitFreezeCoroutine()); // Adding hit freeze here
        StartCoroutine(HitFlashCoroutine()); // Adding hit flash here
        if (attacker.TryGetComponent<HealthManager>(out var hm))
        {
            hm.RunHitFreeze();
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

        if (animator != null && health > 0)
        {
            if (CompareTag("Enemy"))
            {
                AIMover aiCharacter = GetComponent<AIMover>();
                StateController sc = GetComponent<StateController>();
                if (sc.playerDamageMap.ContainsKey(attackerPhotonViewID))
                {
                    sc.playerDamageMap[attackerPhotonViewID] += finalDamage;
                }
                else
                {
                    sc.playerDamageMap.Add(attackerPhotonViewID, finalDamage);
                }
                if (sc.currentState.ToString() == "EnemyWander" || sc.currentState.ToString() == "Idle")
                {
                    sc.target = attacker.transform;
                    sc.focusOnTarget = true;
                }
                sc.reevaluateTargetCounter += 3;
                if (!animator.GetBool("Attacking") && sc.attackCoolDown < sc.enemyStats.attackRate - .3f)
                {
                    animator.SetBool("TakeHit", true);
                }
                aiCharacter.CallUpdateAnimatorHit(transform.position - attacker.transform.position, knockBackForce);
            }
            ThirdPersonCharacter playerCharacter = GetComponent<ThirdPersonCharacter>();
            if (playerCharacter != null)
            {
                playerCharacter.UpdateAnimatorHit(transform.position - attacker.transform.position);
            }

        }
    }
    public void Hit(int damage, ToolType toolType, Vector3 hitPos, GameObject attacker, float knockBackForce)
    {
        pv.RPC("TakeHitRPC", RpcTarget.All, (float)damage, (int)toolType, hitPos, attacker.GetComponent<PhotonView>().ViewID.ToString(), knockBackForce);
    }

    public void Kill()
    {
        if (health > 0)
        {
            Hit((int)health + 1, ToolType.Default, transform.position, this.gameObject, 0);
        }
    }

    public void TakeHit(float damage, ToolType toolType, Vector3 hitPos, GameObject attacker, float knockBackForce = 0)
    {
        float finalDamage = 0;
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
            return;
        }

        if (gameObject.tag == "Enemy" && attacker.tag == "Enemy")
        {
            return;
        }

        if (gameObject.tag == "Player" && !character.canTakeDamage)
        {
            return;
        }

        if (TryGetComponent<BuildingMaterial>(out var bm))
        {
            if (bm.yieldObject == null)
            {
                return;
            }
        }

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
        finalDamage = _damage * (1 - damageReduction);
        health -= finalDamage;

        if (audioManager) audioManager?.PlayHit();
        ShowDamagePopup(finalDamage, transform.position, Color.red);
        StartCoroutine(HitFreezeCoroutine()); // Adding hit freeze here
        StartCoroutine(HitFlashCoroutine()); // Adding hit flash here
        if (attacker.TryGetComponent<HealthManager>(out var hm))
        {
            hm.RunHitFreeze();
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


        if (animator != null && health > 0)
        {
            if (CompareTag("Enemy"))
            {
                AIMover aiCharacter = GetComponent<AIMover>();
                StateController sc = GetComponent<StateController>();
                if (sc.playerDamageMap.ContainsKey(attacker.GetComponent<PhotonView>().ViewID.ToString()))
                {
                    sc.playerDamageMap[attacker.GetComponent<PhotonView>().ViewID.ToString()] += finalDamage;
                }
                else
                {
                    sc.playerDamageMap.Add(attacker.GetComponent<PhotonView>().ViewID.ToString(), finalDamage);
                }
                if (sc.currentState.ToString() == "EnemyWander" || sc.currentState.ToString() == "Idle")
                {
                    sc.target = attacker.transform;
                    sc.focusOnTarget = true;
                }
                sc.reevaluateTargetCounter += 3;
                if (!animator.GetBool("Attacking") && sc.attackCoolDown > .5f)
                {
                    animator.SetBool("TakeHit", true);
                }
                aiCharacter.CallUpdateAnimatorHit(transform.position - attacker.transform.position, knockBackForce);
            }
            ThirdPersonCharacter playerCharacter = GetComponent<ThirdPersonCharacter>();
            if (playerCharacter != null)
            {
                playerCharacter.UpdateAnimatorHit(transform.position - attacker.transform.position);
            }

        };
    }

    public void Heal(int healthValue, GameObject attacker)
    {
        pv.RPC("HealRPC", RpcTarget.All, (float)healthValue, attacker.GetComponent<PhotonView>().ViewID.ToString());
    }
    [PunRPC]
    public void HealRPC(float healthValue, string attackerPhotonViewID)
    {
        GameObject attacker = PhotonView.Find(int.Parse(attackerPhotonViewID)).gameObject;
        if (attacker.tag == "Player" && tag == "Enemy")
        {
            return;
        }
        if (attacker.tag == "Enemy" && tag == "Player")
        {
            return;
        }
        //Check if player is attacking the beast

        health += healthValue;
        if (health > maxHealth)
        {
            health = maxHealth;
        }
        ShowDamagePopup(healthValue, transform.position, Color.green);
    }
    public void RunHitFreeze()
    {
        StartCoroutine(HitFreezeCoroutine()); // Adding hit freeze here
    }
    public IEnumerator HitFreezeCoroutine()
    {
        if (animator != null)
        {
            animator.speed = 0; // Stop the animator
        }
        AIMover aiMover = GetComponent<AIMover>();
        if (aiMover != null)
        {
            aiMover.enabled = false; // Disable enemy movement
        }
        yield return new WaitForSeconds(0.15f); // Freeze for 0.1 seconds
        if (animator != null)
        {
            animator.speed = 1; // Resume the animator
        }
        if (aiMover != null)
        {
            aiMover.enabled = true; // Enable enemy movement
        }
    }

    public IEnumerator HitFlashCoroutine()
    {
        if (renderers != null && hitFlashMaterial != null)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].material = hitFlashMaterial;
            }
            yield return new WaitForSeconds(0.15f); // Flash duration
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].material = originalMaterials[i];
            } // Revert to original material
        }
    }

}
