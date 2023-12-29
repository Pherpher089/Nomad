using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;

public class PortalInteraction : MonoBehaviour
{

    InteractionManager interactionManager;
    AudioManager audioManager;
    ParticleSystem particleSystem;
    bool canTeleport = true;
    public string destinationLevel;
    public void Awake()
    {
        interactionManager = GetComponent<InteractionManager>();
        particleSystem = GetComponentInChildren<ParticleSystem>();
        if (!LevelPrep.Instance.isFirstLoad && SceneManager.GetActiveScene().name == "HubWorld")
        {
            canTeleport = false;
            particleSystem.Stop();
        }
    }
    public void Update()
    {
        if (!canTeleport && GameStateManager.Instance.timeCounter < 5)
        {
            canTeleport = true;
            particleSystem.Play();
        }
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
        if (canTeleport)
        {
            LevelManager.Instance.CallChangeLevelRPC(destinationLevel);
            LevelPrep.Instance.isFirstLoad = false;
            return true;
        }
        return false;
    }
}
