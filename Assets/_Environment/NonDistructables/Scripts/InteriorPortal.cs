using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InteriorPortal : MonoBehaviour
{

    InteractionManager interactionManager;
    public string spawnName;
    public void Awake()
    {
        interactionManager = GetComponent<InteractionManager>();
    }

    public void OnEnable()
    {
        interactionManager.OnInteract += Portal;
    }

    public void OnDisable()
    {
        interactionManager.OnInteract -= Portal;
    }

    public bool Portal(GameObject i)
    {
        if (spawnName == null || spawnName == "")
        {
            Debug.LogError($"!!! Interior Portal script on {gameObject.name} is missing a destination spawn name.");
        }
        LevelPrep.Instance.playerSpawnName = spawnName;
        LevelManager.Instance.CallChangeLevelRPC(LevelPrep.Instance.currentLevel);
        LevelPrep.Instance.isFirstLoad = false;
        return true;
    }
}
