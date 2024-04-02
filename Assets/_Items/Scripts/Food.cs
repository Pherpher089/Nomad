using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : Item
{
    ActorAudioManager audioManager;
    public float foodValue = 100;
    public GameObject eatEffect;

    // Update is called once per frame
    private void Start()
    {
        audioManager = GetComponentInParent<ActorAudioManager>();
    }
    public override void PrimaryAction(float input)
    {
        //TODO this should be a method "eat" in the hunger manager
        m_OwnerObject.GetComponent<HungerManager>().Eat(foodValue);
        if (eatEffect != null)
        {
            Instantiate(eatEffect, transform.position, transform.rotation);
        }
        audioManager.PlayEat();
        m_OwnerObject.GetComponent<ActorEquipment>().SpendItem();
    }

    public override void OnEquipped(GameObject character)
    {
        base.OnEquipped(character);
        audioManager = character.GetComponent<ActorAudioManager>();
    }

    public override void OnUnequipped()
    {
        base.OnUnequipped();
    }
}
