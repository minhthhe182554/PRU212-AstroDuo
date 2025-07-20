using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource; // For background music
    [SerializeField] private AudioSource sfxSource;   // For sound effects
    [SerializeField] private AudioSource loopSource; // For looping effects (saber, shield)
    
    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;
    
    [Header("Weapon Sounds")]
    [SerializeField] private AudioClip basicBulletSound;
    [SerializeField] private AudioClip scatterShotSound;
    
    [Header("Power-up Sounds")]
    [SerializeField] private AudioClip saberActivateSound; // Looping saber sound
    [SerializeField] private AudioClip shieldActivateSound; // Looping shield sound
    
    [Header("Game Event Sounds")]
    [SerializeField] private AudioClip playerDeathSound;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Setup audio sources if not assigned
            SetupAudioSources();
            
            // Start background music
            PlayBackgroundMusic();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void SetupAudioSources()
    {
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.5f;
        }
        
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.volume = 0.7f;
        }
        
        if (loopSource == null)
        {
            GameObject loopObj = new GameObject("LoopSource");
            loopObj.transform.SetParent(transform);
            loopSource = loopObj.AddComponent<AudioSource>();
            loopSource.loop = true;
            loopSource.volume = 0.6f;
        }
    }
    
    // Background Music
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
            Debug.Log("🎵 Background music started");
        }
    }
    
    public void StopBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
    
    // Weapon Sounds
    public void PlayBasicBulletSound()
    {
        if (basicBulletSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(basicBulletSound);
        }
    }
    
    public void PlayScatterShotSound()
    {
        if (scatterShotSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(scatterShotSound);
        }
    }
    
    // Power-up Sounds (Looping)
    public void PlaySaberSound()
    {
        if (saberActivateSound != null && loopSource != null)
        {
            loopSource.clip = saberActivateSound;
            loopSource.Play();
            Debug.Log("⚔️ Saber sound started (looping)");
        }
    }
    
    public void StopSaberSound()
    {
        if (loopSource != null && loopSource.clip == saberActivateSound)
        {
            loopSource.Stop();
            Debug.Log("⚔️ Saber sound stopped");
        }
    }
    
    public void PlayShieldSound()
    {
        if (shieldActivateSound != null && loopSource != null)
        {
            loopSource.clip = shieldActivateSound;
            loopSource.Play();
            Debug.Log("🛡️ Shield sound started (looping)");
        }
    }
    
    public void StopShieldSound()
    {
        if (loopSource != null && loopSource.clip == shieldActivateSound)
        {
            loopSource.Stop();
            Debug.Log("🛡️ Shield sound stopped");
        }
    }
    
    // Game Event Sounds
    public void PlayPlayerDeathSound()
    {
        if (playerDeathSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(playerDeathSound);
            Debug.Log("💀 Player death sound played");
        }
    }
    
    // Volume Controls
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
            musicSource.volume = volume;
    }
    
    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
            sfxSource.volume = volume;
        if (loopSource != null)
            loopSource.volume = volume;
    }
} 