using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ManaGeyserInteraction : InteractionManager
{

    public bool regenerates = true;
    public int m_CurrentResource = 20;
    public int m_MaxResource = 20;
    public int m_Counter = 0;
    public string m_NodeId;
    public GameObject spawnItem;

    void Start()
    {
        m_CurrentResource = m_MaxResource;
        m_NodeId = transform.position.ToString();
    }
    // Start is called before the first frame update
    void Update()
    {
        if (regenerates)
        {
            m_Counter += 1;
            if (m_Counter % 300 == 0 && m_CurrentResource < m_MaxResource)
            {
                m_CurrentResource++;
            }
        }
        if (m_CurrentResource <= 0)
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
        LevelManager.Instance.CallUpdateInteractionResourceRPC(m_NodeId, spawnItem.GetComponent<Item>().itemListIndex, m_Counter);
        return false;
    }
}
