using UnityEngine;
/// <summary>
/// This allows for creating interactable object. The controller of the object
/// should provide a method to the interaction delegate in order for the ActorInteraction
/// to trigger the event. 
/// </summary>
public class InteractionManager : MonoBehaviour
{
    public delegate bool Interaction(GameObject i);
    public event Interaction OnInteract;
    public bool pressInteraction = true;
    public bool holdInteraction = false;
    public float holdInteractionTimer = 0;
    public bool canInteract = true;
    public bool Interact(GameObject _i)
    {
        if (canInteract) return OnInteract(_i);
        return false;
    }
}