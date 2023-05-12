using UnityEngine;

/// <summary>
/// To manage interactions and provide interaction functionality to controllers
/// </summary>
public class ActorInteraction : MonoBehaviour
{
    /// <summary>
    /// The layer on which interactions take place.
    /// </summary>
    int interactLayer;
    ActorEquipment actorEquipment;
    //Door interaction
    bool doorInput;

    public void Awake()
    {
        actorEquipment = GetComponent<ActorEquipment>();
    }

    public void Start()
    {
        interactLayer = LayerMask.GetMask("Interact");
    }

    public void DoorInput(bool input)
    {
        doorInput = input;
    }


    /// <summary>
    /// Raycasts forward 7 units to check of interactable items. If interactable
    /// item is found, it proceeds with the interaction. 
    /// </summary>
    public void RaycastInteraction()
    {
        Ray ray = new Ray(transform.position + Vector3.up, transform.forward * 7);

        RaycastHit hit;

        Debug.DrawRay(transform.position + Vector3.up, transform.forward * 7, Color.red);
        if (Physics.Raycast(ray, out hit, 4, interactLayer, QueryTriggerInteraction.Collide))
        {
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
