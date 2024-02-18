using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBeastCheck : MonoBehaviour
{
    GameObject beast = null;
    State beastStateToTrigger;
    // Update is called once per frame
    void Update()
    {
        if (beast == null)
        {
            beast = GameObject.FindGameObjectWithTag("Beast");
        }
        else
        {
            Debug.Log("### :" + beast.GetComponent<StateController>().currentState);
            if (beast.GetComponent<StateController>().currentState != beastStateToTrigger)
            {
                GetComponent<InteractionManager>().canInteract = true;
            }
        }
    }
}
