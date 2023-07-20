using UnityEngine;

public class ObjectBuildController : MonoBehaviour
{
    public ThirdPersonUserControl player;
    public Vector2 itemIndexRange;
    public int itemIndex = 0;
    float groundHeight = 0;
    bool leftBuildCooldown = false;
    bool rightBuildCooldown = false;
    float deadZone = 0.3f;
    float moveDistance = 0.5f;
    bool cycleCoolDown = false;
    Transform terrainParent;
    // Start is called before the first frame update
    void Start()
    {
        terrainParent = CheckGroundStatus();
        SnapPosToGrid(moveDistance);
    }

    // Update is called once per frame
    void Update()
    {
        if (player)
        {
            terrainParent = CheckGroundStatus();
            if (terrainParent != null)
                Debug.Log("terrain Parent " + terrainParent.gameObject.name);
            float h = Input.GetAxis(player.playerPrefix + "Horizontal");
            float v = Input.GetAxis(player.playerPrefix + "Vertical");
            float hr = Input.GetAxis(player.playerPrefix + "RightStickX");
            float vr = Input.GetAxis(player.playerPrefix + "RightStickY");
            // Handle Input cool down
            if (leftBuildCooldown && v == 0 && h == 0)
            {
                leftBuildCooldown = false;
            }
            if (rightBuildCooldown && vr == 0 && hr == 0)
            {
                rightBuildCooldown = false;
            }


            //check for player to handle input accordingly
            if (player.playerPrefix == "sp")
            { // if mouse and keyboard
                if (Input.GetButtonDown(player.playerPrefix + "Horizontal") || Input.GetButtonDown(player.playerPrefix + "Vertical"))
                {
                    Move(h, 0, v);
                }
            }
            else
            { //if game pad
                if (!leftBuildCooldown && v + h != 0)
                {
                    Move(h, 0, v);
                    leftBuildCooldown = true;
                }
            }

            if (player.playerPrefix == "sp")
            { // if mouse and keyboard
                if (Input.GetAxis("Mouse ScrollWheel") != 0)
                {
                    Move(0, Input.GetAxis("Mouse ScrollWheel"), 0);
                }
            }
            else
            { //if game pad
                if (!rightBuildCooldown && (vr < -deadZone || vr > deadZone))
                {
                    Move(0, vr, 0);
                    rightBuildCooldown = true;
                }
            }

            if (player.playerPrefix == "sp")
            {

                if (Input.GetButtonDown(player.playerPrefix + "Grab"))
                {
                    int l = Input.GetButtonDown(player.playerPrefix + "Grab") ? 1 : 0;
                    Rotate(1);
                }
            }
            else
            {
                if (!rightBuildCooldown && (hr < -deadZone || hr > deadZone))
                {
                    int rot = hr > 0 ? 1 : -1;
                    Rotate(rot);
                    rightBuildCooldown = true;
                }
            }

            if (Input.GetButtonDown(player.playerPrefix + "Fire1") || Input.GetAxis(player.playerPrefix + "Fire1") > 0)
            {
                if (transform.GetChild(itemIndex).GetComponent<BuildingObject>().isValidPlacement)
                {
                    player.gameObject.GetComponent<BuilderManager>().isBuilding = false;
                    player.GetComponent<ActorEquipment>().SpendItem();
                    GameObject buildPiece;
                    for (int i = 0; i < gameObject.transform.childCount; i++)
                    {
                        if (gameObject.transform.GetChild(i).gameObject.activeSelf == true)
                        {
                            buildPiece = gameObject.transform.GetChild(i).gameObject;
                            buildPiece.transform.SetParent(terrainParent);
                            LevelManager.Instance.UpdateSaveData(terrainParent.gameObject.GetComponent<TerrainChunkRef>().terrainChunk, buildPiece.GetComponent<SourceObject>().prefabIndex, buildPiece.GetComponent<SourceObject>().id, false, buildPiece.transform.position, buildPiece.transform.rotation.eulerAngles, false);
                        }
                    }
                    transform.GetChild(itemIndex).GetComponent<BuildingObject>().isPlaced = true;
                    Destroy(this);
                }
            }
            if (player.playerPrefix != "sp")
            {
                if (Input.GetAxis(player.playerPrefix + "Fire2") > 0 && !cycleCoolDown)
                {
                    CycleBuildPiece();
                    cycleCoolDown = true;
                }

                if (Input.GetAxis(player.playerPrefix + "Fire2") == 0 && cycleCoolDown)
                {
                    cycleCoolDown = false;
                }
            }
            else
            {
                if (Input.GetButtonDown(player.playerPrefix + "Fire2"))
                {
                    CycleBuildPiece();
                }
            }
        }
    }

    Transform CheckGroundStatus()
    {
        RaycastHit[] hitInfo = Physics.RaycastAll(transform.position + (Vector3.up * 4), Vector3.down);
        foreach (RaycastHit hit in hitInfo)
        {

            if (hit.collider.gameObject.name == "Terrain Chunk")
            {
                return hit.collider.transform;
            }
        }
        return null;

    }

    public void CycleBuildPiece()
    {
        // Get the list of children
        transform.GetChild(itemIndex).gameObject.SetActive(false);
        if (itemIndex + 1 < itemIndexRange.y)
        {
            transform.GetChild(itemIndex + 1).gameObject.SetActive(true);
            itemIndex = itemIndex + 1;
        }
        else
        {
            transform.GetChild((int)itemIndexRange.x).gameObject.SetActive(true);
            itemIndex = (int)itemIndexRange.x;
        }
        player.lastBuildIndex = itemIndex;
    }

    public void CycleBuildPieceToIndex(int index)
    {
        if (index > itemIndexRange.y || index < itemIndexRange.x)
        {
            Debug.LogError("Build piece index out of range");
            index = (int)itemIndexRange.x;
        }
        // Get the list of children
        transform.GetChild(itemIndex).gameObject.SetActive(false);

        transform.GetChild(index).gameObject.SetActive(true);
        itemIndex = index;

        CycleBuildPieceToIndex(player.lastBuildIndex);
    }
    void Rotate(int dir)
    {

        transform.rotation *= Quaternion.AngleAxis(90 * dir, Vector3.up);
        player.lastBuildRotation = transform.rotation;
    }
    void Move(float x, float y, float z)
    {
        Vector3 targetPos = Vector3.zero;

        if (x < 0)
        {
            targetPos.x = -1 * moveDistance;
        }
        else if (x > 0)
        {
            targetPos.x = 1 * moveDistance;
        }
        if (y < 0)
        {
            targetPos.y = -1 * moveDistance;
        }
        else if (y > 0)
        {
            targetPos.y = 1 * moveDistance;
        }

        if (z < 0)
        {
            targetPos.z = -1 * moveDistance;
        }
        else if (z > 0)
        {
            targetPos.z = 1 * moveDistance;
        }
        transform.position = new Vector3(transform.position.x + targetPos.x, transform.position.y + targetPos.y, transform.position.z + targetPos.z);
        SnapPosToGrid(moveDistance);
    }
    void SnapPosToGrid(float snapValue)
    {
        float x = Mathf.Round(transform.position.x / snapValue) * snapValue;
        float y = Mathf.Round(transform.position.y / snapValue) * snapValue;
        float z = Mathf.Round(transform.position.z / snapValue) * snapValue;
        transform.position = new Vector3(x, y, z);
        player.lastBuildPosition = transform.position;
    }

}
