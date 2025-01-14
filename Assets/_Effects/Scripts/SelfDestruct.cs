using Photon.Pun;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    // Time in seconds before the object is destroyed
    public float lifetime = 3f;
    float counter = 0;
    PhotonView pv;
    private void Start()
    {
        pv = GetComponent<PhotonView>();
    }
    private void Update()
    {
        if (counter > lifetime)
        {
            if (pv.IsMine)
            {
                PhotonNetwork.Destroy(GetComponent<PhotonView>());
            }

        }
        else
        {
            counter += Time.deltaTime;
        }
    }
}
