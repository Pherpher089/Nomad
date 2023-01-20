using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//TODO investigate and remove this class is it is not used anywhere. Looks like 0 scene usage.
public class BuilderManager : MonoBehaviour
{
    public bool isBuilding = false;
    public GameObject m_buildObject;
    private Dictionary<string, Vector2> materialIndices = new Dictionary<string, Vector2>();
    private GameObject[] m_buildPieces;
    private ThirdPersonCharacter playerCharacterController;
    private int childCount;
    private GameObject currentBuildObject;
    private float buildDistance = 3.5f;
    // Start is called before the first frame update

    void Awake()
    {
        playerCharacterController = GetComponent<ThirdPersonCharacter>();
        childCount = m_buildObject.transform.childCount;
        m_buildPieces = new GameObject[childCount];
        for (int i = 0; i < childCount; i++)
        {
            m_buildPieces[i] = m_buildObject.transform.GetChild(i).gameObject;
        }
        SelectBuildObject(0);
    }
    void Start()
    {
        m_buildObject = (GameObject)Resources.Load("Prefabs/BuilderObject");
        //This appears to be the range of items to cycle through for a given material
        materialIndices.Add("Chopped Logs", new Vector2(0, 4));
    }

    void SelectBuildObject(int index)
    {

        for (int i = 0; i < childCount; i++)
        {
            if (i == index)
            {
                m_buildPieces[i].SetActive(true);
            }
            else
            {
                m_buildPieces[i].SetActive(false);
            }
        }
    }

    public void Build(ThirdPersonUserControl player, Item material)
    {
        if (materialIndices.TryGetValue(material.name, out Vector2 value))
        {
            // Key exists, value is stored in the "value" variable
            isBuilding = true;
            Vector3 position = new Vector3(transform.position.x, 0, transform.position.z) + transform.forward * buildDistance;
            SelectBuildObject((int)value.x);
            // Instantiate the prefab at the calculated position with the same rotation as the player.
            currentBuildObject = Instantiate(m_buildObject, position, Quaternion.identity);
            currentBuildObject.GetComponent<ObjectBuildController>().itemIndex = (int)value.x;
            currentBuildObject.GetComponent<ObjectBuildController>().itemIndexRange = value;
            currentBuildObject.GetComponent<ObjectBuildController>().player = player;
        }
        else
        {
            Debug.Log("**This is not a building material**");
        }

    }

    public void CancelBuild()
    {
        isBuilding = false;
        Destroy(currentBuildObject);

    }

}
