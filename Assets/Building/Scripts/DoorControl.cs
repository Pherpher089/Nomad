using UnityEngine;

public class DoorControl : InteractionManager
{
    bool doorState;
    public bool interactinon;
    GameObject childDoor;
    Quaternion closedPos, openPos;
    InteractionManager interactionManager;

    public void Awake()
    {
        closedPos = transform.localRotation;
        openPos = Quaternion.Euler(transform.localRotation.eulerAngles + new Vector3(0, -90, 0));
        interactionManager = GetComponent<InteractionManager>();
    }

    public void OnEnable()
    {
        interactionManager.OnInteract += OpenDoor;
    }

    public void OnDisable()
    {
        interactionManager.OnInteract -= OpenDoor;
    }

    public bool OpenDoor(int i)
    {
        if (transform.localRotation == closedPos)
        {
            transform.localRotation = openPos;
        }
        else
        {
            transform.localRotation = closedPos;
        }

        return true;
    }
}
