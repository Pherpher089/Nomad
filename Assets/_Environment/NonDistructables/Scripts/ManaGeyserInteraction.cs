using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ManaGeyserInteraction : InteractionManager
{

    public int m_CurrentManna = 20;
    public int m_MaxManna = 20;
    public int m_Counter = 0;
    public string m_GeyserId;

    void Start()
    {
        m_CurrentManna = m_MaxManna;
        m_GeyserId = transform.position.ToString();
    }
    // Start is called before the first frame update
    void Update()
    {
        m_Counter += 1;
        if (m_Counter % 300 == 0 && m_CurrentManna < m_MaxManna)
        {
            m_CurrentManna++;
        }
        if (m_CurrentManna <= 0)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            transform.GetChild(0).gameObject.SetActive(true);

        }
    }

    public void OnEnable()
    {
        OnInteract += SpewManna;
    }

    public void OnDisable()
    {
        OnInteract -= SpewManna;
    }

    public bool SpewManna(GameObject i)
    {
        LevelManager.Instance.CallUpdateMannaGeyserRPC(m_GeyserId, m_Counter);
        return false;
    }
}
