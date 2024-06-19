using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text text;
    [SerializeField] GameObject button;
    Player player;
    public void SetUp(Player _player)
    {
        player = _player;
        text.text = player.NickName;
        if (player.IsMasterClient || !PhotonNetwork.IsMasterClient)
        {
            button.SetActive(false);
        }
    }

    public void KickPlayer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CloseConnection(player);
        }
        else
        {
            //Debug.LogWarning("Only the Master Client can kick players.");
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (player == otherPlayer)
        {
            Destroy(gameObject);
        }
    }

    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }

}
