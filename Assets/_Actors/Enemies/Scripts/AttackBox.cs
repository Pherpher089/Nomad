using System.Collections;
using System.Collections.Generic;
using MalbersAnimations.Scriptables;
using Photon.Pun;
using UnityEngine;

public class AttackBox : MonoBehaviour
{
    Animator animator;
    List<Collider> inRange = new List<Collider>();
    CharacterStats stats;
    void Start()
    {
        stats = transform.parent.GetComponent<CharacterStats>();
        animator = transform.parent.GetComponentInChildren<Animator>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (inRange.Contains(other))
        {
            return;
        }
        else
        {
            inRange.Add(other);
        }
    }
    void OnTriggerExit(Collider other)
    {

        if (inRange.Contains(other))
        {
            inRange.Remove(other);
        }
    }

    public void Bite()
    {
        foreach (Collider hit in inRange)
        {
            // Skip self
            // if (hit.gameObject == gameObject.transform.parent.gameObject || hit.transform.root.gameObject == gameObject.transform.parent.gameObject) continue;

            // Start checking up the hierarchy
            Transform currentTransform = hit.transform;
            while (currentTransform != null)
            {
                // Stop searching if we reach the WorldTerrain object
                if (currentTransform.CompareTag("WorldTerrain")) break;

                // Check for SpawnMotionDriver
                if (currentTransform.TryGetComponent<SpawnMotionDriver>(out var driver) && !driver.hasSaved)
                    break;

                // Check for BuildingMaterial
                if (currentTransform.TryGetComponent<BuildingMaterial>(out var bm))
                {
                    LevelManager.Instance.CallUpdateObjectsPRC(
                        bm.id,
                        bm.spawnId,
                        stats.attack,
                        ToolType.Default,
                        hit.transform.position,
                        transform.parent.GetComponent<PhotonView>()
                    );
                    break; // No need to continue once a component is found
                }

                // Check for HealthManager
                if (currentTransform.TryGetComponent<HealthManager>(out var hm))
                {
                    hm.Hit(stats.attack, ToolType.Default, hit.transform.position, gameObject, 1);
                    break; // No need to continue once a component is found
                }

                // Check for SourceObject
                if (currentTransform.TryGetComponent<SourceObject>(out var so))
                {
                    LevelManager.Instance.CallUpdateObjectsPRC(
                        so.id,
                        "",
                        stats.attack,
                        ToolType.Default,
                        hit.transform.position,
                        gameObject.GetComponent<PhotonView>()
                    );
                    break; // No need to continue once a component is found
                }

                // Move to the parent object
                currentTransform = currentTransform.parent;
            }
        }
    }
}
