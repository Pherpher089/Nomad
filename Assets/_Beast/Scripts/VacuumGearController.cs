using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Photon.Pun;
using UnityEngine;
[RequireComponent(typeof(PhotonView))]

public class VacuumGearController : MonoBehaviourPun
{
    public GameObject vacuumHead;
    // Start is called before the first frame update
    void Start()
    {
        vacuumHead = GetComponentInChildren<VacuumRangeManager>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            BeastManager.Instance.Vacuum();
        }
    }
}
