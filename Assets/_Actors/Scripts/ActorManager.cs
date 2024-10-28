using Photon.Pun;
using Unity.Properties;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum ActorState { Alive, Dead }
public class ActorManager : ObjectManager
{
    public ActorState actorState;
    [HideInInspector] public GameStateManager m_GameStateManager;
    public bool inBuilding;
    HealthManager m_HealthManager;
    HungerManager m_HungerManager;
    public GameObject currentBuildingObj;

    //GenerateLevel levelMaster;
    public ItemManager m_ItemManager;
    [HideInInspector] public ActorEquipment equipment;
    public bool isDead = false;
    PhotonView pv;
    // A string for file Path

    public virtual void Awake()
    {
        if (SceneManager.GetActiveScene().name.Contains("LoadingScene")) return;
        //This overrides the Awake in object manager. Not sure we use that class at the moment. 
        pv = GetComponent<PhotonView>();
        m_HealthManager = GetComponent<HealthManager>();
        m_HungerManager = GetComponent<HungerManager>();
        m_GameStateManager = GameObject.FindWithTag("GameController").GetComponent<GameStateManager>();
        m_ItemManager = GameObject.FindWithTag("GameController").GetComponent<ItemManager>();
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
                if (!isDead) Kill();
                break;

            default:
                break;
        }
    }

    public void Revive()
    {
        Animator animator = transform.GetChild(0).gameObject.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("Kill", false);
        }

        if (tag == "DeadPlayer")
        {
            GetComponent<Rigidbody>().isKinematic = false;
            m_HealthManager.health = m_HealthManager.maxHealth;
            m_HealthManager.dead = false;
            m_HungerManager.stats.stomachValue = m_HungerManager.stats.stomachCapacity;
            pv.RPC("ChangeTag", RpcTarget.All, pv.ViewID, "Player");
            try
            {
                GetComponent<CharacterManager>().SaveCharacter();
            }
            catch
            {
                //This is for testing the same player locally. 
            }
        }
        isDead = false;
        actorState = ActorState.Alive;
    }

    public void Kill()
    {
        Animator animator = transform.GetChild(0).gameObject.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("Kill", true);
        }
        GetComponent<Rigidbody>().isKinematic = true;
        actorState = ActorState.Dead;
        if (tag == "Player")
        {
            GetComponent<CharacterManager>().SaveCharacter();
            FindObjectOfType<PlayersManager>().DeathUpdate(GetComponent<ThirdPersonUserControl>());
            pv.RPC("ChangeTag", RpcTarget.All, pv.ViewID, "DeadPlayer");
        }
        if (tag == "Enemy")
        {
            transform.GetChild(0).GetComponent<Collider>().isTrigger = true;
        }
        isDead = true;
    }
    [PunRPC]
    public void ChangeTag(int pvId, string tag)
    {
        Debug.Log($"### changing tag to {tag}");
        PhotonView photonView = PhotonView.Find(pvId);
        if (photonView != null)
        {
            photonView.gameObject.tag = tag;
        }
        else
        {
            Debug.LogWarning("PhotonView with the given ID was not found.");
        }
        PlayersManager.Instance.CheckForDeath();
    }
    private void CheckCharacterHealth()
    {
        if (m_HealthManager.dead)
        {
            actorState = ActorState.Dead;
        }
    }
}
