using UnityEngine;
using System.Collections;

public class InteractionManager : MonoBehaviour
{
    public delegate bool Interaction(int i);

    public event Interaction OnInteract;

    public bool Interact(int _i)
    {
       return OnInteract(_i);
    }
}