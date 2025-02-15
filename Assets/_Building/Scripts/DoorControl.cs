using UnityEngine;

public class DoorControl : InteractionManager
{
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
        if (gameObject.transform.parent.TryGetComponent<SourceObject>(out var sourceObject))
        {
            LevelManager.Instance.CallOpenDoorSourceObjectPRC(sourceObject.id);
        }
        else if (gameObject.transform.parent.TryGetComponent<BuildingMaterial>(out var buildingMaterial))
        {
            LevelManager.Instance.CallOpenDoorBuildingMaterialPRC(buildingMaterial.id);

        }
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

        LevelManager.Instance.UpdateGraphForNewStructure(this.gameObject);
    }
}
