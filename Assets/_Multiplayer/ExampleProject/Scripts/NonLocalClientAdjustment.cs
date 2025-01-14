using Photon.Pun;
using UnityEngine;

public class NonLocalClientAdjustment : MonoBehaviour
{
    PhotonView pv;
    // Start is called before the first frame update
    void Awake()
    {
        pv = GetComponent<PhotonView>();
        if (!pv.IsMine)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            transform.GetChild(transform.childCount - 1).gameObject.SetActive(false);
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
                GetComponent<AttackManager>().enabled = false;
                TheseHands[] theseHands = GetComponentsInChildren<TheseHands>();

                foreach (TheseHands hands in theseHands)
                {
                    hands.enabled = false;
                }
            }
        }
    }
}
