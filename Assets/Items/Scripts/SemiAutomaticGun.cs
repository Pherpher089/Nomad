using UnityEngine;

public class SemiAutomaticGun : Item {

    public ParticleSystem shotEffect;
    public int partsPerShot = 1;
    bool triggerDown;
    
    public override void PrimaryAction(float input)
    {

        if(!triggerDown && input > 0)
        {
            shotEffect.Emit(1);
        }

        if(input > 0)
        {
            triggerDown = true;
        }
        else
        {
            triggerDown = false; 
        }
    }
}
