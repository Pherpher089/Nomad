using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class NonmasterBeastInitialization : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            GetComponent<StateController>().enabled = false;
        }
    }

}
