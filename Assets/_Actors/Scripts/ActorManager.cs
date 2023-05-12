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

    public virtual void Start()
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
        Instantiate(deathEffectPrefab, transform.position, transform.rotation);
        GameObject.Destroy(this.gameObject);
    }

    private void CheckCharacterHealth()
    {
        if (healthManager.health <= 0)
        {
            actorState = ActorState.Dead;
        }
    }
}
