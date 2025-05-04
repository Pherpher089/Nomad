using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPortalManager : MonoBehaviour
{
    public static MainPortalManager Instance;
    HealthManager m_HealthManager;
    MainPortalInteraction m_MainPortalInteraction;
    void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        m_HealthManager = GetComponent<HealthManager>();
        m_MainPortalInteraction = GetComponent<MainPortalInteraction>();
        m_HealthManager.health = 20 * m_MainPortalInteraction.numberOfFragments;

    }
    public void SetFragments()
    {
        //This method should be in here
        m_MainPortalInteraction.SetFragments();
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameStateManager.Instance.isRaidComplete && !GameStateManager.Instance.isRaid && m_MainPortalInteraction.numberOfFragments >= 8)
        {
            GameStateManager.Instance.StartRaid(this.transform, 180f);
        }
        if (m_HealthManager.health < (m_MainPortalInteraction.numberOfFragments - 1) * 20)
        {
            m_MainPortalInteraction.CallRemovePortalPiece();
        }
        if (GameStateManager.Instance.isRaid && m_MainPortalInteraction.numberOfFragments <= 0)
        {
            GameStateManager.Instance.EndRaid();
        }
        if (GameStateManager.Instance.isRaidComplete && GameStateManager.Instance.isRaid)
        {
            GameStateManager.Instance.EndRaid();
        }
    }

    public void AdjustPortalHealth()
    {
        m_HealthManager.health = 20 * m_MainPortalInteraction.numberOfFragments;
    }
}
