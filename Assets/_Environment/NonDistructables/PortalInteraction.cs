using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalInteraction : MonoBehaviour
{

    InteractionManager interactionManager;
    AudioManager audioManager;
    public string destinationLevel;
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
        LevelManager.Instance.CallChangeLevelRPC(destinationLevel);
        return true;
    }
}
