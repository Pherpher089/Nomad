using UnityEngine;

public class DoorControl : InteractionManager
{
    bool doorState;
    public bool interactinon;
    GameObject childDoor;
    Quaternion closedPos, openPos;
    InteractionManager interactionManager;
    AudioManager audioManager;

    public void Awake()
    {
        audioManager = GetComponent<AudioManager>();
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
            audioManager.PlaySoundEffect(0);
        }
        else
        {
            transform.localRotation = closedPos;
            audioManager.PlaySoundEffect(1);

        }

        return true;
    }
}
