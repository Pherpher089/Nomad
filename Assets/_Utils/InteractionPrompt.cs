using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class InteractionPrompt : MonoBehaviour
{
    bool isPromptShowing = false;
    // Start is called before the first frame update
    Outline outline;
    GameObject promptUiParent;
    PromptController promptController;

    public void Start()
    {
        outline = GetComponent<Outline>();
        if (outline) outline.enabled = false;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).TryGetComponent(out PromptController prompt))
            {
                promptController = prompt;
            }
        }
        promptUiParent = promptController.gameObject;
        promptUiParent.SetActive(false);
    }
    public void Update()
    {
        if (isPromptShowing)
        {
            promptUiParent.SetActive(true);
            if (outline) outline.enabled = true;

        }
        else
        {
            if (outline) outline.enabled = false;
            promptUiParent.SetActive(false);
            // shut off prompt
        }
        isPromptShowing = false;
    }
    // Update is called once per frame
    public void ShowPrompt(bool controller)
    {
        isPromptShowing = true;
        promptController.SetButton(controller);
    }
}
