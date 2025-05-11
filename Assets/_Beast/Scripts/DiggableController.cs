
using System.Collections;
using Photon.Pun;
using UnityEngine;

public class DiggableController : MonoBehaviour
{
    public Transform diggableTransform;
    Transform digSiteTransform;
    public float desiredDisplayHeight;
    public float desiredHHeight;
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
        digSiteTransform = transform.GetChild(transform.childCount - 1);
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
        if (isRestoration && !GameStateManager.Instance.isRaid)
        {
            GameStateManager.Instance.StartRaid(transform, requiredDigTime);
        }
        if (timer >= requiredDigTime)
        {
            isDigComplete = true;
            photonView.RPC("StartRiseRPC", RpcTarget.All);
            GameStateManager.Instance.EndRaid();

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

    public void QuickCompleteDig()
    {
        hasBeenDug = true;
        isDigComplete = true;
        diggableTransform.position = new Vector3(diggableTransform.localPosition.x, desiredDisplayHeight, diggableTransform.localPosition.z);
        if (isRestoration)
        {
            digSiteTransform.position = new Vector3(digSiteTransform.localPosition.x, digSiteTransform.localPosition.y - 50, digSiteTransform.localPosition.z);
        }
    }

    IEnumerator RaiseObject()
    {
        while (diggableTransform.localPosition.y < desiredDisplayHeight)
        {
            diggableTransform.position += Vector3.up * Time.deltaTime * 0.5f;
            digSiteTransform.position -= Vector3.up * Time.deltaTime * 8f;
            yield return null;
        }
        hasBeenDug = true;
        BeastManager.Instance.digTarget = null;
        if (isRestoration)
        {
            GetComponent<RestorationSiteUIController>().SaveRestorationState();
        }

        digParticleSystem.Stop();
    }
}
