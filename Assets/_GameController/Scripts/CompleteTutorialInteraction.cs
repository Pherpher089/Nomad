using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompleteTutorialInteraction : MonoBehaviour
{
    InteractionManager interactionManager;
    public void Awake()
    {
        interactionManager = GetComponent<InteractionManager>();
    }

    public void OnEnable()
    {
        interactionManager.OnInteract += CompleteTutorial;
    }

    public void OnDisable()
    {
        interactionManager.OnInteract -= CompleteTutorial;
    }

    public bool CompleteTutorial(GameObject i)
    {
        LevelManager.Instance.SaveGameProgress(1);
        return false;
    }
}
