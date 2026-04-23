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

    public Sound()
    {
        volume = 1;
        pitch = 1;
        loop = false;
    }
}

public class AudioManager2 : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager2 instance;
    //AudioManager

    void Awake()
    {
        // NEW: single instance across scenes
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            if (!s.source)
                s.source = gameObject.AddComponent<AudioSource>();

            s.source.clip = s.clip;
            s.source.playOnAwake = false; 
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

            
            if (s.playOnAwake) s.source.Play();
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
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

        if (s.source.isPlaying)
            s.source.Stop();
    }

    public bool IsPlaying(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        return s != null && s.source != null && s.source.isPlaying;
    }
}