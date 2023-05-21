using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDParent : MonoBehaviour
{

    public List<Canvas> canvasList = new List<Canvas>();
    public List<Slider> healthbarList = new List<Slider>();
    public List<Slider> hungerhbarList = new List<Slider>();


    void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            canvasList.Add(transform.GetChild(i).GetComponent<Canvas>());
        }
        foreach (Canvas item in canvasList)
        {
            //This will assign the slider to the list based on its position as a child to the canvas. Health first, then hunger and so on..
            healthbarList.Add(item.transform.GetChild(0).gameObject.GetComponent<Slider>());
            hungerhbarList.Add(item.transform.GetChild(1).gameObject.GetComponent<Slider>());
        }
    }
}
