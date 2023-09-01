using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeastCargoInteraction : InteractionManager
{
    BeastCargoInventoryManager cargoManager;
    private void Start()
    {
        cargoManager = transform.GetChild(0).gameObject.GetComponent<BeastCargoInventoryManager>();
    }

    public void OnEnable()
    {
        OnInteract += OpenCargoUI;
    }

    public void OnDisable()
    {
        OnInteract -= OpenCargoUI;
    }

    public bool OpenCargoUI(GameObject _gameObject)
    {
        cargoManager.PlayerOpenUI(_gameObject);
        return false;
    }

}
