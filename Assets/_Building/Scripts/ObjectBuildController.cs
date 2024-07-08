using Photon.Pun;
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
    PhotonView pv;
    float moveCounter = 0;
    void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    // Start is called before the first frame update
    void Start()
    {
        terrainParent = CheckGroundStatus();
        SnapPosToGrid(moveDistance);
        if (GameStateManager.Instance.currentTent != null)
        {
            GameStateManager.Instance.currentTent.TurnOnBoundsVisuals();
        }
    }

    // Update is called once per frame
    void Update()
    {
        moveCounter++;
        if (player)
        {
            terrainParent = CheckGroundStatus();

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
                    moveCounter = 0;
                }
                if (Input.GetButton(player.playerPrefix + "Horizontal") || Input.GetButton(player.playerPrefix + "Vertical"))
                {
                    Move(h, 0, v);
                }
            }
            else
            { //if game pad
                if (!leftBuildCooldown)
                {
                    moveCounter = 0;
                }
                if (v + h != 0)
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
                    player.gameObject.GetComponent<ThirdPersonCharacter>().wasBuilding = true;
                    //we need to check to see if the player is holding an item that is spent when building
                    //This needs a better solution. If player is holding a torch when placing a 
                    // chest, the torch will be spent.
                    ActorEquipment ac = player.GetComponent<ActorEquipment>();
                    if (ac.equippedItem != null)
                    {
                        Item playerItem = ac.equippedItem.GetComponent<Item>();
                        //is the player holding one of the spendables?
                        if (playerItem.itemListIndex == 1 || playerItem.itemListIndex == 3 || playerItem.itemListIndex == 6)
                        {
                            ac.SpendItem();
                        }
                    }
                    GameObject buildPiece;
                    for (int i = 0; i < gameObject.transform.childCount; i++)
                    {
                        if (gameObject.transform.GetChild(i).gameObject.activeSelf == true)
                        {
                            buildPiece = gameObject.transform.GetChild(i).gameObject;
                            Item itm = buildPiece.GetComponent<Item>();
                            int prefabIndex;
                            bool isItem = false;
                            string id;
                            if (itm != null)
                            {
                                prefabIndex = itm.itemListIndex;
                                isItem = true;
                                id = GenerateObjectId.GenerateItemId(itm);
                                Debug.Log("### item id " + id);
                            }
                            else
                            {
                                SourceObject so = buildPiece.GetComponent<SourceObject>();
                                prefabIndex = so.environmentListIndex;
                                id = GenerateObjectId.GenerateSourceObjectId(so);

                            }
                            LevelManager.Instance.CallPlaceObjectPRC(prefabIndex, buildPiece.transform.position, buildPiece.transform.rotation.eulerAngles, id, false);

                            player.gameObject.GetComponent<BuilderManager>().isBuilding = false;
                            if (GameStateManager.Instance.currentTent != null && FindObjectsOfType<ObjectBuildController>().Length == 1)
                            {
                                GameStateManager.Instance.currentTent.TurnOffBoundsVisuals();
                            }
                            PhotonNetwork.Destroy(pv);
                        }
                    }
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
        pv.RPC("UpdateBuildObjectState", RpcTarget.AllBuffered, itemIndex, false);

        player.lastBuildIndex = itemIndex;
    }

    public void CycleBuildPieceToIndex(int index)
    {
        if (index > itemIndexRange.y || index < itemIndexRange.x)
        {
            index = (int)itemIndexRange.x;
        }
        // Get the list of children
        transform.GetChild(itemIndex).gameObject.SetActive(false);

        transform.GetChild(index).gameObject.SetActive(true);
        itemIndex = index;
        pv.RPC("UpdateBuildObjectState", RpcTarget.Others, index, false);

        CycleBuildPieceToIndex(player.lastBuildIndex);
    }
    [PunRPC]
    void UpdateBuildObjectState(int activeChildIndex, bool isPlaced)
    {
        // Set all children to inactive
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        // Activate the correct child
        GameObject activeChild = transform.GetChild(activeChildIndex).gameObject;
        activeChild.SetActive(true);

        // Set the correct material
        if (isPlaced)
        {
            transform.GetChild(activeChildIndex).GetComponent<BuildingObject>().isPlaced = true;
        }
    }
    public void CallInitializeBuildPicePRC(int _itemIndex, Vector2 _itemIndexRange)
    {
        pv.RPC("InitializeBuildPicePRC", RpcTarget.Others, _itemIndex, _itemIndexRange);
    }
    [PunRPC]
    public void InitializeBuildPicePRC(int _itemIndex, Vector2 _itemIndexRange)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
        itemIndexRange = _itemIndexRange;
        itemIndex = _itemIndex;
        if (itemIndex > itemIndexRange.y || itemIndex < itemIndexRange.x)
        {
            _itemIndex = (int)itemIndexRange.x;
        }
        // Get the list of children
        transform.GetChild(_itemIndex).gameObject.SetActive(true);
        itemIndex = _itemIndex;
    }

    void Rotate(int dir)
    {

        transform.rotation *= Quaternion.AngleAxis(90 * dir, Vector3.up);
        player.lastBuildRotation = transform.rotation;
    }
    void Move(float x, float y, float z)
    {
        if (moveCounter % 5 != 0)
        {
            x = 0;
            z = 0;
        }
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
