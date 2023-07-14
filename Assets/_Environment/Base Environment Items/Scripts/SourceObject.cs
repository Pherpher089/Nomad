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
        int randomInt = 1;
        if (yieldRange != Vector2.zero)
        {
            randomInt = Random.Range((int)yieldRange.x, (int)yieldRange.y);
        }

        for (int i = 0; i < randomInt; i++)
        {
            GameObject newItem = Instantiate(yieldedRes, transform.position + (Vector3.up * .5f), Quaternion.identity);
            newItem.GetComponent<Rigidbody>().isKinematic = true;
            StartCoroutine(ParabolicMotion(newItem, newItem.transform.forward, 5f, 9.8f)); // gravity as 9.8 for example
        }
        GameObject parent = transform.parent.gameObject;
        LevelManager.UpdateSaveData(parent.gameObject.GetComponent<TerrainChunkRef>().terrainChunk, prefabIndex, id, true, transform.position, transform.rotation.eulerAngles);
        this.transform.parent = null;
        Destroy(this.gameObject);
    }
    IEnumerator ParabolicMotion(GameObject obj, Vector3 direction, float initialVelocity, float gravity)
    {
        Debug.Log("### in coroutine");
        float time = 0;

        // Get the initial position.
        Vector3 startPos = obj.transform.position;

        while (true)
        {
            time += Time.deltaTime;

            // Calculate the next position.
            float y = startPos.y + initialVelocity * time - 0.5f * gravity * Mathf.Pow(time, 2);
            float x = direction.x * initialVelocity * time;
            float z = direction.z * initialVelocity * time;
            Vector3 nextPos = new Vector3(startPos.x + x, y, startPos.z + z);

            // Check if the object has hit the ground.
            if (nextPos.y >= startPos.y)
            {
                obj.transform.position = new Vector3(nextPos.x, startPos.y, nextPos.z);
                yield break;
            }

            // Update the position.
            obj.transform.position = nextPos;

            yield return null;
        }
    }
}
