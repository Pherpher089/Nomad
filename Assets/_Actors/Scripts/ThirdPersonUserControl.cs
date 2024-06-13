using UnityEngine;
using Photon.Pun;
using System;

public enum PlayerNumber { Player_1 = 1, Player_2 = 2, Player_3 = 3, Player_4 = 4, Single_Player = 0 }
[RequireComponent(typeof(ThirdPersonCharacter))]
public class ThirdPersonUserControl : MonoBehaviour
{
    public string characterName = "New Character";
    public PlayerNumber playerNum;
    [HideInInspector] public string playerPrefix;
    private ThirdPersonCharacter m_Character;           // A reference to the ThirdPersonCharacter on the object
    private Rigidbody m_Rigidbody;                      // A reference to the Rigidbody on the object
    public ActorEquipment actorEquipment;              // A reference to the ActorEquipment on the object
    private ActorInteraction actorInteraction;          // A reference to the ActorInteractionManager on this object
    private PlayerInventoryManager inventoryManager;
    private CharacterManager characterManager;
    private BuilderManager builderManager;
    HUDControl hudControl;
    private Transform m_Cam;                            // A reference to the main camera in the scenes transform
    private Vector3 m_CamForward;                       // The current forward direction of the camera
    private Vector3 m_Move;                             // The direction of movement as given by the input
    private bool m_Jump;                             // the world-relative desired move direction, calculated from the camForward and user input.
    private Vector3 m_Direction;
    [SerializeField] public int playerPos;              // Tracks the position of the player. Player 1, Player 2 ect
    bool uiReturn = false;                              //Tracks the return of the input axis because they are not boolean input
    Animator m_Animator;
    public Vector3 MoveDebug;
    bool m_Crouch = false;
    bool m_Sprint = false;
    bool m_Roll = false;
    public int lastBuildIndex = 0;
    public Vector3 lastLastBuildPosition;
    public Vector3 lastBuildPosition;
    public Quaternion lastBuildRotation;
    bool primaryDown, secondaryDown = false;
    public bool online;
    PhotonView pv;
    PlayerManager playerManager;
    public bool initialized = false;
    public bool cargoUI = false;
    public bool craftingBenchUI = false;

    public bool chestUI = false;
    public bool infoPromptUI = false;
    public bool usingUI;
    public float inventoryControlDeadZone = 0.01f;

    void Awake()
    {
        if (online)
        {
            pv = GetComponent<PhotonView>();
            playerManager = PhotonView.Find((int)pv.InstantiationData[0]).GetComponent<PlayerManager>();
        }
        if (pv.IsMine)
        {
            characterManager = GetComponent<CharacterManager>();
            // get the transform of the main camera
            m_Cam = GameObject.FindWithTag("MainCamera").transform;
            m_CamForward = m_Cam.forward;
            m_Animator = GetComponentInChildren<Animator>();
            // get the third person character ( this should never be null due to require component )
            m_Character = GetComponent<ThirdPersonCharacter>();
            m_Rigidbody = GetComponent<Rigidbody>();
            actorInteraction = GetComponent<ActorInteraction>();
            inventoryManager = GetComponent<PlayerInventoryManager>();
            builderManager = GetComponent<BuilderManager>();
            hudControl = FindObjectOfType<HUDControl>();
            lastBuildPosition = lastLastBuildPosition = new Vector3((int)transform.position.x, (int)transform.position.y, (int)transform.position.z) + (transform.forward * 2);
            lastBuildRotation = Quaternion.identity;
        }
        actorEquipment = GetComponent<ActorEquipment>();
    }

    public void SetPlayerPrefix(PlayerNumber playerNum)
    {
        switch (playerNum)
        {
            case PlayerNumber.Player_1:
                playerPrefix = "p1";
                playerPos = 1;
                break;

            case PlayerNumber.Player_2:
                playerPrefix = "p2";
                playerPos = 2;
                break;
            case PlayerNumber.Player_3:
                playerPrefix = "p3";
                playerPos = 3;
                break;
            case PlayerNumber.Player_4:
                playerPrefix = "p4";
                playerPos = 4;
                break;
            case PlayerNumber.Single_Player:
                playerPrefix = "sp";
                playerPos = 0;
                break;
            default:
                break;
        }
    }

    private void Update()
    {
        if (!GameStateManager.Instance.initialized) return;
        usingUI = cargoUI || craftingBenchUI || chestUI || transform.GetChild(1).gameObject.activeSelf || infoPromptUI;
        if (m_Character.isRiding)
        {
            if (!usingUI && Input.GetButtonDown(playerPrefix + "Grab"))
            {
                BeastManager.Instance.rideBeast.Ride(this.gameObject);
            }
            if (m_Character.seatNumber == 1)
            {
                float h = Input.GetAxis(playerPrefix + "Horizontal");
                float v = Input.GetAxis(playerPrefix + "Vertical");
                BeastManager.Instance.CallBeastMove(new Vector2(h, v), Input.GetButtonDown(playerPrefix + "Jump"));
            }
        }
        // Gathering weather a UI menu is open or not
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

        // Pausing the game
        else if (Input.GetButtonDown(playerPrefix + "Pause") && !inventoryManager.isActive && !builderManager.isBuilding && !cargoUI && !craftingBenchUI && !chestUI && !infoPromptUI)
        {
            hudControl.EnablePauseScreen(!hudControl.isPaused);
        }

        if (hudControl.isPaused || characterManager.actorState == ActorState.Dead)
        {
            if (playerPrefix == "sp" && Input.GetButtonDown(playerPrefix + "Grab") || Input.GetButtonDown(playerPrefix + "Roll"))
            {
                GameStateManager.Instance.hudControl.OnNextPage();
            }
            if (Input.GetButtonDown(playerPrefix + "Block"))
            {
                GameStateManager.Instance.hudControl.OnPrevPage();
            }
            return;
        }
        if (!hudControl.isPaused && infoPromptUI)
        {
            if (playerPrefix == "sp" && Input.GetButtonDown(playerPrefix + "Grab") || Input.GetButtonDown(playerPrefix + "Roll"))
            {
                InfoRuneController[] _openRunes = FindObjectsOfType<InfoRuneController>();
                foreach (InfoRuneController openRune in _openRunes)
                {
                    if (openRune.fullScreenPrompt)
                    {
                        openRune.OnNextPage();
                        break;
                    }
                }
            }
            if (Input.GetButtonDown(playerPrefix + "Block"))
            {
                InfoRuneController[] _openRunes = FindObjectsOfType<InfoRuneController>();
                foreach (InfoRuneController openRune in _openRunes)
                {
                    if (openRune.fullScreenPrompt)
                    {
                        openRune.OnPrevPage();
                        break;
                    }
                }
            }
            if (playerPrefix == "sp" && Input.GetButtonDown(playerPrefix + "Cancel") || Input.GetButtonDown(playerPrefix + "Build"))
            {
                InfoRuneController[] _openRunes = FindObjectsOfType<InfoRuneController>();
                foreach (InfoRuneController openRune in _openRunes)
                {
                    if (openRune.fullScreenPrompt && openRune.isOpen)
                    {
                        openRune.ShowInfo();
                        break;
                    }
                }
            }
            return;
        }
        //No controls if player is dead
        if (characterManager.actorState == ActorState.Dead)
        {
            return;
        }
        //Resetting attack animation triggers
        m_Animator.ResetTrigger("LeftAttack");
        m_Animator.ResetTrigger("RightAttack");

        //To   
        if (!inventoryManager.isActive && !builderManager.isBuilding && !cargoUI && !craftingBenchUI && !chestUI && !m_Character.isRiding && !infoPromptUI)
        {
            //Play state
            PlayControls();
            //Play State
            GroundedActions();
            if (playerPrefix == "sp")
            {

            }

            float v = Input.GetAxisRaw(playerPrefix + "HotKey1");
            float h = Input.GetAxisRaw(playerPrefix + "HotKey2");
            if (uiReturn && v < inventoryControlDeadZone && h < inventoryControlDeadZone && v > -inventoryControlDeadZone && h > -inventoryControlDeadZone)
            {
                uiReturn = false;
            }

            if (playerPrefix == "sp")
            {
                if (Input.GetButtonDown(playerPrefix + "HotKey1"))
                {
                    actorEquipment.inventoryManager.EquipFromInventory(0);
                }
                if (Input.GetButtonDown(playerPrefix + "HotKey2"))
                {
                    actorEquipment.inventoryManager.EquipFromInventory(1);
                }
                if (Input.GetButtonDown(playerPrefix + "HotKey3"))
                {
                    actorEquipment.inventoryManager.EquipFromInventory(2);
                }
                if (Input.GetButtonDown(playerPrefix + "HotKey4"))
                {
                    actorEquipment.inventoryManager.EquipFromInventory(3);
                }
            }
            else
            {
                if (!uiReturn && v + h != 0)
                {
                    if (h > 0)
                    {
                        actorEquipment.inventoryManager.EquipFromInventory(0);
                    }
                    if (v > 0)
                    {
                        actorEquipment.inventoryManager.EquipFromInventory(1);
                    }
                    if (h < 0)
                    {
                        actorEquipment.inventoryManager.EquipFromInventory(2);
                    }
                    if (v < 0)
                    {
                        actorEquipment.inventoryManager.EquipFromInventory(3);
                    }
                    uiReturn = true;
                }
            }


            if (Input.GetButtonDown(playerPrefix + "Build") && actorEquipment.hasItem && actorEquipment.equippedItem.GetComponent<BuildingMaterial>() != null)
            {
                builderManager.Build(this, actorEquipment.equippedItem.GetComponent<BuildingMaterial>());
                return;
            }
        }
        else if (inventoryManager.isActive && !builderManager.isBuilding && !cargoUI && !craftingBenchUI && !chestUI && m_Character.seatNumber != 1)
        {
            m_Rigidbody.velocity = Vector3.zero;
            //Inventory state
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
            if (Input.GetButtonDown(playerPrefix + "Build"))
            {
                inventoryManager.AddIngredient();
            }
            if (Input.GetButtonDown(playerPrefix + "Crouch"))
            {
                inventoryManager.DropItem();
            }

        }
        else if (builderManager.isBuilding && !cargoUI && !craftingBenchUI && !chestUI && !m_Character.isRiding)
        {
            if (playerPrefix == "sp" && Input.GetButtonDown(playerPrefix + "Cancel") || Input.GetButtonDown(playerPrefix + "Pause"))
            {
                builderManager.CancelBuild(this);
            }
        }
        else if (cargoUI && (Input.GetButtonDown(playerPrefix + "Cancel") || Input.GetButtonDown(playerPrefix + "BackPack")))
        {
            BeastStorageContainerController[] beastCargoInventories = FindObjectsOfType<BeastStorageContainerController>();
            foreach (BeastStorageContainerController im in beastCargoInventories)
            {
                if (im.m_PlayerCurrentlyUsing == this.gameObject)
                {
                    if (im.m_IsOpen)
                    {
                        im.PlayerOpenUI(this.gameObject);
                        return;
                    }
                }
            }
        }
        else if (craftingBenchUI)
        {
            if (playerPrefix == "sp" && Input.GetButtonDown(playerPrefix + "Cancel") || Input.GetButtonDown(playerPrefix + "BackPack"))
            {
                CraftingBenchUIController[] craftingUI = FindObjectsOfType<CraftingBenchUIController>();
                foreach (CraftingBenchUIController im in craftingUI)
                {
                    if (im.playerCurrentlyUsing == this.gameObject)
                    {
                        if (im.isOpen)
                        {
                            im.PlayerOpenUI(this.gameObject);
                            return;
                        }
                    }
                }
                BeastStableCraftingUIController[] saddleCraftingBenchUIs = FindObjectsOfType<BeastStableCraftingUIController>();

                foreach (BeastStableCraftingUIController im in saddleCraftingBenchUIs)
                {
                    if (im.playerCurrentlyUsing == this.gameObject)
                    {
                        if (im.isOpen)
                        {
                            im.PlayerOpenUI(this.gameObject);
                            return;
                        }
                    }
                }
                SaddleStationUIController[] saddleStationUIs = FindObjectsOfType<SaddleStationUIController>();

                foreach (SaddleStationUIController im in saddleStationUIs)
                {
                    if (im.playerCurrentlyUsing == this.gameObject)
                    {
                        if (im.isOpen)
                        {
                            im.PlayerOpenUI(this.gameObject);
                            return;
                        }
                    }
                }
            }
        }
        else if (chestUI && (Input.GetButtonDown(playerPrefix + "Cancel") || Input.GetButtonDown(playerPrefix + "BackPack")))
        {
            ChestController[] craftingUI = FindObjectsOfType<ChestController>();
            foreach (ChestController im in craftingUI)
            {
                if (im.m_PlayerCurrentlyUsing == this.gameObject)
                {
                    if (im.m_IsOpen)
                    {
                        im.PlayerOpenUI(this.gameObject);
                        return;
                    }
                }
            }
        }


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
            //actorEquipment.GrabItem();
        }
        if (Input.GetButtonUp(playerPrefix + "Grab"))
        {
            actorInteraction.HoldRaycastInteraction(false);
            actorEquipment.GrabItem();
        }
    }

    private void PlayControls()
    {
        // read inputs
        float h = Input.GetAxis(playerPrefix + "Horizontal");
        float v = Input.GetAxis(playerPrefix + "Vertical");

        bool hasRangeWeapon = false;
        bool throwing = false;
        if (actorEquipment.hasItem && actorEquipment.equippedItem != null)
        {
            hasRangeWeapon = actorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 18 || actorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 13 || actorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 49 || actorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 50;
        }

        if (actorEquipment.hasItem && actorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 50)
        {
            throwing = true;
        }

        // Gathering look direction input
        if (playerNum == PlayerNumber.Single_Player)
        {
            int layerMask = LayerMask.GetMask("MousePlane");
            Ray mCamRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(mCamRay.origin, mCamRay.direction, out hit, 100, layerMask))
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
        if (!FindObjectOfType<HUDControl>().isPaused)
        {

            m_Jump = Input.GetButtonDown(playerPrefix + "Jump");
        }
        else
        {
            m_Jump = false;
        }

        if (!FindObjectOfType<HUDControl>().isPaused)
        {
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
            m_Roll = false;
        }

        m_Move = new Vector3(h, 0, v);
        bool block = Input.GetButton(playerPrefix + "Block");
        if (playerPrefix != "sp")
        {
            if (Input.GetButtonDown(playerPrefix + "Sprint"))
            {
                if (!m_Sprint)
                {
                    m_Sprint = true;
                    m_Crouch = false;
                }
                else
                {
                    m_Sprint = false;
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
            if (m_Sprint)
            {
                if (v < 0.1f && h < 0.1f && v > -0.1f && h > -0.1f)
                {
                    m_Sprint = false;
                }
                else
                {
                    m_Direction = m_Rigidbody.velocity.normalized;
                }
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

        // pass all parameters to the character control script
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
        if ((actorEquipment != null && actorEquipment.equippedItem != null && actorEquipment.equippedItem.tag == "Tool") || !actorEquipment.hasItem)
        {
            m_Character.Attack(primary, secondary, isAiming, throwing);
        }
        if (actorEquipment != null && actorEquipment.equippedItem != null && actorEquipment.equippedItem.GetComponent<Food>() != null && primary)
        {
            m_Character.Eat();
        }
        m_Character.Move(m_Move, m_Crouch, m_Jump, m_Sprint, block, m_Roll);
        m_Jump = false;
    }
}

