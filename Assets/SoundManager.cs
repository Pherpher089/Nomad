using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public float m_MusicInterval = 45;
    public float timer = 0;
    public AudioClip[] musicTracks;
    public AudioClip[] ambientSounds;
    public AudioClip[] soundEffects;

    private AudioSource musicSource;
    private AudioSource ambientSource;
    private AudioSource sfxSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        ambientSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = false;
        ambientSource.loop = true;
    }

    void Start()
    {
        //PlayMusic(0);
        PlayAmbientSound(0);
    }
    void Update()
    {
        AmbientMusicTimer();
    }

    void AmbientMusicTimer()
    {
        if (!musicSource.isPlaying)
        {
            if (timer < m_MusicInterval)
            {
                timer += Time.deltaTime;
            }
            else
            {
                PlayMusic(0);
                timer = 0;
            }
        }
    }

    public void PlayMusic(int trackIndex)
    {
        musicSource.clip = musicTracks[trackIndex];
        musicSource.Play();
    }

    public void PlayAmbientSound(int soundIndex)
    {
        ambientSource.clip = ambientSounds[soundIndex];
        ambientSource.Play();
    }

    public void PlaySoundEffect(int effectIndex)
    {
        sfxSource.PlayOneShot(soundEffects[effectIndex]);
    }
}
