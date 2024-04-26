using TMPro;
using UnityEngine;

public class PrintVersion : MonoBehaviour
{
    void Start()
    {
        GetComponent<TMP_Text>().text = Application.version;
    }
}
