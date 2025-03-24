using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private int sfxCount;
    public Queue<AudioSource> sfxPool;

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private int musicCount;
    public Queue<AudioSource> musicPool;

    [SerializeField] private AudioMixer mixer;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else Destroy(gameObject);

        sfxPool = new Queue<AudioSource>();
        musicPool = new Queue<AudioSource>();

        for (int i = 0; i < sfxCount; i++)
        {
            sfxPool.Enqueue(Instantiate(sfxSource, transform));
        }

        for (int i = 0; i < musicCount; i++)
        {
            musicPool.Enqueue(Instantiate(musicSource, transform));
        }
    }

    private AudioSource GetSFXSource()
    {
        AudioSource sfx = sfxPool.Dequeue();
        sfxPool.Enqueue(sfx);
        return sfx;
    }

    public void PlaySFX(AudioClip clip)
    {
        AudioSource sfx = GetSFXSource();
        sfx.clip = clip;
        sfx.Play();
    }

    public AudioSource PlaySFX(AudioClip clip, bool getClip)
    {
        AudioSource sfx = GetSFXSource();
        sfx.clip = clip;
        sfx.Play();

        if (!getClip) return null;
        else return sfx;
    }

    public void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat("SFX", volume);
        mixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
    }

    private AudioSource GetMusicSource()
    {
        AudioSource music = musicPool.Dequeue();
        musicPool.Enqueue(music);
        return music;
    }

    public void PlayMusic(AudioClip clip)
    {
        AudioSource music = GetMusicSource();
        music.clip = clip;
        music.Play();
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("Music", volume);
        mixer.SetFloat("Music", Mathf.Log10(volume) * 20);
    }

    public void SetMasterVolume(float volume)
    {
        PlayerPrefs.SetFloat("Master", volume);
        mixer.SetFloat("Master", Mathf.Log10(volume) * 20);
        Debug.Log(PlayerPrefs.GetFloat("Master"));
    }

    public void StopSounds()
    {
        for (int i = 0; i < musicPool.Count; i++)
        {
            AudioSource source = musicPool.Dequeue();
            source.Stop();
            musicPool.Enqueue(source);
        }

        for (int i = 0; i < sfxPool.Count; i++)
        {
            AudioSource source = sfxPool.Dequeue();
            source.Stop();
            sfxPool.Enqueue(source);
        }
    }
}
