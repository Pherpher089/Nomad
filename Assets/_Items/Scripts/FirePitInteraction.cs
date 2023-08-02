using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirePitInteraction : MonoBehaviour
{
    InteractionManager interactionManager;
    AudioManager audioManager;
    GameStateManager gameController;
    GameObject fireLight;
    public GameObject stokeEffect;
    public ParticleSystem fireEffect;
    public GameObject logSocket;
    public int logs;
    public int maxLogs;
    public float logBurnRate;
    public float logBurnCounter;
    bool isBurning = false;

    public void Awake()
    {
        stokeEffect = Resources.Load("StokeFireEffect") as GameObject;
        gameController = FindObjectOfType<GameStateManager>();
        audioManager = GetComponent<AudioManager>();
        interactionManager = GetComponent<InteractionManager>();
        logSocket = transform.GetChild(0).gameObject;
        fireLight = transform.GetChild(1).gameObject;
        fireEffect = transform.GetChild(2).gameObject.GetComponent<ParticleSystem>();
        logBurnCounter = 0;
        isBurning = false;
        logSocket.SetActive(false);
        fireEffect.Stop();
        fireLight.SetActive(false);
    }
    void Update()
    {
        Burning();
        FlickerLight();
    }

    //manages fule consumption and turns off fire when fuel is 0
    private void Burning()
    {
        if (isBurning)
        {
            logBurnCounter -= Time.deltaTime;

            if (logBurnCounter <= 0)
            {
                if (logs <= 0)
                {
                    logs = 0;
                    isBurning = false;
                    logSocket.SetActive(false);
                    fireEffect.Stop();
                    fireLight.SetActive(false);
                    return;
                }
                if (logs > 0)
                {
                    logs--;
                    logBurnCounter = logBurnRate;
                }
            }
        }
    }

    private void FlickerLight()
    {
        if (isBurning)
        {
            Light light = fireLight.GetComponent<Light>();

            // The second argument of Mathf.PingPong determines the length of the ping pong, you can adjust it as needed.
            float maxIntensity = 1.0f;
            float minIntensity = 0.7f;
            float speed = 0.5f;

            // PingPong between minIntensity and maxIntensity
            float intensity = Mathf.PingPong(Time.time * speed, maxIntensity - minIntensity) + minIntensity;

            light.intensity = intensity;

        }
    }



    public void OnEnable()
    {
        interactionManager.OnInteract += StokeFire;
    }

    public void OnDisable()
    {
        interactionManager.OnInteract -= StokeFire;
    }

    public bool StokeFire(GameObject i)
    {
        string item = i.GetComponent<ActorEquipment>().equippedItem.GetComponent<Item>().itemName;
        if ((item == "Chopped Logs" || item == "Stick") && logs < maxLogs)
        {
            logs++;
            i.GetComponent<ActorEquipment>().SpendItem();
            if (!isBurning)
            {
                logs--;
                logSocket.SetActive(true);
                fireEffect.Play();
                fireLight.SetActive(true);
                logBurnCounter = logBurnRate;
                isBurning = true;
            }
            Instantiate(stokeEffect, transform.position, transform.rotation);
            gameController.currentRespawnPoint = transform.position + Vector3.up;
            LevelManager.SaveLevel(transform.position + Vector3.up);
        }
        return true;
    }

}
