using UnityEngine;
using Photon.Pun;
using System.Runtime.CompilerServices;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(HealthManager))]
public class BeastManager : MonoBehaviour
{
    Animator m_Animator;
    PhotonView m_PhotonView;
    HealthManager m_HealthManager;
    public bool isCamping = false;

    // Start is called before the first frame update
    void Awake()
    {
        m_Animator = transform.GetChild(0).GetComponent<Animator>();
        m_PhotonView = GetComponent<PhotonView>();
        m_HealthManager = GetComponent<HealthManager>();
    }

    public void Hit()
    {
        m_PhotonView.RPC("SetCamping", RpcTarget.All);
    }

    [PunRPC]
    public void SetCamping()
    {
        isCamping = !isCamping;
        if (PhotonNetwork.IsMasterClient)
        {
            m_Animator.SetBool("Camping", isCamping);
        }
    }
}
