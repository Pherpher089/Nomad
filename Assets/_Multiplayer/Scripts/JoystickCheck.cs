using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickCheck : MonoBehaviour {
    void Update()
    {
        int i = 0;
        while (i < 4)
        {
            if (Mathf.Abs(Input.GetAxis("Joy" + i + "X")) > 0.2F || Mathf.Abs(Input.GetAxis("Joy" + i + "Y")) > 0.2F)
                Debug.Log(Input.GetJoystickNames()[i] + " is moved");
            i++;
        }
    }
}
