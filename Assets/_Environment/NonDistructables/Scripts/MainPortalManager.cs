using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPortalManager : MonoBehaviour
{
    HealthManager m_HealthManager;
    MainPortalInteraction m_MainPortalInteraction;
    // Start is called before the first frame update
    void Start()
    {
        m_HealthManager = GetComponent<HealthManager>();
        m_MainPortalInteraction = GetComponent<MainPortalInteraction>();
        m_HealthManager.health = 20 * m_MainPortalInteraction.numberOfFragments;

    }

    // Update is called once per frame
    void Update()
    {
        if (m_HealthManager.health < (m_MainPortalInteraction.numberOfFragments - 1) * 20)
        {
            m_MainPortalInteraction.CallRemovePortalPiece();
        }
    }

    public void AdjustPortalHealth()
    {
        m_HealthManager.health = 20 * m_MainPortalInteraction.numberOfFragments;
    }
}
