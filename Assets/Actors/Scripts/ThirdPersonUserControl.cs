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
    public Vector3 MoveDebug;
    bool m_Crouch = false;
    bool m_Sprint = false;
    public int lastBuildIndex = 0;
    public Vector3 lastLastBuildPosition;
    public Vector3 lastBuildPosition;
    public Quaternion lastBuildRotation;
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

        lastBuildPosition = lastLastBuildPosition = transform.position + (transform.forward * 2);

        lastBuildRotation = Quaternion.identity;
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
                inventoryManager.EquipSelection();
            }
            if (Input.GetButtonDown(playerPrefix + "Build"))
            {
                inventoryManager.Craft();
            }
            if (Input.GetButtonDown(playerPrefix + "Crouch"))
            {
                inventoryManager.DropItem();
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

    private void GroundedActions()
    {
        if (Input.GetButtonDown(playerPrefix + "Grab"))
        {
            actorInteraction.RaycastInteraction();
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
            Debug.DrawRay(mCamRay.origin, mCamRay.direction * 100, Color.red, 2f);
            if (Physics.Raycast(mCamRay.origin, mCamRay.direction, out hit, 100, layerMask))
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



        m_Crouch = Input.GetButton(playerPrefix + "Crouch");
        m_Jump = Input.GetButtonDown(playerPrefix + "Jump");

        m_Move = new Vector3(h, 0, v);

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
            lookVelocity = m_Cam.InverseTransformDirection(lookVelocity);
            m_Character.Turning(lookVelocity);
        }
        MoveDebug = m_Move;
        if (primary || secondary)
        {
            m_Crouch = false;
        }
        m_Character.Move(m_Move, m_Crouch, m_Jump, m_Sprint);
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

