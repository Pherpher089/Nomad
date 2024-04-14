using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class RideBeastInteraction : InteractionManager
{
    public GameObject[] seats;
    int openSeat = 0;

    void Start()
    {
        seats = new GameObject[4];
        for (int i = 0; i < 4; i++)
        {
            seats[i] = GameObject.FindGameObjectWithTag($"Seat{i + 1}");
        }
    }

    public void OnEnable()
    {
        OnInteract += Ride;
    }

    public void OnDisable()
    {
        OnInteract -= Ride;
    }

    public bool Ride(GameObject i)
    {
        BeastManager.Instance.CallSetRiders(i.GetComponent<PhotonView>().ViewID);
        return false;
    }

}
