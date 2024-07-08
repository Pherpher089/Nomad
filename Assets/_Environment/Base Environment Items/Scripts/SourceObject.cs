using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SourceObject : MonoBehaviour
{
    public int maxHitPoints;
    public ToolType properTool;
    public bool properToolOnly;
    public GameObject[] yieldedRes;
    public Vector2[] yieldRange;
    public GameObject shotEffectPrefab;
    public int environmentListIndex;
    private AudioManager audioManager;
    [HideInInspector] public string id;
    [HideInInspector] public GameObject damagePopup;
    [HideInInspector] public int hitPoints;
    [HideInInspector] public StatusEffectController statusEffects;

    private System.Random random;

    void Awake()
    {
        statusEffects = GetComponentInChildren<StatusEffectController>();
        damagePopup = Resources.Load("Prefabs/DamagePopup") as GameObject;
        id = GenerateObjectId.GenerateSourceObjectId(this);
    }

    private void Start()
    {
        audioManager = GetComponent<AudioManager>();
        hitPoints = maxHitPoints;

        if (!shotEffectPrefab)
        {
            shotEffectPrefab = Resources.Load("BleedingEffect") as GameObject;
        }

        // Initializing the random instance with the hashed value of the ID.
        int seed = id.GetHashCode();
        random = new System.Random(seed);
    }
    private void ShowDamagePopup(float damageAmount, Vector3 position)
    {
        GameObject popup = Instantiate(damagePopup, position + (Vector3.up * 2), Quaternion.identity);
        popup.GetComponent<DamagePopup>().Setup(damageAmount);
    }
    public void Hit(int damage, ToolType toolType, Vector3 hitPos, GameObject attacker)
    {
        LevelManager.Instance.CallUpdateObjectsPRC(id, "", damage, toolType, hitPos, attacker.GetComponent<PhotonView>());
    }

    public void TakeDamage(int damage, ToolType toolType, Vector3 hitPos, GameObject attacker)
    {

        Instantiate(shotEffectPrefab, hitPos, transform.rotation);
        if (audioManager)
        {
            int effectIndex = random.Next(0, audioManager.soundEffects.Length);
            if (toolType != ToolType.Hands)
            {
                audioManager.PlaySoundEffect(effectIndex);
            }
            else
            {
                ActorAudioManager actorAudioManager = attacker.GetComponent<ActorAudioManager>();
                if (actorAudioManager)
                {
                    actorAudioManager.PlayImpact();
                }
            }
        }
        if (properToolOnly && toolType != properTool)
        {
            ShowDamagePopup(0, transform.position);

        }
        else if (toolType == properTool && properTool != ToolType.Default)
        {
            hitPoints -= damage * 2;
            ShowDamagePopup(damage * 2, transform.position);

        }
        else if (toolType == ToolType.Arrow)
        {
            hitPoints -= 1;
            ShowDamagePopup(1, transform.position);

        }
        else
        {
            hitPoints -= damage;
            ShowDamagePopup(damage, transform.position);

        }
        if (hitPoints <= 0)
        {
            Yield(yieldedRes, yieldRange, random, id);
            LevelManager.Instance.SaveObject(id, true);
            Destroy(this.gameObject);
        }
    }
    public void Yield(GameObject[] yieldedRes, Vector2[] yieldRange, System.Random random, string id)
    {
        for (int i = 0; i < yieldedRes.Length; i++)
        {
            if (yieldedRes[i] == null || yieldRange[i] == null) continue;

            int randomInt = random.Next((int)yieldRange[i].x, (int)yieldRange[i].y + 1);

            for (int j = 0; j < randomInt; j++)
            {
                GameObject newItem = Instantiate(yieldedRes[i], transform.position + (Vector3.up * 2), Quaternion.identity);
                newItem.GetComponent<Rigidbody>().useGravity = false;
                SpawnMotionDriver spawnMotionDriver = newItem.GetComponent<SpawnMotionDriver>();
                float randX = random.Next(-2, 3);
                float randY = random.Next(-2, 3);
                Item item = newItem.GetComponent<Item>();
                item.spawnId = $"{randX}_{randY}_{environmentListIndex}_{i}_{j}";
                item.hasLanded = false;
                string fallType = gameObject.name.ToLower().Contains("tree") ? "tree" : "default";
                spawnMotionDriver.Fall(new Vector3(randX + i, 5f, randY + i), fallType);
            }
        }

    }

}

