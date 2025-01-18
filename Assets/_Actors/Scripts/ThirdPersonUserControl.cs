using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public enum PlayerNumber { Player_1 = 1, Player_2 = 2, Player_3 = 3, Player_4 = 4, Single_Player = 0 }

[RequireComponent(typeof(ThirdPersonCharacter))]
public class ThirdPersonUserControl : MonoBehaviour
{
    public string characterName = "New Character";
    public PlayerNumber playerNum;
    [HideInInspector] public string playerPrefix;
    private ThirdPersonCharacter m_Character;
    private Rigidbody m_Rigidbody;
    public ActorEquipment actorEquipment;
    private ActorInteraction actorInteraction;
    private PlayerInventoryManager inventoryManager;
    private CharacterManager characterManager;
    private BuilderManager builderManager;
    private HUDControl hudControl;
    private Transform m_Cam;
    private Vector3 m_CamForward;
    private Vector3 m_Move;
    private bool m_Jump;
    private Vector3 m_Direction;
    public int playerPos;
    private bool uiReturn = false;
    private Animator m_Animator;
    public Vector3 MoveDebug;
    private bool m_Crouch = false;
    private bool m_Sprint = false;
    private bool m_Roll = false;
    public int lastBuildIndex = 0;
    public Vector3 lastLastBuildPosition;
    public Vector3 lastBuildPosition;
    public Quaternion lastBuildRotation;
    private bool primaryDown, secondaryDown = false;
    public bool online;
    private PhotonView pv;
    private PlayerManager playerManager;
    public bool initialized = false;
    public bool cargoUI = false;
    public bool craftingBenchUI = false;
    public bool chestUI = false;
    public bool infoPromptUI = false;
    public bool usingUI;
    public float inventoryControlDeadZone = 0.01f;
    public bool quickMode = false;
    [HideInInspector] public int toolBeltIndex;
    GameObject mousePlane;
    bool uiTabControlReturn = false;
    public float lastBuildZoomValue = 1;

    private void Awake()
    {
        m_Character = GetComponent<ThirdPersonCharacter>();
        m_Rigidbody = GetComponent<Rigidbody>();
        actorInteraction = GetComponent<ActorInteraction>();
        inventoryManager = GetComponent<PlayerInventoryManager>();
        builderManager = GetComponent<BuilderManager>();
        hudControl = FindObjectOfType<HUDControl>();
        m_Animator = GetComponentInChildren<Animator>();
        actorEquipment = GetComponent<ActorEquipment>();
        mousePlane = transform.GetChild(transform.childCount - 1).gameObject;
        if (online)
        {
            pv = GetComponent<PhotonView>();
            try
            {
                playerManager = PhotonView.Find((int)pv.InstantiationData[0]).GetComponent<PlayerManager>();
            }
            catch
            {
                playerPrefix = "sp";
                return;
            }
        }
        if (playerPrefix == "sp")
        {
            mousePlane.SetActive(true);
        }
        else
        {
            mousePlane.SetActive(false);
        }
        if (pv.IsMine)
        {
            characterManager = GetComponent<CharacterManager>();
            m_Cam = Camera.main.transform;
            m_CamForward = m_Cam.forward;
            lastBuildPosition = lastLastBuildPosition = new Vector3((int)transform.position.x, (int)transform.position.y, (int)transform.position.z) + (transform.forward * 2);
            lastBuildRotation = Quaternion.identity;
        }

    }

    public void SetPlayerPrefix(PlayerNumber playerNum)
    {
        playerPrefix = playerNum switch
        {
            PlayerNumber.Player_1 => "p1",
            PlayerNumber.Player_2 => "p2",
            PlayerNumber.Player_3 => "p3",
            PlayerNumber.Player_4 => "p4",
            PlayerNumber.Single_Player => "sp",
            _ => playerPrefix
        };
        if (playerPrefix == "sp")
        {
            mousePlane.SetActive(true);
        }
        else
        {
            mousePlane.SetActive(false);
        }
        playerPos = (int)playerNum;
    }

    private void Update()
    {
        if (!quickMode && (!GameStateManager.Instance.initialized || !pv.IsMine)) return;

        usingUI = cargoUI || craftingBenchUI || chestUI || transform.GetChild(1).gameObject.activeSelf || infoPromptUI;

        if (m_Character.isRiding)
        {
            HandleRiding();
            HandleHotKeys();
            HandleCameraZoom(false);
            HandleRotateCamera();
            HandleInventoryState();
            HandleInventoryToggle();
            m_Character.stopMoving = true;
            return;
        }

        if (HandlePause())
        {
            m_Character.stopMoving = true;
            return;
        }

        if (infoPromptUI)
        {
            m_Character.stopMoving = true;
            HandleInfoPromptUI();
            return;
        }

        if (!quickMode && characterManager.actorState == ActorState.Dead) return;

        ResetAttackTriggers();
        HandleInventoryToggle();
        if (!inventoryManager.isActive && !builderManager.isBuilding && !cargoUI && !craftingBenchUI && !chestUI && !m_Character.isRiding && !infoPromptUI)
        {
            m_Character.stopMoving = false;
            PlayControls();
            GroundedActions();
            HandleHotKeys();
            HandleBuild();
            HandleRotateCamera();
        }
        else if (inventoryManager.isActive && !builderManager.isBuilding && !cargoUI && !craftingBenchUI && !chestUI && m_Character.seatNumber != 1)
        {
            m_Character.stopMoving = true;
            HandleInventoryState();
        }
        else if (builderManager.isBuilding && !cargoUI && !craftingBenchUI && !chestUI && !m_Character.isRiding)
        {
            m_Character.stopMoving = true;
            HandleBuilderState();
            HandleRotateCamera();
        }
        else if (cargoUI)
        {
            m_Character.stopMoving = true;
            HandleCargoUI();
        }
        else if (craftingBenchUI)
        {
            m_Character.stopMoving = true;
            HandleCraftingBenchUI();
        }
        else if (chestUI)
        {
            m_Character.stopMoving = true;
            HandleChestUI();
        }

        if (!builderManager.isBuilding)
        {
            HandleCameraZoom(false);
        }
        else
        {
            m_Character.stopMoving = true;
            HandleCameraZoom(true);
        }
    }

    private void HandleCameraZoom(bool isBuilding)
    {
        if (isBuilding)
        {
            CameraControllerPerspective.Instance.UpdateCameraZoom(0, playerPrefix == "sp" && Input.GetKeyDown(KeyCode.BackQuote) || Input.GetButtonDown("Zoom"));
        }
        else
        {
            float scroll = playerPrefix == "sp" ? Input.GetAxis("Mouse ScrollWheel") : 0;
            CameraControllerPerspective.Instance.UpdateCameraZoom(scroll, playerPrefix == "sp" && Input.GetKeyDown(KeyCode.BackQuote) || Input.GetButtonDown("Zoom"));
        }
    }

    private void HandleRiding()
    {
        if (!usingUI && Input.GetButtonDown(playerPrefix + "Grab"))
        {
            BeastManager.Instance.rideBeast.Ride(gameObject);
        }
        if (m_Character.seatNumber == 1)
        {
            float h = Input.GetAxis(playerPrefix + "Horizontal");
            float v = Input.GetAxis(playerPrefix + "Vertical");
            BeastManager.Instance.CallBeastMove(new Vector2(h, v), Input.GetButtonDown(playerPrefix + "Jump"));
        }
    }

    private bool HandlePause()
    {
        if (quickMode) return false;
        if (playerPrefix == "sp")
        {
            if (Input.GetButtonDown(playerPrefix + "Cancel") && !inventoryManager.isActive && !builderManager.isBuilding && !cargoUI && !craftingBenchUI && !chestUI)
            {
                if (GameStateManager.Instance.activeInfoPrompts.Count > 0)
                {
                    GameStateManager.Instance.CloseInfoPrompts();
                }
                else
                {
                    hudControl.EnablePauseScreen(!hudControl.isPaused);
                }
            }
        }
        else if (Input.GetButtonDown(playerPrefix + "Pause") && !inventoryManager.isActive && !builderManager.isBuilding && !cargoUI && !craftingBenchUI && !chestUI && !infoPromptUI)
        {
            hudControl.EnablePauseScreen(!hudControl.isPaused);
        }

        if (hudControl.isPaused || characterManager.actorState == ActorState.Dead)
        {
            HandlePauseScreenNavigation();
            return true;
        }

        return false;
    }

    private void HandlePauseScreenNavigation()
    {
        if (Input.GetButtonDown(playerPrefix + "Roll") || playerPrefix == "sp" && (Input.GetButtonDown(playerPrefix + "Grab")))
        {
            GameStateManager.Instance.hudControl.OnNextPage();
        }
        if (Input.GetButtonDown(playerPrefix + "Block"))
        {
            GameStateManager.Instance.hudControl.OnPrevPage();
        }
        if (playerPrefix != "sp")
        {
            if (!uiTabControlReturn)
            {
                if (Input.GetAxisRaw(playerPrefix + "HotKey2") == 1 && playerPrefix != "sp")
                {
                    GameStateManager.Instance.hudControl.OnTabUp();
                    uiTabControlReturn = true;
                }
                if (Input.GetAxisRaw(playerPrefix + "HotKey2") == -1 && playerPrefix != "sp")
                {
                    GameStateManager.Instance.hudControl.OnTabDown();
                    uiTabControlReturn = true;
                }
            }
            else
            {
                if (Input.GetAxisRaw(playerPrefix + "HotKey2") == 0)
                {
                    uiTabControlReturn = false;
                }
            }
        }

    }

    private void HandleInfoPromptUI()
    {
        if (Input.GetButtonDown(playerPrefix + "Roll") || (playerPrefix == "sp" && Input.GetButtonDown(playerPrefix + "Grab")))
        {
            List<InfoRuneController> openRunes = GameStateManager.Instance.activeInfoPrompts;
            foreach (InfoRuneController openRune in openRunes)
            {
                if (openRune.fullScreenPrompt && openRune.isOpen)
                {
                    openRune.OnNextPage();
                    break;
                }
            }
        }
        if (Input.GetButtonDown(playerPrefix + "Block"))
        {
            List<InfoRuneController> openRunes = GameStateManager.Instance.activeInfoPrompts;
            foreach (InfoRuneController openRune in openRunes)
            {
                if (openRune.fullScreenPrompt && openRune.isOpen)
                {
                    openRune.OnPrevPage();
                    break;
                }
            }
        }
        if ((playerPrefix == "sp" && Input.GetButtonDown(playerPrefix + "Cancel")) || Input.GetButtonDown(playerPrefix + "Build"))
        {
            List<InfoRuneController> openRunes = GameStateManager.Instance.activeInfoPrompts;
            foreach (InfoRuneController openRune in openRunes)
            {
                if (openRune.fullScreenPrompt && openRune.isOpen)
                {
                    openRune.ShowInfo(gameObject);
                    break;
                }
            }
        }
    }

    private void ResetAttackTriggers()
    {
        m_Animator.ResetTrigger("LeftAttack");
        m_Animator.ResetTrigger("RightAttack");
    }

    private void HandleHotKeys()
    {
        float v = Input.GetAxisRaw(playerPrefix + "HotKey2");
        float h = Input.GetAxisRaw(playerPrefix + "HotKey1");
        if (uiReturn && v < inventoryControlDeadZone && h < inventoryControlDeadZone && v > -inventoryControlDeadZone && h > -inventoryControlDeadZone)
        {
            uiReturn = false;
        }

        if (playerPrefix == "sp")
        {
            if (Input.GetButtonDown(playerPrefix + "HotKey1")) actorEquipment.inventoryManager.EquipFromToolBelt(0);
            if (Input.GetButtonDown(playerPrefix + "HotKey2")) actorEquipment.inventoryManager.EquipFromToolBelt(1);
            if (Input.GetButtonDown(playerPrefix + "HotKey3")) actorEquipment.inventoryManager.EquipFromToolBelt(2);
            if (Input.GetButtonDown(playerPrefix + "HotKey4")) actorEquipment.inventoryManager.EquipFromToolBelt(3);
        }
        else
        {
            if (!uiReturn && v + h != 0)
            {
                if (h > 0)
                {
                    if (toolBeltIndex < 4)
                    {
                        toolBeltIndex++;
                    }
                    else
                    {
                        toolBeltIndex = 0;
                    }
                    actorEquipment.inventoryManager.EquipFromToolBelt(toolBeltIndex);
                }
                if (h < 0)
                {
                    if (toolBeltIndex > 0)
                    {
                        toolBeltIndex--;
                    }
                    else
                    {
                        toolBeltIndex = 4;
                    }
                    actorEquipment.inventoryManager.EquipFromToolBelt(toolBeltIndex);
                }
                if (v < 0)
                {
                    actorEquipment.inventoryManager.EquipFromToolBelt(toolBeltIndex);
                }
                uiReturn = true;
            }
        }
    }

    private void HandleBuild()
    {
        if (Input.GetButtonDown(playerPrefix + "Build") && actorEquipment.hasItem && actorEquipment.equippedItem.GetComponent<BuildingMaterial>() != null)
        {
            CameraControllerPerspective.Instance.SetCameraForBuild();
            builderManager.Build(this, actorEquipment.equippedItem.GetComponent<BuildingMaterial>());
        }
    }

    private void HandleInventoryState()
    {
        m_Rigidbody.velocity = Vector3.zero;
        float v = Input.GetAxisRaw(playerPrefix + "Vertical");
        float h = Input.GetAxisRaw(playerPrefix + "Horizontal");
        if (uiReturn && v < inventoryControlDeadZone && h < inventoryControlDeadZone && v > -inventoryControlDeadZone && h > -inventoryControlDeadZone)
        {
            uiReturn = false;
        }

        if (playerPrefix == "sp")
        {
            if (Input.GetButtonDown(playerPrefix + "Horizontal") || Input.GetButtonDown(playerPrefix + "Vertical"))
            {
                inventoryManager.MoveSelection(new Vector2(h, v));
            }
        }
        else
        {
            if (!uiReturn && v + h != 0)
            {
                inventoryManager.MoveSelection(new Vector2(h, v));
                uiReturn = true;
            }
        }

        if (Input.GetButtonDown(playerPrefix + "Grab") || Input.GetButtonDown(playerPrefix + "Block"))
        {
            inventoryManager.InventoryActionButton(Input.GetButtonDown(playerPrefix + "Grab"), Input.GetButtonDown(playerPrefix + "Block"));
        }
        if (Input.GetButtonDown(playerPrefix + "Crouch"))
        {
            inventoryManager.DropItem();
        }
    }

    private void HandleBuilderState()
    {
        m_Rigidbody.velocity = Vector3.zero;
        m_Move = Vector3.zero;
        if (Input.GetButtonDown(playerPrefix + "Cancel") || Input.GetButtonDown(playerPrefix + "Pause"))
        {
            builderManager.isBuilding = false;
            builderManager.CancelBuild(this);
        }
    }

    private void HandleCargoUI()
    {
        if (Input.GetButtonDown(playerPrefix + "Cancel") || Input.GetButtonDown(playerPrefix + "BackPack"))
        {
            BeastStorageContainerController[] beastCargoInventories = FindObjectsOfType<BeastStorageContainerController>();
            foreach (BeastStorageContainerController im in beastCargoInventories)
            {
                if (im.m_PlayerCurrentlyUsing == gameObject && im.m_IsOpen)
                {
                    im.PlayerOpenUI(gameObject);
                    return;
                }
            }
        }
    }

    private void HandleCraftingBenchUI()
    {
        if (Input.GetButtonDown(playerPrefix + "Cancel") || Input.GetButtonDown(playerPrefix + "BackPack"))
        {
            CraftingBenchUIController[] craftingUI = FindObjectsOfType<CraftingBenchUIController>();
            foreach (CraftingBenchUIController im in craftingUI)
            {
                if (im.playerCurrentlyUsing == gameObject && im.isOpen)
                {
                    im.PlayerOpenUI(gameObject);
                    return;
                }
            }
            BeastStableCraftingUIController[] saddleCraftingBenchUIs = FindObjectsOfType<BeastStableCraftingUIController>();
            foreach (BeastStableCraftingUIController im in saddleCraftingBenchUIs)
            {
                if (im.playerCurrentlyUsing == gameObject && im.isOpen)
                {
                    im.PlayerOpenUI(gameObject);
                    return;
                }
            }
            SaddleStationUIController[] saddleStationUIs = FindObjectsOfType<SaddleStationUIController>();
            foreach (SaddleStationUIController im in saddleStationUIs)
            {
                if (im.playerCurrentlyUsing == gameObject && im.isOpen)
                {
                    im.PlayerOpenUI(gameObject);
                    return;
                }
            }
        }
    }

    private void HandleChestUI()
    {
        if (Input.GetButtonDown(playerPrefix + "Cancel") || Input.GetButtonDown(playerPrefix + "BackPack"))
        {
            ChestController[] craftingUI = FindObjectsOfType<ChestController>();
            foreach (ChestController im in craftingUI)
            {
                if (im.m_PlayerCurrentlyUsing == gameObject && im.m_IsOpen)
                {
                    im.PlayerOpenUI(gameObject);
                    return;
                }
            }
        }
    }

    private void HandleInventoryToggle()
    {
        if (!builderManager.isBuilding && !cargoUI && !craftingBenchUI && !chestUI && Input.GetButtonDown(playerPrefix + "BackPack") && !inventoryManager.isActive && m_Character.seatNumber != 1)
        {
            inventoryManager.ToggleInventoryUI();
        }
        else if (!builderManager.isBuilding && playerPrefix == "sp" && Input.GetButtonDown(playerPrefix + "Cancel") && inventoryManager.isActive || Input.GetButtonDown(playerPrefix + "BackPack") && !builderManager.isBuilding && inventoryManager.isActive)
        {
            if (inventoryManager.isCrafting)
            {
                inventoryManager.CancelCraft();
            }
            else
            {
                inventoryManager.ToggleInventoryUI();
            }
        }
    }

    private void GroundedActions()
    {
        if (Input.GetButtonDown(playerPrefix + "Grab"))
        {
            actorInteraction.PressRaycastInteraction();
        }
        if (Input.GetButton(playerPrefix + "Grab"))
        {
            actorInteraction.HoldRaycastInteraction(true);
        }
        if (Input.GetButtonUp(playerPrefix + "Grab"))
        {
            actorInteraction.HoldRaycastInteraction(false);
            actorEquipment.GrabItem();
        }
    }

    private void HandleRotateCamera()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            CameraControllerPerspective.Instance.RotateCameraSmoothly();
        }

        if (playerPrefix != "sp" && Input.GetAxisRaw(playerPrefix + "HotKey2") > 0)
        {
            CameraControllerPerspective.Instance.RotateCameraSmoothly();
        }
    }
    private void PlayControls()
    {
        float h = Input.GetAxis(playerPrefix + "Horizontal");
        float v = Input.GetAxis(playerPrefix + "Vertical");

        bool hasRangeWeapon = actorEquipment.hasItem && actorEquipment.equippedItem != null &&
                              (actorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 18 ||
                               actorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 13 ||
                               actorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 49 ||
                               actorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 55 ||
                               actorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 50 ||
                               actorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 83 ||
                               actorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 90);

        bool throwing = actorEquipment.hasItem && actorEquipment.equippedItem != null && actorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 50;

        if (playerNum == PlayerNumber.Single_Player)
        {
            int layerMask = LayerMask.GetMask("MousePlane");
            Ray mCamRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mCamRay, out RaycastHit hit, 100, layerMask))
            {
                m_Direction = new Vector3(hit.point.x, 0, hit.point.z) - transform.position;
            }
        }
        else
        {
            m_Direction = new Vector3(Input.GetAxis(playerPrefix + "RightStickX"), 0, Input.GetAxis(playerPrefix + "RightStickY"));
        }

        bool primary = false;
        bool isAiming = false;
        if (Input.GetAxisRaw(playerPrefix + "Fire1") > 0 && hasRangeWeapon)
        {
            isAiming = true;
        }
        else if (Input.GetAxisRaw(playerPrefix + "Fire1") < 0 && hasRangeWeapon && isAiming)
        {
            isAiming = false;
        }
        else if (Input.GetAxisRaw(playerPrefix + "Fire1") > 0 && !primaryDown)
        {
            primary = true;
            primaryDown = true;
        }
        else if (Input.GetAxisRaw(playerPrefix + "Fire1") <= 0)
        {
            primaryDown = false;
        }

        bool secondary = false;
        if (Input.GetAxisRaw(playerPrefix + "Fire2") > 0 && !secondaryDown)
        {
            secondary = true;
            secondaryDown = true;
        }
        else if (Input.GetAxisRaw(playerPrefix + "Fire2") <= 0)
        {
            secondaryDown = false;
        }

        if (Input.GetButtonDown(playerPrefix + "Crouch"))
        {
            m_Crouch = !m_Crouch;
        }

        if (quickMode || !hudControl.isPaused)
        {
            m_Jump = Input.GetButtonDown(playerPrefix + "Jump");
            if (Input.GetButtonDown(playerPrefix + "Roll") && m_Move != Vector3.zero && !m_Sprint && !m_Animator.GetBool("Rolling"))
            {
                m_Roll = true;
            }
            else
            {
                m_Roll = false;
            }
        }
        else
        {
            m_Jump = false;
            m_Roll = false;
        }

        m_Move = new Vector3(h, 0, v);
        bool block = Input.GetButton(playerPrefix + "Block");

        if (playerPrefix != "sp")
        {
            if (Input.GetButtonDown(playerPrefix + "Sprint"))
            {
                m_Sprint = !m_Sprint;
                if (m_Sprint)
                {
                    m_Crouch = false;
                }
            }
        }
        else
        {
            if (m_Animator.GetBool("Rolling"))
            {
                m_Sprint = false;
                m_Crouch = false;
                m_Direction = m_Rigidbody.velocity.normalized;
            }
            else if (Input.GetButton(playerPrefix + "Sprint"))
            {
                m_Sprint = true;
                m_Crouch = false;
                m_Direction = m_Rigidbody.velocity.normalized;
            }
            else
            {
                m_Sprint = false;
            }
        }

        if (playerPrefix != "sp")
        {
            if (m_Sprint && Mathf.Abs(v) < 0.1f && Mathf.Abs(h) < 0.1f)
            {
                m_Sprint = false;
            }
            else if (m_Sprint)
            {
                m_Direction = m_Rigidbody.velocity.normalized;
            }
        }

        if (primary || secondary || m_Animator.GetBool("Attacking"))
        {
            if (m_Sprint)
            {
                m_Direction = transform.forward;
            }
            m_Crouch = false;
            m_Sprint = false;
        }

        if (playerNum == PlayerNumber.Single_Player || m_Sprint || m_Animator.GetBool("Rolling"))
        {
            m_Character.Turning(m_Direction, Vector3.up);
        }
        else if (m_Direction != Vector3.zero)
        {
            m_Character.Turning(m_Direction);
        }
        else if (m_Rigidbody.velocity.x != 0 || m_Rigidbody.velocity.z != 0)
        {
            m_Character.Turning(m_Move);
        }

        MoveDebug = m_Move;

        if (actorEquipment == null) return;

        if ((actorEquipment.equippedItem != null && actorEquipment.equippedItem.tag == "Tool") || !actorEquipment.hasItem)
        {
            m_Character.Attack(primary, secondary, isAiming, throwing);
        }
        if (actorEquipment.equippedItem != null && actorEquipment.equippedItem.GetComponent<Food>() != null && primary)
        {
            m_Character.Eat();
        }

        m_Character.Move(m_Move, false, m_Jump, m_Sprint, block, m_Roll);
        m_Jump = false;
    }
}
