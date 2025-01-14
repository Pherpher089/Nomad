using Photon.Pun;
using UnityEngine;

public class TrailDestructor : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.3f; // Lifetime of the trail

    void Start()
    {
        // Schedule this GameObject for destruction across the network
        if (PhotonNetwork.IsMasterClient)
        {
            Invoke(nameof(DestroyTrail), lifetime);
        }
    }

    private void DestroyTrail()
    {
        if (PhotonNetwork.IsConnected && PhotonView.Get(this).IsMine)
        {
            //PhotonNetwork.Destroy(gameObject);
        }
    }
}
