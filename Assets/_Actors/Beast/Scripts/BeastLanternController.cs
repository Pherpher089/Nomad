using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class BeastLanternController : MonoBehaviour
{
    Light m_Light;
    // Start is called before the first frame update
    void Start()
    {
        m_Light = GetComponent<Light>();
        m_Light.intensity = 0;
    }

    // Update is called once per frame
    void Update()
    {

        if (GameStateManager.Instance && GameStateManager.Instance.timeState == TimeState.Night && m_Light.intensity != 1)
        {
            m_Light.intensity = Mathf.Lerp(m_Light.intensity, 1, Time.deltaTime);
        }
        else if (GameStateManager.Instance && GameStateManager.Instance.timeState == TimeState.Day && m_Light.intensity != 0)
        {
            m_Light.intensity = Mathf.Lerp(m_Light.intensity, 0, Time.deltaTime);

        }
    }
}
