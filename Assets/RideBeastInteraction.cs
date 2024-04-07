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
        int seatCounter = 1;
        seats = new GameObject[4];
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.name == "Seat" + seatCounter.ToString())
            {
                seats[seatCounter - 1] = transform.GetChild(i).gameObject;
                seatCounter++;
            }
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
