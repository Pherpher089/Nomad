using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Actions/Dig")]

public class DigAction : Action
{
    public override void Act(StateController controller)
    {
        Dig(controller);
    }

    private void Dig(StateController controller)
    {
        Debug.Log("### Digging ", controller.gameObject);
        if (BeastManager.Instance.digTarget != null)
        {
            Debug.Log("### Digging");
            BeastManager.Instance.digTarget.Dig();
        }
    }
}
