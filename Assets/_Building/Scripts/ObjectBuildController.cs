using System.Collections.Generic;
using MalbersAnimations;
using Photon.Pun;
using UnityEngine;

public class ObjectBuildController : MonoBehaviour
{
    public ThirdPersonUserControl player;
    public Vector2 itemIndexRange;
    public int currentBuildPieceIndex = 0;
    float groundHeight = 0;
    bool leftBuildCooldown = false;
    bool rightBuildCooldown = false;

    public bool buildCooldown = false;
    float deadZone = 0.3f;
    float moveDistance = 0.5f;
    bool cycleCoolDown = false;
    Transform terrainParent;
    PhotonView pv;
    float moveCounter = 0;
    List<GameObject> objectsInCursor;
    bool is45DegreeMode;
    public CurrentlySelectedBuildPiece currentlySelectedBuildPiece;
    public BuildingObject currentBuildObject;
    public SnappingPoint[] currentSnap = null;
    void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    // Start is called before the first frame update
    void Start()
    {

        currentlySelectedBuildPiece = new();
        terrainParent = CheckGroundStatus();
        CheckRotation();
        currentBuildObject = transform.GetChild(currentBuildPieceIndex).GetComponent<BuildingObject>();
        SnapPosToGrid(moveDistance);
        if (GameStateManager.Instance.currentTent != null)
        {
            GameStateManager.Instance.currentTent.TurnOnBoundsVisuals();
        }
        objectsInCursor = new List<GameObject>();
        GameStateManager.Instance.numberOfBuilders++;
        GameStateManager.Instance.activeBuildPieces.Add(this);
    }
    void OnDestroy()
    {
        GameStateManager.Instance.numberOfBuilders--;
        GameStateManager.Instance.activeBuildPieces.Remove(this);
    }

    private void CheckRotation()
    {
        // Determine the current rotation of the piece
        float currentYRotation = transform.eulerAngles.y;

        // Threshold for determining rotation
        float threshold = 5.0f;

        // Determine if we are in 45-degree mode or 90-degree mode
        if (IsApproximately(currentYRotation, 45.0f, threshold) ||
            IsApproximately(currentYRotation, 135.0f, threshold) ||
            IsApproximately(currentYRotation, 225.0f, threshold) ||
            IsApproximately(currentYRotation, 315.0f, threshold))
        {
            is45DegreeMode = true;
        }
        else
        {
            is45DegreeMode = false;
        }
    }

    bool IsApproximately(float value, float target, float threshold)
    {
        return Mathf.Abs(value - target) <= threshold;
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
            if (buildCooldown && player.playerPrefix != "sp" && Input.GetAxis(player.playerPrefix + "Fire1") < deadZone)
            {
                buildCooldown = false;
            }
            if (buildCooldown && player.playerPrefix == "sp" && Input.GetButtonUp(player.playerPrefix + "Fire1"))
            {
                buildCooldown = false;
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
                    Move(h, 0, v, moveCounter == 0);
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
                    Move(h, 0, v, moveCounter == 0);
                    leftBuildCooldown = true;
                }
            }

            if (player.playerPrefix == "sp")
            { // if mouse and keyboard
                if (Input.GetAxis("Mouse ScrollWheel") != 0)
                {
                    Move(0, Input.GetAxis("Mouse ScrollWheel"), 0, true);
                }
            }
            else
            { //if game pad
                if (!rightBuildCooldown && (vr < -deadZone || vr > deadZone))
                {
                    Move(0, vr, 0, true);
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

            if (Input.GetButtonDown(player.playerPrefix + "Crouch"))
            {
                GameStateManager.Instance.enableBuildSnapping = !GameStateManager.Instance.enableBuildSnapping;
            }

            if ((player.playerPrefix == "sp" && Input.GetButtonDown(player.playerPrefix + "Fire1") && !buildCooldown) || (player.playerPrefix != "sp" && Input.GetAxis(player.playerPrefix + "Fire1") > 0 && !buildCooldown))
            {
                buildCooldown = true;
                if (transform.GetChild(currentBuildPieceIndex).name.Contains("BuilderCursor"))
                {
                    //Collect the item number of that item
                    //find the index of the matching builder object piece
                    // figure out what build range object corresponds to that build piece and apply that to this
                    // RPC destroy the selected build piece
                    GameObject selectedObject = transform.GetChild(currentBuildPieceIndex).GetComponent<BuildingObject>().GetSelectedObject();
                    if (selectedObject != null)
                    {

                        if (selectedObject.TryGetComponent<SourceObject>(out var so))
                        {
                            int index;
                            for (int i = 0; i < transform.childCount; i++)
                            {

                                if (transform.GetChild(i).name + "(Clone)" == so.name)
                                {
                                    index = i;
                                    BuilderManager bm = player.GetComponent<BuilderManager>();
                                    foreach (BuildableItemIndexRange range in bm.materialIndices)
                                    {
                                        if (index >= range.buildableItemIndexRange.x && index < range.buildableItemIndexRange.y)
                                        {
                                            itemIndexRange = range.buildableItemIndexRange;
                                            CycleBuildPieceToIndex(index);
                                            transform.SetPositionAndRotation(so.transform.localToWorldMatrix.GetPosition(), so.transform.rotation);
                                            CheckRotation();
                                            currentlySelectedBuildPiece = new CurrentlySelectedBuildPiece(so.transform.position, so.transform.rotation.eulerAngles, so.environmentListIndex, so.id, true);
                                            LevelManager.Instance.CallShutOffObjectRPC(so.id, false);
                                            return;
                                        }
                                    }
                                }
                            }

                        }
                        if (selectedObject.TryGetComponent<BuildingMaterial>(out var _bm))
                        {
                            int index;
                            for (int i = 0; i < transform.childCount; i++)
                            {
                                if (transform.GetChild(i).name + "(Clone)" == _bm.name)
                                {
                                    index = i;
                                    BuilderManager bm = player.GetComponent<BuilderManager>();
                                    foreach (BuildableItemIndexRange range in bm.materialIndices)
                                    {
                                        if (index >= range.buildableItemIndexRange.x && index < range.buildableItemIndexRange.y)
                                        {
                                            itemIndexRange = range.buildableItemIndexRange;
                                            CycleBuildPieceToIndex(index);
                                            string id = _bm.id;
                                            int underscoreIndex = id.LastIndexOf('_');
                                            string state = id.Substring(underscoreIndex + 1, id.Length - underscoreIndex - 1);
                                            currentlySelectedBuildPiece = new CurrentlySelectedBuildPiece(_bm.transform.position, _bm.transform.rotation.eulerAngles, _bm.itemListIndex, _bm.id, false, state);
                                            LevelManager.Instance.CallShutOffBuildingMaterialRPC(_bm.id, false);
                                            return;
                                        }
                                    }
                                }
                            }

                        }
                    }
                    return;
                }
                else if (transform.GetChild(currentBuildPieceIndex).GetComponent<BuildingObject>().isValidPlacement)
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
                            }
                            else
                            {
                                SourceObject so = buildPiece.GetComponent<SourceObject>();
                                prefabIndex = so.environmentListIndex;
                                id = GenerateObjectId.GenerateSourceObjectId(so);

                            }
                            LevelManager.Instance.CallPlaceObjectPRC(prefabIndex, buildPiece.transform.position, buildPiece.transform.rotation.eulerAngles, id, false);
                            if (currentlySelectedBuildPiece.state != "")
                            {
                                LevelManager.Instance.CallSaveObjectsPRC(id, false, currentlySelectedBuildPiece.state);
                                //update object state ^
                            }
                            if (currentlySelectedBuildPiece.id != "")
                            {
                                if (currentlySelectedBuildPiece.isSourceObject)
                                {
                                    LevelManager.Instance.CallShutOffObjectRPC(currentlySelectedBuildPiece.id);
                                    currentlySelectedBuildPiece = new();
                                }
                                else
                                {
                                    LevelManager.Instance.CallShutOffBuildingMaterialRPC(currentlySelectedBuildPiece.id);
                                    currentlySelectedBuildPiece = new();
                                }

                            }


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
                    if (transform.GetChild(currentBuildPieceIndex).name.Contains("BuilderCursor"))
                    {
                        transform.GetChild(currentBuildPieceIndex).GetComponent<BuildingObject>().CycleSelectedPiece();
                        return;
                    }
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
                    if (transform.GetChild(currentBuildPieceIndex).name.Contains("BuilderCursor"))
                    {
                        transform.GetChild(currentBuildPieceIndex).GetComponent<BuildingObject>().CycleSelectedPiece();
                        return;
                    }
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
        transform.GetChild(currentBuildPieceIndex).gameObject.SetActive(false);
        if (currentBuildPieceIndex + 1 < itemIndexRange.y)
        {
            transform.GetChild(currentBuildPieceIndex + 1).gameObject.SetActive(true);
            currentBuildPieceIndex = currentBuildPieceIndex + 1;
        }
        else
        {
            transform.GetChild((int)itemIndexRange.x).gameObject.SetActive(true);
            currentBuildPieceIndex = (int)itemIndexRange.x;
        }
        currentBuildObject = transform.GetChild(currentBuildPieceIndex).GetComponent<BuildingObject>();
        pv.RPC("UpdateBuildObjectState", RpcTarget.AllBuffered, currentBuildPieceIndex, false);

        player.lastBuildIndex = currentBuildPieceIndex;
    }

    public void CycleBuildPieceToIndex(int index)
    {
        if (index > itemIndexRange.y || index < itemIndexRange.x)
        {
            index = (int)itemIndexRange.x;
        }
        // Get the list of children
        transform.GetChild(currentBuildPieceIndex).gameObject.SetActive(false);

        transform.GetChild(index).gameObject.SetActive(true);
        currentBuildPieceIndex = index;
        currentBuildObject = transform.GetChild(currentBuildPieceIndex).GetComponent<BuildingObject>();
        pv.RPC("UpdateBuildObjectState", RpcTarget.Others, index, false);
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
        currentBuildPieceIndex = _itemIndex;
        if (currentBuildPieceIndex > itemIndexRange.y || currentBuildPieceIndex < itemIndexRange.x)
        {
            _itemIndex = (int)itemIndexRange.x;
        }
        // Get the list of children
        transform.GetChild(_itemIndex).gameObject.SetActive(true);
        currentBuildPieceIndex = _itemIndex;
    }

    void Rotate(int dir)
    {

        transform.rotation *= Quaternion.AngleAxis(45 * dir, Vector3.up);
        player.lastBuildRotation = transform.rotation;
        CheckRotation();
    }
    void Move(float x, float y, float z, bool isKeyDown = false)
    {
        if (moveCounter % 5 != 0)
        {
            x = 0;
            z = 0;
        }

        // Get the camera's forward and right directions, adjusted for the Y-axis rotation (ignoring vertical tilt)
        Vector3 cameraForward = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized;
        Vector3 cameraRight = new Vector3(Camera.main.transform.right.x, 0, Camera.main.transform.right.z).normalized;

        // Calculate the rotated vectors to move at a 45-degree offset relative to the camera's forward direction
        Vector3 forwardLeft = (cameraForward - cameraRight).normalized;  // Forward Left (45 degrees left of forward)
        Vector3 forwardRight = (cameraForward + cameraRight).normalized; // Forward Right (45 degrees right of forward)
        Vector3 backwardLeft = (-cameraForward - cameraRight).normalized; // Backward Left (45 degrees left of backward)
        Vector3 backwardRight = (-cameraForward + cameraRight).normalized; // Backward Right (45 degrees right of backward)

        // Initialize movement direction
        Vector3 moveDirection = Vector3.zero;

        // Apply movement based on input values
        if (x < 0)
        {
            moveDirection += backwardLeft * moveDistance; // Left movement aligned with camera rotation
        }
        else if (x > 0)
        {
            moveDirection += forwardRight * moveDistance; // Right movement aligned with camera rotation
        }

        if (y < 0)
        {
            moveDirection.y -= moveDistance; // Downward movement
        }
        else if (y > 0)
        {
            moveDirection.y += moveDistance; // Upward movement
        }

        if (z < 0)
        {
            moveDirection += backwardRight * moveDistance;  // Down movement aligned with camera rotation
        }
        else if (z > 0)
        {
            moveDirection += forwardLeft * moveDistance; // Up movement aligned with camera rotation
        }

        // Update position and snap to grid
        transform.position += moveDirection;
        if (GameStateManager.Instance.enableBuildSnapping)
        {
            CheckForSnapPointsAndSnap(moveDirection, isKeyDown);
        }
        SnapPosToGrid(moveDistance);
    }

    public void CheckForSnapPointsAndSnap(Vector3 moveDir, bool isKeydown)
    {
        Debug.Log("### checking and snapping");
        if (!currentBuildObject.isSnapped)
        {
            SnappingPoint[] snappingPoints = currentBuildObject.GetOverlappingSnappingPoint();
            if (snappingPoints != null)
            {
                Vector3 offset = currentBuildObject.transform.position - snappingPoints[1].transform.position;
                transform.position = snappingPoints[0].transform.position + offset;
                currentBuildObject.isSnapped = true;
                currentSnap = snappingPoints;
            }
        }
        else
        {
            if (isKeydown)
            {
                currentSnap = null;
            }

            transform.position += moveDir;

            if (currentBuildObject.GetOverlappingSnappingPoint() == null)
            {
                currentBuildObject.isSnapped = false;
            }
        }
        //if snap point
        //transform.position = make the two snap points have the same position;
    }

    public void SnapPosToGrid(float snapValue)
    {
        if (is45DegreeMode)
        {
            // Snap in 45-degree mode - adjust the grid calculation accordingly.
            SnapTo45DegreeGrid(snapValue);
        }
        else
        {
            // Snap in 90-degree mode - standard global grid snapping.
            SnapTo90DegreeGrid(snapValue);
        }
    }

    private void SnapTo90DegreeGrid(float snapValue)
    {
        // Snap to global grid at 90-degree alignment
        float x = Mathf.Round(transform.position.x / snapValue) * snapValue;
        float y = Mathf.Round(transform.position.y / snapValue) * snapValue;
        float z = Mathf.Round(transform.position.z / snapValue) * snapValue;
        transform.position = new Vector3(x, y, z);
        player.lastBuildPosition = transform.position;
    }

    private void SnapTo45DegreeGrid(float snapValue)
    {
        // Snap to 45-degree grid - adjust based on rotation and position
        Vector3 localPosition = transform.position;

        // Calculate snapping based on local rotation being at a 45-degree alignment
        float localX = Mathf.Round(localPosition.x / (snapValue * Mathf.Sqrt(2))) * snapValue * Mathf.Sqrt(2);
        float localZ = Mathf.Round(localPosition.z / (snapValue * Mathf.Sqrt(2))) * snapValue * Mathf.Sqrt(2);
        float y = Mathf.Round(localPosition.y / snapValue) * snapValue;

        // Assign snapped position
        transform.position = new Vector3(localX, y, localZ);
        player.lastBuildPosition = transform.position;
    }

    public class CurrentlySelectedBuildPiece
    {
        public Vector3 curPos;
        public Vector3 curRotEuler;
        public int itemIndex;
        public string id;
        public bool isSourceObject;
        public string state;
        public CurrentlySelectedBuildPiece(Vector3 curPos, Vector3 curRotEuler, int itemIndex, string id, bool isSourceObject, string state = "")
        {
            this.curPos = curPos;
            this.curRotEuler = curRotEuler;
            this.itemIndex = itemIndex;
            this.id = id;
            this.isSourceObject = isSourceObject;
            this.state = state;

        }
        public CurrentlySelectedBuildPiece()
        {
            this.curPos = Vector3.zero;
            this.curRotEuler = Vector3.zero;
            this.itemIndex = -1;
            this.id = "";
            this.isSourceObject = false;
        }
    }

}
