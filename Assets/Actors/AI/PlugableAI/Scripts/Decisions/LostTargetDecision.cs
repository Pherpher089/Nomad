using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LostTargetDecision : Decision {

    public override bool Decide(StateController controller)
    {
        return true;
    }
}
