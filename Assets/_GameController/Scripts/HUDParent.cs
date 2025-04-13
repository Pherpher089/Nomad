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
    public List<Slider> healthRatioSlider = new List<Slider>();
    public List<Slider> hungerRatioSlider = new List<Slider>();
    public List<TextMeshProUGUI> healthRatioText = new List<TextMeshProUGUI>();
    public List<TextMeshProUGUI> hungerRatioText = new List<TextMeshProUGUI>();
    public List<TextMeshProUGUI> nameList = new List<TextMeshProUGUI>();
    public List<TextMeshProUGUI> levelList = new List<TextMeshProUGUI>();
    public List<List<Image>> toolBeltImages = new List<List<Image>>();
    public List<List<TMP_Text>> toolBeltItemCount = new List<List<TMP_Text>>();
    public List<GameObject> toolBeltCursors = new List<GameObject>();
    public List<int> backgroundIndices = new List<int>();

    public List<GameObject> backgrounds = new List<GameObject>();

    bool initialized = false;

    public void InitializeBars()
    {
        if (initialized)
        {
            return;
        }
        canvasList = new List<Canvas>();
        for (int i = 0; i < transform.childCount; i++)
        {
            canvasList.Add(transform.GetChild(i).GetComponent<Canvas>());
        }
        foreach (Canvas item in canvasList)
        {
            //This will assign the slider to the list based on its position as a child to the canvas. Health first, then hunger and so on..
            backgrounds.Add(item.transform.GetChild(0).gameObject);
            healthList.Add(item.transform.GetChild(1).gameObject.GetComponent<Slider>());
            experienceList.Add(item.transform.GetChild(2).gameObject.GetComponent<Slider>());
            hungerList.Add(item.transform.GetChild(3).gameObject.GetComponent<Slider>());
            healthRatioSlider.Add(item.transform.GetChild(4).gameObject.GetComponent<Slider>());
            hungerRatioSlider.Add(item.transform.GetChild(5).gameObject.GetComponent<Slider>());
            healthRatioText.Add(item.transform.GetChild(4).GetChild(2).gameObject.GetComponent<TextMeshProUGUI>());
            hungerRatioText.Add(item.transform.GetChild(5).GetChild(2).gameObject.GetComponent<TextMeshProUGUI>());
            nameList.Add(item.transform.GetChild(6).gameObject.GetComponent<TextMeshProUGUI>());
            levelList.Add(item.transform.GetChild(7).gameObject.GetComponent<TextMeshProUGUI>());
            List<Image> itemImages = new List<Image>();
            List<TMP_Text> itemCount = new List<TMP_Text>();
            for (int j = 0; j < item.transform.GetChild(8).childCount; j++)
            {
                if (j == 4)
                {
                    toolBeltCursors.Add(item.transform.GetChild(8).GetChild(j).gameObject);
                }
                else
                {
                    itemImages.Add(item.transform.GetChild(8).GetChild(j).GetChild(1).GetComponent<Image>());
                    itemCount.Add(item.transform.GetChild(8).GetChild(j).GetChild(2).GetComponent<TMP_Text>());
                }
            }
            toolBeltImages.Add(itemImages);
            toolBeltItemCount.Add(itemCount);
        }
        for (int i = 0; i < backgroundIndices.Count; i++)
        {
            if (backgroundIndices.Count > i)
            {
                if (backgroundIndices[i] < backgrounds.Count)
                {
                    backgrounds[i].transform.GetChild(backgroundIndices[i]).gameObject.SetActive(true);
                }
            }
        }
        initialized = true;
    }
}
