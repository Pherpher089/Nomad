using UnityEngine;

public enum ActorToSpawn {Player1, Player2, Player3, Player4, Single_Player, Survivor, Enemy };
public class ActorSpawner : MonoBehaviour {

    public ActorToSpawn actorToSpawn;
    GameObject actor;
    private MeshRenderer renderer;

    private void Awake()
    {
        renderer = GetComponent<MeshRenderer>();
        switch (actorToSpawn)
        {
            case ActorToSpawn.Player1:
                actor = Resources.Load("Player_1") as GameObject;
                break;
            case ActorToSpawn.Player2:
                actor = Resources.Load("Player_2") as GameObject;
                break;
            case ActorToSpawn.Player3:
                actor = Resources.Load("Player_3") as GameObject;
                break;
            case ActorToSpawn.Player4:
                actor = Resources.Load("Player_4") as GameObject;
                break;
            case ActorToSpawn.Single_Player:
                actor = Resources.Load("Single_Player") as GameObject;
                break;
            case ActorToSpawn.Survivor:
                actor = Resources.Load("Survivor") as GameObject;

                break;
            case ActorToSpawn.Enemy:
                actor = Resources.Load("Enemy") as GameObject;

                break;
            default:
                break;
        }
    }

    private void Start()
    {
        GameObject newSpwn;
        renderer.enabled = false;
        newSpwn = Instantiate(actor,transform.position, transform.rotation, null);
    }
}
