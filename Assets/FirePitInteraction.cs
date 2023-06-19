using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirePitInteraction : MonoBehaviour
{
    InteractionManager interactionManager;
    AudioManager audioManager;
    GameStateManager gameController;
    public Light fireLight;
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
        fireLight = transform.GetChild(1).gameObject.GetComponent<Light>();
        fireEffect = transform.GetChild(2).gameObject.GetComponent<ParticleSystem>();
        logBurnCounter = logBurnRate;
        isBurning = false;
        logSocket.SetActive(false);
        fireEffect.Stop();
        fireLight.enabled = false;
    }
    void Update()
    {
        Burning();
        FlickerLight();
    }

    //manages fule consumption and turns off fire when fuel is 0
    private void Burning()
    {
        if (logs >= 0)
        {
            logBurnCounter -= Time.deltaTime;
            if (logBurnCounter <= 0)
            {
                if (logs <= 0)
                {
                    logs = 0;
                    return;
                }
                logs--;
                if (logs > 0)
                {
                    logBurnCounter = logBurnRate;
                }
            }
        }
        else
        {
            if (isBurning)
            {
                isBurning = false;
                logSocket.SetActive(false);
                fireEffect.Stop();
                fireLight.enabled = false;
            }
        }
    }

    private void FlickerLight()
    {
        if (isBurning)
        {
            // Flicker effect by slightly randomizing the light intensity.
            float randomIntensity = fireLight.intensity * (0.9f + Mathf.PerlinNoise(Time.time, 0.0f) * 0.2f);
            fireLight.intensity = randomIntensity;
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
                logSocket.SetActive(true);
                fireEffect.Play();
                fireLight.enabled = true;
                isBurning = true;
            }
            Instantiate(stokeEffect, transform.position, transform.rotation);
            gameController.currentRespawnPoint = transform.position + Vector3.up;
            LevelManager.SaveLevel(transform.position + Vector3.up);
        }
        return true;
    }

}
