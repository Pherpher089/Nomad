using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PromptController : MonoBehaviour
{
    public bool controller = false;
    public Sprite interactionButton;
    public Sprite interactionButtonController;

    public SpriteRenderer interactionPromptRenderer;

    public void Start()
    {
        interactionPromptRenderer = GetComponentInChildren<SpriteRenderer>();
        interactionPromptRenderer.sprite = interactionButton;
    }

    public void SetButton(bool controller)
    {
        if (controller)
        {
            interactionPromptRenderer.sprite = interactionButtonController;
        }
        else
        {
            interactionPromptRenderer.sprite = interactionButton;
        }
    }
}
