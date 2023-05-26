using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDParent : MonoBehaviour
{

    public List<Canvas> canvasList = new List<Canvas>();
    public List<Slider> healthList = new List<Slider>();
    public List<Slider> experienceList = new List<Slider>();
    public List<Slider> hungerList = new List<Slider>();
    public List<TextMeshProUGUI> nameList = new List<TextMeshProUGUI>();
    public List<TextMeshProUGUI> levelList = new List<TextMeshProUGUI>();




    void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            canvasList.Add(transform.GetChild(i).GetComponent<Canvas>());
        }
        foreach (Canvas item in canvasList)
        {
            //This will assign the slider to the list based on its position as a child to the canvas. Health first, then hunger and so on..
            healthList.Add(item.transform.GetChild(0).gameObject.GetComponent<Slider>());
            experienceList.Add(item.transform.GetChild(1).gameObject.GetComponent<Slider>());
            hungerList.Add(item.transform.GetChild(2).gameObject.GetComponent<Slider>());
            nameList.Add(item.transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>());
            levelList.Add(item.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>());
        }
    }
}
