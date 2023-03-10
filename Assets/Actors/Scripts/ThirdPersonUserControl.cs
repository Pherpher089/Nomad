using UnityEngine;

public enum PlayerNumber { Player_1, Player_2, Player_3, Player_4, Single_Player }
[RequireComponent(typeof(ThirdPersonCharacter))]
public class ThirdPersonUserControl : MonoBehaviour
{
    public string playerName = "New Character";
    public PlayerNumber playerNum;
    [HideInInspector] public string playerPrefix;
    private ThirdPersonCharacter m_Character;           // A reference to the ThirdPersonCharacter on the object
    private Rigidbody m_Rigidbody;                      // A reference to the Rigidbody on the object
    public ActorEquipment actorEquipment;              // A reference to the ActorEquipment on the object
    private ActorInteraction actorInteraction;          // A reference to the ActorInteractionManager on this object
    private PlayerInventoryManager inventoryManager;
    private BuilderManager builderManager;
    private Transform m_Cam;                            // A reference to the main camera in the scenes transform
    private Vector3 m_CamForward;                       // The current forward direction of the camera
    private Vector3 m_Move;                             // The direction of movement as given by the input
    private bool m_Jump;                             // the world-relative desired move direction, calculated from the camForward and user input.
    private Vector3 m_Direction;
    private Vector3 m_CurserWorldPos;                   // Used to gather the curser position in world space for single player mouse and keyboard player control. 
    [SerializeField] public int playerPos;              // Tracks the position of the player. Player 1, Player 2 ect
    bool uiReturn = false;                              //Tracks the return of the input axis because they are not boolean input
    Animator m_Animator;

    private void Start()
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

        // get the transform of the main camera
        m_Cam = GameObject.FindWithTag("MainCamera").transform;
        m_CamForward = m_Cam.forward;
        m_Animator = GetComponentInChildren<Animator>();
        // get the third person character ( this should never be null due to require component )
        m_Character = GetComponent<ThirdPersonCharacter>();
        m_Rigidbody = GetComponent<Rigidbody>();
        actorEquipment = GetComponent<ActorEquipment>();
        actorInteraction = GetComponent<ActorInteraction>();
        inventoryManager = GetComponent<PlayerInventoryManager>();
        builderManager = GetComponent<BuilderManager>();
    }

    private void Update()
    {
        if (!inventoryManager.isActive && !builderManager.isBuilding)
        {
            //Play state
            PlayControls();
            //Play State
            GroundedActions();
            if (actorEquipment.hasItem && actorEquipment.equippedItem.tag == "BuildingMaterial" && Input.GetButtonDown(playerPrefix + "Build"))
            {
                builderManager.Build(this, actorEquipment.equippedItem.GetComponent<Item>());
            }
        }
        else if (inventoryManager.isActive && !builderManager.isBuilding)
        {
            m_Rigidbody.velocity = Vector3.zero;
            //Inventory state
            float v = Input.GetAxis(playerPrefix + "Vertical");
            float h = Input.GetAxis(playerPrefix + "Horizontal");

            if (uiReturn && v < 0.3f && h < 0.3f && v > -0.3f && h > -0.3f)
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
                inventoryManager.EquipSelection();
            }
            if (Input.GetButtonDown(playerPrefix + "Build"))
            {
                inventoryManager.Craft();
            }

        }
        else if (builderManager.isBuilding)
        {
            if (Input.GetButtonDown(playerPrefix + "Cancel"))
            {
                builderManager.CancelBuild();
            }
        }


        if (!builderManager.isBuilding && Input.GetButtonDown(playerPrefix + "BackPack") && !inventoryManager.isActive)
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

    // Fixed update is called in sync with physics
    private void FixedUpdate()
    {


    }

    private void GroundedActions()
    {
        if (Input.GetButtonDown(playerPrefix + "Grab"))
        {
            Debug.Log("### Interacting");
            actorInteraction.RaycastInteraction(true);
            actorEquipment.GrabItem();
        }
    }

    private void PlayControls()
    {
        // read inputs
        float h;
        float v;
        if (playerNum == PlayerNumber.Single_Player)
        {
            h = Input.GetAxis(playerPrefix + "Horizontal");
            v = Input.GetAxis(playerPrefix + "Vertical");
        }
        else
        {
            h = Input.GetAxis(playerPrefix + "Horizontal");
            v = Input.GetAxis(playerPrefix + "Vertical");
        }

        // Gathering look direction input
        if (playerNum == PlayerNumber.Single_Player)
        {
            Ray mCamRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(mCamRay.origin, mCamRay.direction, out hit))
            {
                m_Direction = new Vector3(hit.point.x, 0, hit.point.z) - transform.position;
            }
        }
        else
        {
            m_Direction = new Vector3(Input.GetAxis(playerPrefix + "RightStickX"), 0, Input.GetAxis(playerPrefix + "RightStickY"));
        }


        bool primary = Input.GetAxisRaw(playerPrefix + "Fire1") > 0 ? true : false;
        bool secondary = Input.GetAxisRaw(playerPrefix + "Fire2") > 0 ? true : false;



        bool crouch = Input.GetButton(playerPrefix + "Crouch");
        m_Jump = Input.GetButtonDown(playerPrefix + "Jump");

        m_Move = new Vector3(h, 0, v);

        // pass all parameters to the character control script
        if (playerNum == PlayerNumber.Single_Player)
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
            lookVelocity = m_Cam.InverseTransformDirection(lookVelocity);
            m_Character.Turning(lookVelocity);
        }
        m_Character.Move(m_Move, crouch, m_Jump);
        m_Jump = false;
        m_Character.Attack(primary, secondary);

    }

    public class PlayerSaveData
    {
        public string playerName;
        public float x;
        public float y;
        public float z;

        public int[] itemIds;
        public int equippedItemId;

        PlayerSaveData(string playerName, float x, float y, float z, int[] itemIds, int equippedItemId)
        {
            this.playerName = playerName;
            this.x = x;
            this.y = y;
            this.z = z;
            this.itemIds = itemIds;
            this.equippedItemId = equippedItemId;
        }
    }

}

