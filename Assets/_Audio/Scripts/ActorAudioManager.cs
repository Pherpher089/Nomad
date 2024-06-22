using System;
using UnityEngine;
using Random = UnityEngine.Random;
public class ActorAudioManager : MonoBehaviour
{
    public AudioClip[] steps;
    public AudioClip[] attacks;
    public AudioClip[] hits;
    public AudioClip[] unarmedBlockedHits;
    public AudioClip[] impacts;

    public AudioClip[] jump;
    public AudioClip[] death;
    public AudioClip[] eat;
    public AudioClip[] dropItem;

    [Range(0, 1)] public float m_Volume;



    public AudioSource sfxSource;

    void Awake()
    {
        sfxSource = gameObject.GetComponent<AudioSource>();
    }

    public void PlayStep()
    {
        if (sfxSource == null)
        {
            return;
        }
        sfxSource.volume = 0.1f;
        int randIndex = Random.Range(0, steps.Length);
        sfxSource.PlayOneShot(steps[randIndex]);
    }

    public void PlayAttack()
    {
        if (sfxSource == null)
        {
            return;
        }
        sfxSource.volume = .3f;

        int randIndex = Random.Range(0, attacks.Length);
        sfxSource.PlayOneShot(attacks[randIndex]);
    }

    public void PlayHit()
    {
        sfxSource.volume = m_Volume;
        int randIndex = Random.Range(0, hits.Length);
        if (randIndex < hits.Length && hits.Length > 0)
        {
            sfxSource.PlayOneShot(hits[randIndex]);
        }
    }
    public void PlayBlockedHit()
    {
        if (sfxSource == null)
        {
            // Debug.LogWarning("~ Audio Source missing for sound effects - " + gameObject.name);
            return;
        }
        sfxSource.volume = m_Volume;
        int randIndex = Random.Range(0, unarmedBlockedHits.Length);
        if (randIndex < unarmedBlockedHits.Length)
        {
            sfxSource.PlayOneShot(unarmedBlockedHits[randIndex]);
        }
    }
    public void PlayImpact()
    {
        if (sfxSource == null)
        {
            // Debug.LogWarning("~ Audio Source missing for sound effects - " + gameObject.name);
            return;
        }
        sfxSource.volume = 1;
        if (impacts == null || impacts.Length == 0) return;
        int randIndex = Random.Range(0, impacts.Length);
        sfxSource.PlayOneShot(impacts[randIndex]);
    }
    public void PlayJump()
    {
        if (sfxSource == null)
        {
            // Debug.LogWarning("~ Audio Source missing for sound effects - " + gameObject.name);
            return;
        }
        sfxSource.volume = m_Volume;
        sfxSource.PlayOneShot(jump[0]);
    }
    public void PlayLand()
    {
        if (sfxSource == null)
        {
            // Debug.LogWarning("~ Audio Source missing for sound effects - " + gameObject.name);
            return;
        }
        sfxSource.volume = 0.2f;

        sfxSource.PlayOneShot(jump[1]);
    }

    public void PlayDeath()
    {
        if (death == null || death.Length == 0) return;
        sfxSource.volume = 1;
        if (death[0] == null) return;
        sfxSource.PlayOneShot(death[0]);
    }

    public void PlayEat()
    {
        sfxSource.volume = 1;

        sfxSource.PlayOneShot(eat[0]);
    }
    public void PlayDropItem()
    {
        sfxSource.volume = 1;
        if (dropItem.Length > 0)
        {
            sfxSource.PlayOneShot(dropItem[0]);
        }
    }
}
