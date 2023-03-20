using UnityEngine;
/// <summary>
/// This allows for creating interactable object. The controller of the object
/// should provide a method to the interaction delegate in order for the ActorInteraction
/// to trigger the event. 
/// </summary>
public class InteractionManager : MonoBehaviour
{
    public delegate bool Interaction(int i);

    public event Interaction OnInteract;

    public bool Interact(int _i)
    {
        return OnInteract(_i);
    }
}