
using System.Collections;
using Photon.Pun;
using UnityEngine;

public class DiggableController : MonoBehaviour
{
    public Transform diggableTransform;
    public float desiredDisplayHeight;
    public float requiredDigTime = 20f;
    float timer = 0f;
    public bool isRestoration = false;
    bool isDigComplete = false;
    [HideInInspector] public bool hasBeenDug = false;
    public ParticleSystem digParticleSystem;
    PhotonView photonView;

    void Start()
    {
        digParticleSystem = transform.GetChild(isRestoration ? 1 : 0).GetComponent<ParticleSystem>();
        photonView = GetComponent<PhotonView>();
        if (diggableTransform == null)
        {
            Debug.LogError("DiggableTransform is not assigned in the inspector.");
        }
    }
    public void Dig()
    {
        Debug.Log("Digging");
        if (hasBeenDug)
        {
            return;
        }
        timer += Time.deltaTime;
        photonView.RPC("DigRPC", RpcTarget.All);
        if (timer >= requiredDigTime)
        {
            isDigComplete = true;
            photonView.RPC("StartRiseRPC", RpcTarget.All);
        }
    }
    [PunRPC]
    public void DigRPC()
    {
        digParticleSystem.Play();
    }
    [PunRPC]
    public void StartRiseRPC()
    {
        StartCoroutine(RaiseObject());
    }

    IEnumerator RaiseObject()
    {
        while (diggableTransform.localPosition.y < desiredDisplayHeight)
        {
            diggableTransform.position += Vector3.up * Time.deltaTime * 0.5f;
            yield return null;
        }
        hasBeenDug = true;
        BeastManager.Instance.digTarget = null;
        digParticleSystem.Stop();

    }
}
