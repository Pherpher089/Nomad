using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameNextButton : MonoBehaviour
{
    [SerializeField] TMP_InputField playerNameInput;
    [SerializeField] Button nextButton;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (playerNameInput.text == "")
        {
            nextButton.interactable = false;
        }
        else
        {
            nextButton.interactable = true;
        }
    }
}
