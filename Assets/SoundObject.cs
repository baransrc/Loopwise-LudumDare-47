using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.SerializableAttribute]
public class SoundObject
{
    [SerializeField] private Sounds _soundType;
    [SerializeField] private AudioSource _audioSource;

    public Sounds SoundType
    {
        get
        {
            return _soundType;
        }

        private set
        {
            
        }
    }
    
    public void Play(float pitch = 1)
    {
        // _audioSource.pitch = pitch;
        _audioSource.Play();
    }

    public void Pause()
    {
        _audioSource.Pause();
    }

    public void Stop()
    {
        _audioSource.Stop();
    }

    public bool IsPlaying()
    {
        return _audioSource.isPlaying;
    }
}