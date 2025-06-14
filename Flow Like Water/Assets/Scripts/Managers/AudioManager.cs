using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AudioTrack
{
    [Header("Track Settings")]
    public string trackName = "Untitled Track";
    public AudioClip clip;
    
    [Header("Playback Settings")]
    [Range(0f, 1f)]
    public float volume = 1f;
    public bool loop = true;
    public bool fadeInOnStart = false;
    
    [Header("Transition Settings")]
    public float fadeInDuration = 2f;
    public float fadeOutDuration = 2f;
    
    [Header("Randomization")]
    public bool randomizeStartTime = false;
    public bool randomizePitch = false;
    [Range(0.8f, 1.2f)]
    public float minPitch = 0.9f;
    [Range(0.8f, 1.2f)]
    public float maxPitch = 1.1f;
}

public class AudioManager : MonoBehaviour
{
    [Header("Background Music")]
    public AudioTrack backgroundTrack;
    public bool playBackgroundOnStart = true;
    
    [Header("Ambient Layers")]
    public List<AudioTrack> ambientTracks = new List<AudioTrack>();
    public int maxConcurrentAmbient = 3;
    public float ambientChangeInterval = 30f;
    public bool randomizeAmbientChanges = true;
    
    [Header("Sound Effects")]
    public AudioClip successSound; // Ding sound for successful prompts
    public AudioClip missSound;    // Sound for missed/failed prompts
    [Range(0f, 1f)]
    public float sfxVolume = 0.8f;
    public bool randomizeSfxPitch = true;
    [Range(0.9f, 1.1f)]
    public float sfxPitchVariation = 0.1f;
    
    [Header("Master Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    [Range(0f, 1f)]
    public float ambientVolume = 0.5f;
    
    [Header("Advanced Settings")]
    public bool crossfadeAmbient = true;
    public float crossfadeDuration = 3f;
    public AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    
    // Audio sources
    private AudioSource backgroundSource;
    private List<AudioSource> ambientSources = new List<AudioSource>();
    private AudioSource sfxSource; // NEW: Dedicated sound effects source
    private List<TrackPlayer> activeTracks = new List<TrackPlayer>();
    
    // Internal state
    private float ambientTimer;
    private Dictionary<AudioTrack, TrackPlayer> trackPlayers = new Dictionary<AudioTrack, TrackPlayer>();
    
    public static AudioManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (playBackgroundOnStart && backgroundTrack.clip != null)
        {
            PlayBackgroundMusic();
        }
        
        ambientTimer = ambientChangeInterval;
        StartCoroutine(InitialAmbientSetup());
    }
    
    void Update()
    {
        UpdateAmbientSystem();
        UpdateVolumeSettings();
    }
    
    void InitializeAudioSources()
    {
        // Background music source
        backgroundSource = gameObject.AddComponent<AudioSource>();
        backgroundSource.playOnAwake = false;
        backgroundSource.spatialBlend = 0f;
        
        // Create ambient audio sources
        for (int i = 0; i < maxConcurrentAmbient; i++)
        {
            AudioSource ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.playOnAwake = false;
            ambientSource.spatialBlend = 0f;
            ambientSources.Add(ambientSource);
        }
        
        // NEW: Sound effects source
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.priority = 128; // High priority for sound effects
        
        Debug.Log($"AudioManager initialized with {ambientSources.Count} ambient sources and SFX source");
    }
    
    // NEW: Play success sound (ding)
    public void PlaySuccessSound()
    {
        PlaySoundEffect(successSound, "Success");
    }
    
    // NEW: Play miss sound
    public void PlayMissSound()
    {
        PlaySoundEffect(missSound, "Miss");
    }
    
    // NEW: Generic sound effect player
    public void PlaySoundEffect(AudioClip clip, string soundName = "SFX")
    {
        if (clip == null || sfxSource == null)
        {
            Debug.LogWarning($"Cannot play {soundName} sound - clip or source missing");
            return;
        }
        
        // Set volume
        sfxSource.volume = sfxVolume * masterVolume;
        
        // Optional pitch randomization
        if (randomizeSfxPitch)
        {
            sfxSource.pitch = Random.Range(1f - sfxPitchVariation, 1f + sfxPitchVariation);
        }
        else
        {
            sfxSource.pitch = 1f;
        }
        
        // Play the sound
        sfxSource.PlayOneShot(clip);
        
        Debug.Log($"Played {soundName} sound effect");
    }
    
    // NEW: Static methods for easy access from other scripts
    public static void PlaySuccess()
    {
        if (Instance != null)
            Instance.PlaySuccessSound();
    }
    
    public static void PlayMiss()
    {
        if (Instance != null)
            Instance.PlayMissSound();
    }
    
    IEnumerator InitialAmbientSetup()
    {
        yield return new WaitForSeconds(1f);
        
        int initialCount = Random.Range(1, Mathf.Min(3, ambientTracks.Count + 1));
        for (int i = 0; i < initialCount; i++)
        {
            PlayRandomAmbientTrack();
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    void UpdateAmbientSystem()
    {
        if (!randomizeAmbientChanges) return;
        
        ambientTimer -= Time.deltaTime;
        
        if (ambientTimer <= 0f)
        {
            ambientTimer = ambientChangeInterval + Random.Range(-5f, 10f);
            
            float action = Random.Range(0f, 1f);
            
            if (action < 0.4f && activeTracks.Count > 0)
            {
                StopRandomAmbientTrack();
            }
            else if (action < 0.8f && activeTracks.Count < maxConcurrentAmbient)
            {
                PlayRandomAmbientTrack();
            }
            else if (activeTracks.Count > 1)
            {
                StartCoroutine(SwapRandomTrack());
            }
        }
    }
    
    void UpdateVolumeSettings()
    {
        if (backgroundSource != null)
        {
            backgroundSource.volume = backgroundTrack.volume * musicVolume * masterVolume;
        }
        
        foreach (TrackPlayer player in activeTracks)
        {
            if (player.audioSource != null)
            {
                float targetVolume = player.track.volume * ambientVolume * masterVolume * player.currentFadeMultiplier;
                player.audioSource.volume = targetVolume;
            }
        }
        
        // NEW: Update SFX volume
        if (sfxSource != null)
        {
            // SFX volume is handled per-sound in PlaySoundEffect, but we can set a base here
            // This is mainly for consistency
        }
    }
    
    public void PlayBackgroundMusic()
    {
        if (backgroundTrack.clip == null || backgroundSource == null) return;
        
        backgroundSource.clip = backgroundTrack.clip;
        backgroundSource.loop = backgroundTrack.loop;
        backgroundSource.volume = backgroundTrack.volume * musicVolume * masterVolume;
        
        if (backgroundTrack.randomizePitch)
        {
            backgroundSource.pitch = Random.Range(backgroundTrack.minPitch, backgroundTrack.maxPitch);
        }
        
        if (backgroundTrack.randomizeStartTime && backgroundTrack.clip.length > 0)
        {
            backgroundSource.time = Random.Range(0f, backgroundTrack.clip.length);
        }
        
        backgroundSource.Play();
        
        if (backgroundTrack.fadeInOnStart)
        {
            StartCoroutine(FadeInBackground());
        }
        
        Debug.Log($"Started background music: {backgroundTrack.trackName}");
    }
    
    public void PlayRandomAmbientTrack()
    {
        if (ambientTracks.Count == 0 || activeTracks.Count >= maxConcurrentAmbient) return;
        
        List<AudioTrack> availableTracks = new List<AudioTrack>();
        foreach (AudioTrack track in ambientTracks)
        {
            if (!trackPlayers.ContainsKey(track) || !trackPlayers[track].isActive)
            {
                availableTracks.Add(track);
            }
        }
        
        if (availableTracks.Count == 0) return;
        
        AudioTrack selectedTrack = availableTracks[Random.Range(0, availableTracks.Count)];
        PlayAmbientTrack(selectedTrack);
    }
    
    public void PlayAmbientTrack(AudioTrack track)
    {
        if (track.clip == null) return;
        
        AudioSource availableSource = GetAvailableAmbientSource();
        if (availableSource == null) return;
        
        TrackPlayer player;
        if (trackPlayers.ContainsKey(track))
        {
            player = trackPlayers[track];
        }
        else
        {
            player = new TrackPlayer(track);
            trackPlayers[track] = player;
        }
        
        availableSource.clip = track.clip;
        availableSource.loop = track.loop;
        availableSource.pitch = track.randomizePitch ? 
                               Random.Range(track.minPitch, track.maxPitch) : 1f;
        
        if (track.randomizeStartTime && track.clip.length > 0)
        {
            availableSource.time = Random.Range(0f, track.clip.length);
        }
        
        player.audioSource = availableSource;
        player.isActive = true;
        player.currentFadeMultiplier = track.fadeInOnStart ? 0f : 1f;
        
        activeTracks.Add(player);
        availableSource.Play();
        
        if (track.fadeInOnStart || track.fadeInDuration > 0)
        {
            StartCoroutine(FadeInTrack(player));
        }
        
        Debug.Log($"Started ambient track: {track.trackName}");
    }
    
    public void StopRandomAmbientTrack()
    {
        if (activeTracks.Count == 0) return;
        
        TrackPlayer randomPlayer = activeTracks[Random.Range(0, activeTracks.Count)];
        StopAmbientTrack(randomPlayer);
    }
    
    public void StopAmbientTrack(TrackPlayer player)
    {
        if (player == null || !player.isActive) return;
        
        StartCoroutine(FadeOutTrack(player));
    }
    
    IEnumerator SwapRandomTrack()
    {
        if (activeTracks.Count == 0) yield break;
        
        TrackPlayer oldPlayer = activeTracks[Random.Range(0, activeTracks.Count)];
        StopAmbientTrack(oldPlayer);
        
        yield return new WaitForSeconds(crossfadeDuration * 0.5f);
        
        PlayRandomAmbientTrack();
    }
    
    AudioSource GetAvailableAmbientSource()
    {
        foreach (AudioSource source in ambientSources)
        {
            if (!source.isPlaying)
                return source;
        }
        
        foreach (AudioSource source in ambientSources)
        {
            bool isUsed = false;
            foreach (TrackPlayer player in activeTracks)
            {
                if (player.audioSource == source)
                {
                    isUsed = true;
                    break;
                }
            }
            if (!isUsed) return source;
        }
        
        return null;
    }
    
    IEnumerator FadeInBackground()
    {
        float elapsed = 0f;
        float startVolume = 0f;
        float targetVolume = backgroundTrack.volume * musicVolume * masterVolume;
        
        backgroundSource.volume = startVolume;
        
        while (elapsed < backgroundTrack.fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / backgroundTrack.fadeInDuration;
            float curveValue = fadeInCurve.Evaluate(progress);
            
            backgroundSource.volume = Mathf.Lerp(startVolume, targetVolume, curveValue);
            yield return null;
        }
        
        backgroundSource.volume = targetVolume;
    }
    
    IEnumerator FadeInTrack(TrackPlayer player)
    {
        float elapsed = 0f;
        float duration = player.track.fadeInDuration;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            player.currentFadeMultiplier = fadeInCurve.Evaluate(progress);
            yield return null;
        }
        
        player.currentFadeMultiplier = 1f;
    }
    
    IEnumerator FadeOutTrack(TrackPlayer player)
    {
        float elapsed = 0f;
        float duration = player.track.fadeOutDuration;
        float startMultiplier = player.currentFadeMultiplier;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            player.currentFadeMultiplier = Mathf.Lerp(startMultiplier, 0f, fadeOutCurve.Evaluate(progress));
            yield return null;
        }
        
        if (player.audioSource != null)
        {
            player.audioSource.Stop();
            player.audioSource.clip = null;
        }
        
        player.isActive = false;
        activeTracks.Remove(player);
        
        Debug.Log($"Faded out track: {player.track.trackName}");
    }
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
    }
    
    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
    }
    
    // NEW: Set SFX volume
    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }
    
    public void FadeOutAllAmbient(float duration = 2f)
    {
        foreach (TrackPlayer player in System.Linq.Enumerable.ToArray(activeTracks))
        {
            StartCoroutine(FadeOutTrackCustomDuration(player, duration));
        }
    }
    
    IEnumerator FadeOutTrackCustomDuration(TrackPlayer player, float duration)
    {
        float elapsed = 0f;
        float startMultiplier = player.currentFadeMultiplier;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            player.currentFadeMultiplier = Mathf.Lerp(startMultiplier, 0f, progress);
            yield return null;
        }
        
        if (player.audioSource != null)
        {
            player.audioSource.Stop();
            player.audioSource.clip = null;
        }
        
        player.isActive = false;
        activeTracks.Remove(player);
    }
}

[System.Serializable]
public class TrackPlayer
{
    public AudioTrack track;
    public AudioSource audioSource;
    public bool isActive;
    public float currentFadeMultiplier = 1f;
    
    public TrackPlayer(AudioTrack audioTrack)
    {
        track = audioTrack;
        isActive = false;
        currentFadeMultiplier = 1f;
    }
}
