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
    private ParticleSystem particleSys;
    public bool canTeleport = true;
    public string destinationLevel;
    public void Awake()
    {
        interactionManager = GetComponent<InteractionManager>();
        particleSys = GetComponentInChildren<ParticleSystem>();
        if (!LevelPrep.Instance.isFirstLoad && SceneManager.GetActiveScene().name == "HubWorld")
        {
            canTeleport = false;
            if (particleSys) particleSys.Stop();
        }
    }
    public void Update()
    {
        if (SceneManager.GetActiveScene().name == "TutorialWorld") return;
        if (!canTeleport && GameStateManager.Instance.timeCounter < 5)
        {
            canTeleport = true;
            if (particleSys) particleSys.Play();
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
            GameStateManager.Instance.CallChangeLevelRPC(destinationLevel, LevelPrep.Instance.currentLevel);
            LevelPrep.Instance.isFirstLoad = false;
            return true;
        }
        return false;
    }
}
