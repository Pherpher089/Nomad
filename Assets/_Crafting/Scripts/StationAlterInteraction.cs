using System.Diagnostics;
using UnityEngine;

public class StationAlterInteraction : InteractionManager
{
    StationCraftingManager m_StationCraftManager;
    public Transform m_Socket;
    // Start is called before the first frame update
    void Awake()
    {
        m_Socket = transform.GetChild(0);
        m_StationCraftManager = GetComponentInParent<StationCraftingManager>();
    }

    public void OnEnable()
    {
        OnInteract += TrySpellCraft;
    }

    public void OnDisable()
    {
        OnInteract -= TrySpellCraft;
    }

    public bool TrySpellCraft(GameObject i)
    {
        m_StationCraftManager.TryStationCraft();
        return false;
    }
}
