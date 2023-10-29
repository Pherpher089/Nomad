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
    private Vector3 m_CurserWorldPos;                   // Used to gather the curser position in world space for single player mouse and keyboard player control. 
    [SerializeField] public int playerPos;              // Tracks the position of the player. Player 1, Player 2 ect
    bool uiReturn = false;                              //Tracks the return of the input axis because they are not boolean input
    Animator m_Animator;
    public Vector3 MoveDebug;
    bool m_Crouch = false;
    bool m_Sprint = false;
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
        //GetComponent<PlayerInventoryManager>().UpdateButtonPrompts();
    }

    private void Update()
    {
        if (playerPrefix == "sp")
        {
            if (Input.GetButtonDown(playerPrefix + "Cancel") && !inventoryManager.isActive && !builderManager.isBuilding && !cargoUI)
            {
                hudControl.EnablePauseScreen(!hudControl.isPaused);
            }
        }

        if (hudControl.isPaused || characterManager.actorState == ActorState.Dead)
        {
            return;
        }
        m_Animator.ResetTrigger("LeftAttack");
        m_Animator.ResetTrigger("RightAttack");
        if (!inventoryManager.isActive && !builderManager.isBuilding && !cargoUI)
        {
            //Play state
            PlayControls();
            //Play State
            GroundedActions();
            if (actorEquipment.hasItem && actorEquipment.equippedItem.GetComponent<BuildingMaterial>() != null && Input.GetButtonDown(playerPrefix + "Build"))
            {
                builderManager.Build(this, actorEquipment.equippedItem.GetComponent<Item>());
            }
        }
        else if (inventoryManager.isActive && !builderManager.isBuilding && !cargoUI)
        {
            m_Rigidbody.velocity = Vector3.zero;
            //Inventory state
            float v = Input.GetAxisRaw(playerPrefix + "Vertical");
            float h = Input.GetAxisRaw(playerPrefix + "Horizontal");

            if (uiReturn && v < 0.1f && h < 0.1f && v > -0.1f && h > -0.1f)
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

            if (Input.GetButtonDown(playerPrefix + "Grab"))
            {
                inventoryManager.InventoryActionButton();
            }
            if (Input.GetButtonDown(playerPrefix + "Craft"))
            {
                inventoryManager.AddIngredient();
            }
            if (Input.GetButtonDown(playerPrefix + "Crouch"))
            {
                inventoryManager.DropItem();
            }

        }
        else if (builderManager.isBuilding && !cargoUI)
        {
            if (Input.GetButtonDown(playerPrefix + "Cancel"))
            {
                builderManager.CancelBuild(this);
            }
        }
        else if (cargoUI && (Input.GetButtonDown(playerPrefix + "Cancel") || Input.GetButtonDown(playerPrefix + "BackPack")))
        {
            BeastCargoInventoryManager[] beastCargoInventories = FindObjectsOfType<BeastCargoInventoryManager>();
            foreach (BeastCargoInventoryManager im in beastCargoInventories)
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

        if (!builderManager.isBuilding && !cargoUI && Input.GetButtonDown(playerPrefix + "BackPack") && !inventoryManager.isActive)
        {
            inventoryManager.ToggleInventoryUI();
        }
        else if (!builderManager.isBuilding && Input.GetButtonDown(playerPrefix + "Cancel") && inventoryManager.isActive || Input.GetButtonDown(playerPrefix + "BackPack") && !builderManager.isBuilding && inventoryManager.isActive)
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
        if (Input.GetAxisRaw(playerPrefix + "Fire1") > 0 && !primaryDown)
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
            if (Input.GetButton(playerPrefix + "Sprint"))
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

        // pass all parameters to the character control script
        if (playerNum == PlayerNumber.Single_Player || m_Sprint)
        {
            m_Character.Turning(m_Direction, Vector3.up);
        }
        else if (m_Direction != Vector3.zero)
        {

            m_Character.Turning(m_Direction);
        }
        else if (m_Rigidbody.velocity.x != 0 || m_Rigidbody.velocity.z != 0)
        {
            Vector3 lookVelocity = new Vector3(m_Rigidbody.velocity.x, 0, m_Rigidbody.velocity.z);
            lookVelocity = m_Cam.InverseTransformDirection(m_Move);
            m_Character.Turning(m_Move);
        }
        MoveDebug = m_Move;
        if (primary || secondary)
        {
            m_Crouch = false;
        }
        m_Character.Move(m_Move, m_Crouch, m_Jump, m_Sprint, block);
        m_Jump = false;
        if (actorEquipment == null) return;
        if ((actorEquipment != null && actorEquipment.equippedItem != null && actorEquipment.equippedItem.tag == "Tool") || !actorEquipment.hasItem)
        {
            m_Character.Attack(primary, secondary, m_Move);
        }
        if (actorEquipment != null && actorEquipment.equippedItem != null && actorEquipment.equippedItem.GetComponent<Food>() != null && primary)
        {
            m_Character.Eat();
        }
    }
}

