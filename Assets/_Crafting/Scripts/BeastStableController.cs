using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeastStableController : MonoBehaviour
{
    public GameObject m_BeastObject;
    public SaddleStationUIController m_SaddleStationController;
    public bool isCorralDoorOpen = false;

    void Awake()
    {
        m_SaddleStationController = GetComponentInChildren<SaddleStationUIController>();
        
    }
}
