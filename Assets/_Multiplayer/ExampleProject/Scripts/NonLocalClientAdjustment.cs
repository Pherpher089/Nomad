using Photon.Pun;
using UnityEngine;

public class NonLocalClientAdjustment : MonoBehaviour
{
    PhotonView pv;
    // Start is called before the first frame update
    void Start()
    {
        pv = GetComponent<PhotonView>();
        if (!pv.IsMine)
        {
            Rigidbody rb = GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = true;
                GetComponent<CharacterStats>().enabled = false;
                GetComponent<ActorEquipment>().enabled = false;
                GetComponent<HealthManager>().enabled = false;
                GetComponent<ThirdPersonUserControl>().enabled = false;
                GetComponent<ActorInteraction>().enabled = false;
                GetComponent<CharacterManager>().enabled = false;
                GetComponent<BuilderManager>().enabled = false;
                GetComponent<ActorAudioManager>().enabled = false;
                GetComponentInChildren<TheseHands>().enabled = false;
            }
        }
    }
}
