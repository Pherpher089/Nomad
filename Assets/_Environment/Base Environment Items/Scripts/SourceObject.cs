using System.Collections;
using Photon.Pun;
using UnityEngine;

public class SourceObject : MonoBehaviour
{
    public int hitPoints;
    public int maxHitPoints;
    public GameObject[] yieldedRes;
    public Vector2[] yieldRange;
    public ToolType properTool = ToolType.Default;
    public int itemIndex;
    public GameObject shotEffectPrefab;
    public AudioManager audioManager;
    public string id;

    private System.Random random;

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

    public void Hit(int damage, ToolType toolType, Vector3 hitPos, GameObject attacker)
    {
        LevelManager.Instance.CallUpdateObjectsPRC(id, damage, toolType, hitPos, attacker.GetComponent<PhotonView>());
    }

    public void TakeDamage(int damage, ToolType toolType, Vector3 hitPos, GameObject attacker)
    {
        Debug.Log("### SO taking damage");
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

        if (toolType == properTool && properTool != ToolType.Default)
        {
            hitPoints -= damage * 2;
        }
        else
        {
            hitPoints -= damage;
        }
        if (hitPoints <= 0)
        {
            YieldAndDie();
        }
    }

    public void YieldAndDie()
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
                item.parentChunk = transform.parent.GetComponent<TerrainChunkRef>().terrainChunk;
                item.transform.parent = transform.parent;
                item.hasLanded = false;
                string fallType = gameObject.name.ToLower().Contains("tree") ? "tree" : "default";
                spawnMotionDriver.Fall(new Vector3(randX + i, 5f, randY + i), fallType);
            }
        }
        GameObject parent = transform.parent.gameObject;
        LevelManager.Instance.UpdateSaveData(parent.gameObject.GetComponent<TerrainChunkRef>().terrainChunk, itemIndex, id, true, transform.position, transform.rotation.eulerAngles, false);
        this.transform.parent = null;
        Destroy(this.gameObject);
    }
}
