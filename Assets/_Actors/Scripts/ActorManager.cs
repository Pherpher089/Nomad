using UnityEngine;
public enum ActorState { Alive, Dead }
public class ActorManager : ObjectManager
{
    public ActorState actorState;
    [HideInInspector] public GameStateManager m_GameStateManager;
    public bool inBuilding;
    public GameObject currentBuildingObj;
    ThirdPersonUserControl userControl;
    PlayerInventoryManager inventoryManager;
    //GenerateLevel levelMaster;
    public ItemManager m_ItemManager;
    [HideInInspector] public ActorEquipment equipment;
    bool isLoaded = false;
    // A string for file Path

    public virtual void Awake()
    {
        userControl = GetComponent<ThirdPersonUserControl>();
        m_GameStateManager = GameObject.FindWithTag("GameController").GetComponent<GameStateManager>();
        m_ItemManager = GameObject.FindWithTag("GameController").GetComponent<ItemManager>();
        healthManager = GetComponent<HealthManager>();
        equipment = GetComponent<ActorEquipment>();
        actorState = ActorState.Alive;
    }


    public virtual void Update()
    {
        CharacterStateMachine();
    }

    public void CharacterStateMachine()
    {
        switch (actorState)
        {
            case ActorState.Alive:
                CheckCharacterHealth();
                break;

            case ActorState.Dead:
                Kill();
                break;

            default:
                break;
        }
    }

    public void Kill()
    {
        Animator animator = transform.GetChild(0).gameObject.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("Kill", true);
        }
        else
        {
            Instantiate(deathEffectPrefab, transform.position, transform.rotation);
            GameObject.Destroy(this.gameObject);
        }
        if (tag == "Player")
        {
            GetComponent<HealthManager>().health = 10;
            GetComponent<HungerManager>().m_StomachValue = 35;
            GetComponent<CharacterManager>().SaveCharacter();
            FindObjectOfType<PlayersManager>().DeathUpdate(GetComponent<ThirdPersonUserControl>());
        }
    }

    private void CheckCharacterHealth()
    {
        if (healthManager.health <= 0)
        {
            actorState = ActorState.Dead;
        }
    }
}
