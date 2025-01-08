using Photon.Pun;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    // Time in seconds before the object is destroyed
    public float lifetime = 3f;
    float counter = 0;
    private void Update()
    {
        if (counter > lifetime)
        {

            PhotonNetwork.Destroy(GetComponent<PhotonView>());

        }
        else
        {
            counter += Time.deltaTime;
        }
    }
}
