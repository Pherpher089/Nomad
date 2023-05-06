using UnityEngine;
public class ActorAudioManager : MonoBehaviour
{
    public AudioClip[] steps;
    public AudioClip[] attacks;
    public AudioClip[] hits;
    public AudioClip[] impacts;

    public AudioClip[] jump;
    public AudioClip[] death;

    [Range(0, 1)] public float m_Volume;



    private AudioSource sfxSource;

    void Start()
    {
        sfxSource = gameObject.GetComponent<AudioSource>();
    }

    public void PlayStep()
    {
        sfxSource.volume = 0.1f;
        int randIndex = Random.Range(0, steps.Length);
        sfxSource.PlayOneShot(steps[randIndex]);
    }

    public void PlayAttack()
    {
        sfxSource.volume = .3f;

        int randIndex = Random.Range(0, attacks.Length);
        sfxSource.PlayOneShot(attacks[randIndex]);
    }

    public void PlayHit()
    {
        sfxSource.volume = m_Volume;
        int randIndex = Random.Range(0, hits.Length);
        if (randIndex < hits.Length)
        {
            sfxSource.PlayOneShot(hits[randIndex]);
        }
    }
    public void PlayImpact()
    {
        sfxSource.volume = 1;
        int randIndex = Random.Range(0, impacts.Length);
        sfxSource.PlayOneShot(impacts[randIndex]);
    }
    public void PlayJump()
    {
        sfxSource.volume = m_Volume;
        sfxSource.PlayOneShot(jump[0]);
    }
    public void PlayLand()
    {
        sfxSource.volume = 0.2f;

        sfxSource.PlayOneShot(jump[1]);
    }

    public void PlayDeath()
    {
        sfxSource.volume = 1;

        sfxSource.PlayOneShot(death[0]);
    }
}
