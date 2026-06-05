using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    
    [Header("音频源")]
    public AudioSource engineAudio;
    public AudioSource afterburnerAudio;
    public AudioSource weaponAudio;
    public AudioSource hitAudio;
    public AudioSource ambientAudio;
    
    [Header("音频剪辑")]
    public AudioClip engineLoop;
    public AudioClip afterburnerLoop;
    public AudioClip laserShot;
    public AudioClip missileLaunch;
    public AudioClip explosion;
    public AudioClip ambientSpace;
    
    [Header("音频参数")]
    public float engineMinVolume = 0.3f;
    public float engineMaxVolume = 0.8f;
    public float engineMinPitch = 0.5f;
    public float engineMaxPitch = 1.5f;
    
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        InitializeAudioSources();
    }
    
    void InitializeAudioSources()
    {
        if (engineAudio == null)
        {
            engineAudio = gameObject.AddComponent<AudioSource>();
            engineAudio.loop = true;
            engineAudio.clip = engineLoop;
        }
        
        if (afterburnerAudio == null)
        {
            afterburnerAudio = gameObject.AddComponent<AudioSource>();
            afterburnerAudio.loop = true;
            afterburnerAudio.clip = afterburnerLoop;
            afterburnerAudio.volume = 0.6f;
        }
        
        if (weaponAudio == null)
        {
            weaponAudio = gameObject.AddComponent<AudioSource>();
            weaponAudio.volume = 0.5f;
        }
        
        if (hitAudio == null)
        {
            hitAudio = gameObject.AddComponent<AudioSource>();
            hitAudio.volume = 0.7f;
        }
        
        if (ambientAudio == null)
        {
            ambientAudio = gameObject.AddComponent<AudioSource>();
            ambientAudio.loop = true;
            ambientAudio.clip = ambientSpace;
            ambientAudio.volume = 0.2f;
            ambientAudio.Play();
        }
    }
    
    public void PlayEngineSound(float thrustPercentage)
    {
        if (!engineAudio.isPlaying)
            engineAudio.Play();
        
        engineAudio.volume = Mathf.Lerp(engineMinVolume, engineMaxVolume, thrustPercentage);
        engineAudio.pitch = Mathf.Lerp(engineMinPitch, engineMaxPitch, thrustPercentage);
    }
    
    public void StopEngineSound()
    {
        engineAudio.Stop();
    }
    
    public void PlayAfterburner(bool active)
    {
        if (active && !afterburnerAudio.isPlaying)
            afterburnerAudio.Play();
        else if (!active && afterburnerAudio.isPlaying)
            afterburnerAudio.Stop();
    }
    
    public void PlayLaserShot()
    {
        weaponAudio.clip = laserShot;
        weaponAudio.Play();
    }
    
    public void PlayMissileLaunch()
    {
        weaponAudio.clip = missileLaunch;
        weaponAudio.Play();
    }
    
    public void PlayExplosion()
    {
        hitAudio.clip = explosion;
        hitAudio.Play();
    }
    
    public void PlayHit()
    {
        hitAudio.clip = explosion;
        hitAudio.volume = 0.3f;
        hitAudio.Play();
    }
    
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }
    
    public void PauseAll()
    {
        engineAudio.Pause();
        afterburnerAudio.Pause();
        ambientAudio.Pause();
    }
    
    public void ResumeAll()
    {
        engineAudio.UnPause();
        afterburnerAudio.UnPause();
        ambientAudio.UnPause();
    }
}
