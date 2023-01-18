using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScatterGun : Item {

    public ParticleSystem shotEffect;
    bool triggerDown;


    public override void PrimaryAction(float input)
    {
        base.PrimaryAction(input);

        if (!triggerDown && input > 0)
        {
            shotEffect.Emit(1);
        }

        if (input > 0)
        {
            triggerDown = true;
        }
        else
        {
            triggerDown = false;
        }
    }
}
