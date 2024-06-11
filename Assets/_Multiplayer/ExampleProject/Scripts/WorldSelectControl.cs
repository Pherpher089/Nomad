using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldSelectControl : MonoBehaviour
{
    [SerializeField] TMP_Text selectedWorldText;
    [SerializeField] Button startGameButton;
    [SerializeField] GameObject worldList;


    public void SelectFirstWorld()
    {
        if (worldList.transform.childCount > 0)
        {
            SetWorld(worldList.transform.GetChild(0).gameObject.GetComponent<WorldListItem>().text.text);
        }
    }

    public void SetWorld(string world)
    {
        LevelPrep.Instance.settlementName = world;
        selectedWorldText.text = world;
    }

    void Update()
    {
        if (selectedWorldText.text == "")
        {
            startGameButton.interactable = false;
        }
        else
        {
            startGameButton.interactable = true;
        }
    }
}
