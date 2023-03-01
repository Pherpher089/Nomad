using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handels an actors interactions with the base and Items
/// </summary>
public class ActorInteraction : MonoBehaviour
{

    //Base building interaction
    ActorEquipment actorEquipment;
    bool buildPosition;
    bool buildInput;

    int interactLayer;              // The layer that all interactive objects lives
    int buildLayer;                 // The layer that all buildable objects live
    //Door interaction
    bool doorInput;

    public void Awake()
    {
        actorEquipment = GetComponent<ActorEquipment>();
    }

    public void Start()
    {
        interactLayer = LayerMask.GetMask("Interact");
        buildLayer = LayerMask.GetMask("Build");
    }

    public void DoorInput(bool input)
    {
        doorInput = input;
    }

    public void RaycastInteraction(bool interact)
    {
        Ray ray = new Ray(transform.position + Vector3.up, transform.forward * 7);

        RaycastHit hit;
        Debug.Log("### casting ");

        Debug.DrawRay(transform.position + Vector3.up, transform.forward * 7, Color.red);
        if (Physics.Raycast(ray, out hit, 4, interactLayer, QueryTriggerInteraction.Collide))
        {
            if (interact)
            {
                Debug.Log("### HIT " + hit.collider.gameObject.name);
                // Raycast in front of the player but only on the interact layer. This means all interactive objects need to be on the interact layer.
                InteractionManager im = hit.collider.gameObject.GetComponent<InteractionManager>();
                if (im)
                {
                    //interact with the parent object
                    im.Interact(0);
                }
            }
        }
    }
}
