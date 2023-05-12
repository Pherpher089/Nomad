using UnityEngine;
public class AudioManager : MonoBehaviour
{
    public AudioClip[] soundEffects;
    private AudioSource sfxSource;

    void Start()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlaySoundEffect(int effectIndex)
    {
        sfxSource.PlayOneShot(soundEffects[effectIndex]);
    }
}
