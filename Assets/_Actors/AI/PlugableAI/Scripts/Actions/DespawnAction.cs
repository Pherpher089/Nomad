using Photon.Pun;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Actions/Despawn")]

public class DespawnAction : Action
{
    public override void Act(StateController controller)
    {
        Despawn(controller);

    }
    private void Despawn(StateController controller)
    {
        PhotonNetwork.Destroy(controller.gameObject);
    }

}
