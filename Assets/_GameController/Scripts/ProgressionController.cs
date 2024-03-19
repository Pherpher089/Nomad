using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProgressionController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (SceneManager.GetActiveScene().name == "HubWorld" && LevelManager.Instance.worldProgress == 0)
        {
            LevelManager.Instance.SaveGameProgress(1);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
