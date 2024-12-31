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
        interactionManager.OnInteract += DoorInteract;
    }

    public void OnDisable()
    {
        interactionManager.OnInteract -= DoorInteract;
    }
    public bool DoorInteract(GameObject i)
    {
        LevelManager.Instance.CallOpenDoorPRC(GetComponentInParent<SourceObject>().id);
        return true;
    }
    public void OpenDoor()
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

        LevelManager.Instance.UpdateGraphForNewStructure(childDoor);
    }
}
