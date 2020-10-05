using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSourceMusic;
    [SerializeField] private List<SoundObject> _playableSounds;
    
    private static int currentId = 0;
    private static AudioManager _instance = null;
    
    private int _previousState;
    private int _ID;

    public static AudioManager Instance
    {
        get
        {
            return _instance;
        }

        private set
        {
            _instance = value;
        }
    }
    
    public int ID
    {
        get
        {
            return _ID;
        }

        private set
        {
            _ID = value;
        }
    }
    
    private void Awake()
    {
        currentId++;
        ID = currentId;
        
        if (Instance  ==  null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
            return;
        }
                
        _previousState = PlayerPrefs.GetInt(StoredVariables.MusicToggle_Int, 1);

        if (_previousState == 0)
            _audioSourceMusic.Pause();
        else
            _audioSourceMusic.Play();
    }

    private void PlayMusic()
    {
        var currentState = PlayerPrefs.GetInt(StoredVariables.MusicToggle_Int, 1);

        if (_previousState == currentState) return;

        _previousState = currentState; 
        
        if (currentState == 0)
            _audioSourceMusic.Pause();
        else
            _audioSourceMusic.Play();
    }

    public void PlaySound(Sounds soundType)
    {
        if (PlayerPrefs.GetInt(StoredVariables.SoundToggle_Int, 1) == 0) return;
        
        if (_playableSounds.Count == 0) return;

        var soundToPlay = _playableSounds.Find(x => x.SoundType == soundType);

        if (soundToPlay.IsPlaying()) soundToPlay.Stop();

        soundToPlay.Play();
    }

    public void StopSound(Sounds soundType)
    {
        var soundToPlay = _playableSounds.Find(x => x.SoundType == soundType);

        if (soundToPlay.IsPlaying())
        {
            soundToPlay.Stop();
        }
    }
    
    private void Update()
    {
        PlayMusic();
    }
}