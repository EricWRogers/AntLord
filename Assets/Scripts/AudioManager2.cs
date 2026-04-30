using UnityEngine.Audio;
using System;
using UnityEngine;

//Credit to Brackeys youtube tutorial on Audio managers, as the majority of this code and learning how to use it was made by him.
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
    public AudioSource source;

    
    [HideInInspector] public float baseVolume = 1;
}

public class AudioManager2 : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager2 instance;

    void Awake()
    {
        instance = this;

        foreach (Sound s in sounds)
        {
            if (!s.source)
                s.source = gameObject.AddComponent<AudioSource>();

            s.source.clip = s.clip;
            s.source.playOnAwake = s.playOnAwake;

            s.source.volume = s.volume;
            s.baseVolume = s.volume; // NEW
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

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

        
        if (!s.source.isPlaying)
            s.source.Play();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null || s.source == null) return;

        s.source.Stop();
    }

    
    public void SetVolume(string name, float vol)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null || s.source == null) return;

        s.source.volume = Mathf.Clamp01(vol);
    }

    
    public void RestoreVolume(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null || s.source == null) return;

        s.source.volume = s.baseVolume;
    }
}