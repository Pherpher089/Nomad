using System.Collections;
using Photon.Pun;
using UnityEngine;


/// <summary>
/// This script controls the source objects from which players will gather resources from.
/// </summary>
public class SourceObject : MonoBehaviour
{

    public int hitPoints;     //the objects Hit points
    public int maxHitPoints;
    public GameObject yieldedRes;          //the resource object that is dropped
    public Vector2 yieldRange = new Vector2(0, 0);
    public ToolType properTool = ToolType.Default;
    public int prefabIndex;
    public GameObject shotEffectPrefab;
    public AudioManager audioManager;
    public string id;

    void Start()
    {
        audioManager = GetComponent<AudioManager>();
        hitPoints = maxHitPoints;
        if (!shotEffectPrefab)
        {
            shotEffectPrefab = Resources.Load("BleedingEffect") as GameObject;
        }
    }
    public void TakeDamage(int damage, ToolType toolType, Vector3 hitPos, GameObject attacker)
    {
        LevelManager.Instance.CallUpdateObjectsPRC(id, damage, toolType, hitPos, attacker.GetComponent<PhotonView>());
        Instantiate(shotEffectPrefab, hitPos, transform.rotation);
        if (audioManager)
        {
            int effectIdex = Random.Range(0, audioManager.soundEffects.Length);
            if (toolType != ToolType.Hands)
            {
                audioManager.PlaySoundEffect(effectIdex);
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
        // Create a System.Random instance with the shared seed
        System.Random random = new System.Random(LevelManager.Instance.seed);

        int randomInt = 1;
        if (yieldRange != Vector2.zero)
        {
            randomInt = random.Next((int)yieldRange.x, (int)yieldRange.y);
        }

        for (int i = 0; i < randomInt; i++)
        {
            GameObject newItem = Instantiate(yieldedRes, transform.position + (Vector3.up), Quaternion.identity);
            newItem.GetComponent<Rigidbody>().useGravity = false;
            SpawnMotionDriver spawnMotionDriver = newItem.GetComponent<SpawnMotionDriver>();
            float randX = random.Next(-2, 3);
            float randY = random.Next(-2, 3);
            Item item = newItem.GetComponent<Item>();
            item.parentChunk = transform.parent.GetComponent<TerrainChunkRef>().terrainChunk;
            item.hasLanded = false;
            spawnMotionDriver.Fall(new Vector3(randX, 5f, randY));
        }
        GameObject parent = transform.parent.gameObject;
        LevelManager.Instance.UpdateSaveData(parent.gameObject.GetComponent<TerrainChunkRef>().terrainChunk, prefabIndex, id, true, transform.position, transform.rotation.eulerAngles, false);
        this.transform.parent = null;
        Destroy(this.gameObject);
    }

}
