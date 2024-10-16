using System.Diagnostics;
using UnityEngine;

public class SpellCircleAlterInteraction : InteractionManager
{
    SpellCraftingManager m_SpellCraftManager;
    public Transform m_Socket;
    // Start is called before the first frame update
    void Awake()
    {
        m_Socket = transform.GetChild(0);
        m_SpellCraftManager = GetComponentInParent<SpellCraftingManager>();
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
        m_SpellCraftManager.TrySpellCraft();
        return false;
    }
}
