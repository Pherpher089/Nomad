using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class reload_WPIDEMO : MonoBehaviour
{
    //float time;

    void OnEnable()
    {
        event_manager_WPIDEMO.onCloseOptionScreen += reloadObject;
        event_manager_WPIDEMO.onStartLevel += reloadObject;
    }

    void OnDisable()
    {
        event_manager_WPIDEMO.onCloseOptionScreen -= reloadObject;
        event_manager_WPIDEMO.onStartLevel -= reloadObject;
    }

    void reloadObject()
    {
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }
}
