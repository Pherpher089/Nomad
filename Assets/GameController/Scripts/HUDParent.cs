using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDParent : MonoBehaviour {

    public List<Canvas> canvasList = new List<Canvas>();
    public List<Slider> healthbarList = new List<Slider>();

    void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            canvasList.Add(transform.GetChild(i).GetComponent<Canvas>());
        }
        foreach (Canvas item in canvasList)
        {
            healthbarList.Add(item.transform.GetComponentInChildren<Slider>());
        }
    }
}
