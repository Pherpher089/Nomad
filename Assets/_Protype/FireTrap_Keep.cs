using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.ParticleSystemJobs;

public class FireTrap_Keep : MonoBehaviour
{
    // Start is called before the first frame update
    bool IsPlaying = false;
    private float time = 0.0f;
    public float TrapTriggerTimer = 5;
    [SerializeField] ParticleSystem FX_FlameThrower_01;

    void Start()
    {
        //StartCoroutine(TrapTimerCoroutine());

    }

    /*
    IEnumerator TrapTimerCoroutine()
    {
        //Print first time called timestamp
        Debug.Log("Started Corutine at timestamp:" + Time.time);
        //instruct to play after x seconds
        yield return new WaitForSeconds(5);
        if (IsPlaying)
        {
            GetComponent<VisualEffect>().Play();
        }
        else
        {
            GetComponent<VisualEffect>().Stop();
        }

        //print timestamp after x seconds
        Debug.Log("Finished Coroutine at timestamp: " + Time.time);

    } 

    */

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        if (time > 5)
        {
            //time = 0.0f;
            if (IsPlaying)
            {
                GetComponent<ParticleSystem>().Play();
            }
            // GetComponent<VisualEffect>().Play();
            //GetComponent<ParticleSystem>().Play();
            //FX_FlameThrower_01.Play();
       
            else
                {
                //GetComponent<VisualEffect>().Stop();
                GetComponent<ParticleSystem>().Stop();
                //FX_FlameThrower_01.Stop();
                }
            IsPlaying = !IsPlaying;
            time = 0.0f;
        }

    }
}
