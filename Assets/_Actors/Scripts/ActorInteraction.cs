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
    public float holdInteractionTimer = 0;
    public bool hasInteracted = false;
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

    public void Update()
    {
        CheckPrompts();
    }

    private void CheckPrompts()
    {
        Ray ray = new Ray(transform.position + Vector3.up, transform.forward * 7);
        RaycastHit hit;
        Debug.DrawRay(transform.position + Vector3.up, transform.forward * 7, Color.blue);
        if (Physics.Raycast(ray, out hit, 4, interactLayer, QueryTriggerInteraction.Collide))
        {
            // Raycast in front of the player but only on the interact layer. This means all interactive objects need to be on the interact layer.
            InteractionPrompt im = hit.collider.gameObject.GetComponent<InteractionPrompt>();
            if (im)
            {
                im.ShowPrompt(LevelPrep.Instance.firstPlayerGamePad);
                return;
            }
        }
        ray = new Ray(transform.position + (Vector3.up * 0.02f), transform.forward * 7);

        Debug.DrawRay(transform.position + (Vector3.up * 0.02f), transform.forward * 7, Color.blue);
        if (Physics.Raycast(ray, out hit, 4, interactLayer, QueryTriggerInteraction.Collide))
        {
            // Raycast in front of the player but only on the interact layer. This means all interactive objects need to be on the interact layer.
            InteractionPrompt im = hit.collider.gameObject.GetComponent<InteractionPrompt>();
            if (im)
            {
                im.ShowPrompt(LevelPrep.Instance.firstPlayerGamePad);
                return;
            }
        }
    }


    /// <summary>
    /// Raycasts forward 7 units to check of intractable items. If intractable
    /// item is found, it proceeds with the interaction. 
    /// </summary>
    public void PressRaycastInteraction()
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
                if (!im.pressInteraction) return;
                //interact with the parent object
                im.Interact(this.gameObject);
                return;
            }
        }
        ray = new Ray(transform.position + (Vector3.up * 0.02f), transform.forward * 7);


        Debug.DrawRay(transform.position + (Vector3.up * 0.02f), transform.forward * 7, Color.red);
        if (Physics.Raycast(ray, out hit, 4, interactLayer, QueryTriggerInteraction.Collide))
        {
            // Raycast in front of the player but only on the interact layer. This means all interactive objects need to be on the interact layer.
            InteractionManager im = hit.collider.gameObject.GetComponent<InteractionManager>();
            if (im)
            {
                //interact with the parent object
                im.Interact(this.gameObject);
            }
        }
    }
    public void HoldRaycastInteraction(bool buttonDown)
    {
        if (buttonDown == false)
        {
            ResetInteraction();
            return;
        }

        bool missed = RaycastInteractionHold(transform.position + Vector3.up);
        missed &= RaycastInteractionHold(transform.position + (Vector3.up * 0.02f));

        if (missed)
        {
            ResetInteraction();
        }
    }


    /// <summary>
    /// Checks via raycast if the player is in range of an intractable object and implements 
    /// the interaction based on hold information. 
    /// </summary>
    /// <param name="rayOrigin">Where does the raycast start?</param>
    /// <returns>False</returns>
    private bool RaycastInteractionHold(Vector3 rayOrigin)
    {
        Ray ray = new Ray(rayOrigin, transform.forward * 7);
        Debug.DrawRay(rayOrigin, transform.forward * 7, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, 4, interactLayer, QueryTriggerInteraction.Collide))
        {
            InteractionManager im = hit.collider.gameObject.GetComponent<InteractionManager>();
            if (im && im.holdInteraction)
            {
                if (!hasInteracted)
                {
                    holdInteractionTimer += Time.deltaTime;
                }

                if (holdInteractionTimer >= im.holdInteractionTimer && !hasInteracted)
                {
                    im.Interact(gameObject);
                    ResetInteraction();
                }
                return false;
            }
        }
        return true;
    }

    private void ResetInteraction()
    {
        hasInteracted = false;
        holdInteractionTimer = 0;
    }
}
