using UnityEngine.Audio;
using System;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0, 1)]
    public float volume = 1;

    [Range(-3, 3)]
    public float pitch = 1;

    public bool loop = false;
    public bool playOnAwake = false;

    [Header("Mute Settings")]
    public bool isMusic = false;

    [HideInInspector] public AudioSource source;

    [HideInInspector] public float baseVolume = 1;
    [HideInInspector] public float currentVolume = 1;
}

public class AudioManager2 : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager2 instance;

    private bool musicMuted = false;

    [Range(0, 1)]
    private float musicVolume = 1f;

    void Awake()
    {
        instance = this;

        musicMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);

        foreach (Sound s in sounds)
        {
            if (!s.source)
                s.source = gameObject.AddComponent<AudioSource>();

            s.source.clip = s.clip;
            s.source.playOnAwake = s.playOnAwake;

            s.baseVolume = s.volume;
            s.currentVolume = s.volume;

            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

            ApplyVolume(s);

            if (s.playOnAwake)
                s.source.Play();
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null || s.source == null)
        {
            Debug.LogWarning("Sound: " + name + " not found");
            return;
        }

        ApplyVolume(s);

        if (!s.source.isPlaying)
            s.source.Play();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null || s.source == null)
            return;

        s.source.Stop();
    }

    public void SetVolume(string name, float vol)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null || s.source == null)
            return;

        s.currentVolume = Mathf.Clamp01(vol);
        ApplyVolume(s);
    }

    public void RestoreVolume(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null || s.source == null)
            return;

        s.currentVolume = s.baseVolume;
        ApplyVolume(s);
    }

    public void SetMusicMuted(bool muted)
    {
        musicMuted = muted;

        PlayerPrefs.SetInt("MusicMuted", musicMuted ? 1 : 0);
        PlayerPrefs.Save();

        foreach (Sound s in sounds)
        {
            ApplyVolume(s);
        }
    }

    public bool IsMusicMuted()
    {
        return musicMuted;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();

        foreach (Sound s in sounds)
        {
            ApplyVolume(s);
        }
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    private void ApplyVolume(Sound s)
    {
        if (s == null || s.source == null)
            return;

        if (s.isMusic)
        {
            if (musicMuted)
            {
                s.source.volume = 0f;
            }
            else
            {
                s.source.volume = s.currentVolume * musicVolume;
            }
        }
        else
        {
            s.source.volume = s.currentVolume;
        }
    }
}