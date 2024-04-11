using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BeastGear : MonoBehaviour
{
    public string description;
    public string gearName;
    public Sprite icon;
    public int gearIndex;
    public BeastManager beastManager;
    void Start()
    {
        beastManager = BeastManager.Instance;
    }

}
