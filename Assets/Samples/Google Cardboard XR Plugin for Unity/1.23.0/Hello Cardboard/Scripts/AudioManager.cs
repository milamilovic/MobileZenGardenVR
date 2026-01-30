using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : PersistentSingleton<AudioManager>
{
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource effectsAudioSource;

    // Adaptive music fields
    private AudioClip[] musicLayers;
    private AudioClip[] musicTails;
    private int currentComplexity = 1;
    private int maxComplexity = 8;
    private bool isTransitioning = false;
    private int loopCounter = 0;
    private int loopsBeforeComplexityIncrease = 3;
    private bool autoProgressionEnabled = true;
    private bool pendingComplexityIncrease = false;
    private int maxComplexityLoops = 0;

    public void PlaySimpleMusic(AudioClip audioClip, float volume = 1f)
    {
        // Stop adaptive music system
        autoProgressionEnabled = false;
        StopAllCoroutines();

        // Play simple looping music
        musicAudioSource.loop = true;
        musicAudioSource.clip = audioClip;
        musicAudioSource.volume = volume;
        musicAudioSource.Play();

        Debug.Log("Playing simple looping music");
    }

    public void PlayAdaptiveMusic(float volume = 1f)
    {
        if (musicLayers == null || musicLayers.Length == 0)
        {
            Debug.LogWarning("Adaptive music not set up! Use SetupAdaptiveMusic() first.");
            return;
        }

        StopMusic();
        StartAdaptiveMusic(volume, true);

        Debug.Log("Switched to adaptive music");
    }

    public void SetupAdaptiveMusic(AudioClip[] loops, AudioClip[] tails)
    {
        musicLayers = loops;
        musicTails = tails;
        currentComplexity = 1;
        loopCounter = 0;
        maxComplexityLoops = 0;
    }

    public void StartAdaptiveMusic(float volume = 1f, bool enableAutoProgression = true)
    {
        if (musicLayers == null || musicLayers.Length == 0)
        {
            Debug.LogWarning("No music layers assigned!");
            return;
        }

        currentComplexity = 1;
        loopCounter = 0;
        maxComplexityLoops = 0;
        autoProgressionEnabled = enableAutoProgression;
        pendingComplexityIncrease = false;

        musicAudioSource.clip = musicLayers[0]; // Complexity 1
        musicAudioSource.volume = volume;
        musicAudioSource.loop = true;
        musicAudioSource.Play();

        if (autoProgressionEnabled)
        {
            StartCoroutine(MonitorLoopsForAutoProgression());
        }
    }

    private IEnumerator MonitorLoopsForAutoProgression()
    {
        float lastTime = 0f;

        while (autoProgressionEnabled && musicAudioSource.isPlaying)
        {
            float currentTime = musicAudioSource.time;

            // Detect when the loop restarts (time goes back to near 0)
            if (currentTime < lastTime && lastTime > 0.5f)
            {
                // Loop just restarted - if we have a pending increase, do it now
                if (pendingComplexityIncrease)
                {
                    pendingComplexityIncrease = false;
                    PerformComplexityIncrease();
                }
                else
                {
                    loopCounter++;

                    // If we're at max complexity, count those loops separately
                    if (currentComplexity == maxComplexity)
                    {
                        maxComplexityLoops++;
                        Debug.Log($"Max complexity loop {maxComplexityLoops}/3");

                        // After 3 loops at max complexity, play tail and restart
                        if (maxComplexityLoops >= 3)
                        {
                            StartCoroutine(PlayTailAndRestart());
                            yield break; // Exit this coroutine
                        }
                    }
                    else
                    {
                        Debug.Log($"Loop completed. Count: {loopCounter}");

                        // Every 3 loops, schedule complexity increase for NEXT loop restart
                        if (loopCounter >= loopsBeforeComplexityIncrease)
                        {
                            loopCounter = 0;
                            if (currentComplexity < maxComplexity)
                            {
                                pendingComplexityIncrease = true;
                                Debug.Log("Complexity increase scheduled for next loop");
                            }
                        }
                    }
                }
            }

            lastTime = currentTime;
            yield return null;
        }
    }

    private IEnumerator PlayTailAndRestart()
    {
        Debug.Log("Playing tail 8 and restarting from complexity 1");

        float lastTime = musicAudioSource.time;
        float currentVolume = musicAudioSource.volume;

        // Wait for the current loop to restart naturally
        while (musicAudioSource.isPlaying)
        {
            float currentTime = musicAudioSource.time;

            if (currentTime < lastTime && lastTime > 0.5f)
            {
                break;
            }

            lastTime = currentTime;
            yield return null;
        }

        // Play tail 8
        if (musicTails != null && musicTails.Length >= maxComplexity)
        {
            musicAudioSource.loop = false;
            musicAudioSource.clip = musicTails[maxComplexity - 1];
            musicAudioSource.Play();

            // Wait for tail to finish
            yield return new WaitWhile(() => musicAudioSource.isPlaying);
        }

        // Restart from complexity 1
        currentComplexity = 1;
        loopCounter = 0;
        maxComplexityLoops = 0;
        pendingComplexityIncrease = false;

        musicAudioSource.clip = musicLayers[0];
        musicAudioSource.volume = currentVolume;
        musicAudioSource.loop = true;
        musicAudioSource.Play();

        Debug.Log("Restarted from complexity 1");

        // Resume monitoring
        if (autoProgressionEnabled)
        {
            StartCoroutine(MonitorLoopsForAutoProgression());
        }
    }

    private void PerformComplexityIncrease()
    {
        currentComplexity++;
        currentComplexity = Mathf.Min(currentComplexity, maxComplexity);

        Debug.Log($"Increasing to Complexity {currentComplexity}");

        float currentVolume = musicAudioSource.volume;
        musicAudioSource.clip = musicLayers[currentComplexity - 1];
        musicAudioSource.Play();
        musicAudioSource.volume = currentVolume;
    }

    public void IncreaseComplexity()
    {
        if (currentComplexity < maxComplexity && !isTransitioning)
        {
            StartCoroutine(TransitionToNextComplexity());
        }
    }

    public void SetComplexity(int complexity)
    {
        complexity = Mathf.Clamp(complexity, 1, maxComplexity);
        if (complexity != currentComplexity && !isTransitioning)
        {
            StartCoroutine(TransitionToComplexity(complexity));
        }
    }

    private IEnumerator TransitionToNextComplexity()
    {
        isTransitioning = true;
        
        // Wait for current loop to finish (sync to beat)
        yield return new WaitUntil(() => 
            musicAudioSource.time >= musicAudioSource.clip.length - 0.1f);
        
        currentComplexity++;
        currentComplexity = Mathf.Min(currentComplexity, maxComplexity);
        
        Debug.Log($"Transitioning to Complexity {currentComplexity}");
        
        // Switch to next complexity layer
        float currentVolume = musicAudioSource.volume;
        musicAudioSource.clip = musicLayers[currentComplexity - 1];
        musicAudioSource.Play();
        musicAudioSource.volume = currentVolume;
        
        isTransitioning = false;
    }

    public void SetLoopsBeforeIncrease(int loops)
    {
        loopsBeforeComplexityIncrease = Mathf.Max(1, loops);
    }

    public void SetAutoProgression(bool enabled)
    {
        autoProgressionEnabled = enabled;
        if (enabled && musicAudioSource.isPlaying)
        {
            StartCoroutine(MonitorLoopsForAutoProgression());
        }
    }

    private IEnumerator TransitionToComplexity(int targetComplexity)
    {
        isTransitioning = true;

        // Wait for current loop to finish
        yield return new WaitUntil(() =>
            musicAudioSource.time >= musicAudioSource.clip.length - 0.1f);

        currentComplexity = targetComplexity;

        float currentVolume = musicAudioSource.volume;
        musicAudioSource.clip = musicLayers[currentComplexity - 1];
        musicAudioSource.Play();
        musicAudioSource.volume = currentVolume;

        isTransitioning = false;
    }

    public void EndAdaptiveMusicWithTail()
    {
        autoProgressionEnabled = false; // Stop auto-progression
        StopAllCoroutines();

        if (musicTails != null && musicTails.Length >= currentComplexity)
        {
            StartCoroutine(PlayMusicTail());
        }
        else
        {
            StopMusic();
        }
    }

    private IEnumerator PlayMusicTail()
    {
        // Wait for current loop to finish
        yield return new WaitUntil(() =>
            musicAudioSource.time >= musicAudioSource.clip.length - 0.1f);

        musicAudioSource.loop = false;
        musicAudioSource.clip = musicTails[currentComplexity - 1];
        musicAudioSource.Play();
    }

    public int GetCurrentComplexity() => currentComplexity;

    public void PlaySound(AudioClip audioClip)
    {
        effectsAudioSource.PlayOneShot(audioClip);
    }

    public void PlaySound(AudioClip audioClip, float volume)
    {
        effectsAudioSource.PlayOneShot(audioClip, volume);
    }

    public void PlayRandomSound(AudioClip[] audioClips)
    {
        int randomAudioClipIndex = Random.Range(0, audioClips.Length);
        AudioClip audioClip = audioClips[randomAudioClipIndex];
        effectsAudioSource.PlayOneShot(audioClip);
    }

    public void PlayRandomSound(AudioClip[] audioClips, float volume)
    {
        int randomAudioClipIndex = Random.Range(0, audioClips.Length);
        AudioClip audioClip = audioClips[randomAudioClipIndex];
        effectsAudioSource.PlayOneShot(audioClip, volume);
    }

    public void PlayMusic(AudioClip audioClip, float volume)
    {
        if (musicAudioSource.clip == audioClip)
            return;

        musicAudioSource.Stop();
        musicAudioSource.clip = audioClip;
        musicAudioSource.volume = volume;
        musicAudioSource.Play();
    }

    public void PauseMusic()
    {
        musicAudioSource.Pause();
    }

    public void StopMusic()
    {
        musicAudioSource.Stop();
    }

    public void ToggleMusic()
    {
        musicAudioSource.mute = !musicAudioSource.mute;
    }

    public void ToggleEffects()
    {
        effectsAudioSource.mute = !effectsAudioSource.mute;
    }

    public void ChangeMasterVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void ChangeMusicVolume(float volume)
    {
        musicAudioSource.volume = volume;
    }

    public void ChangeEffectsVolume(float volume)
    {
        effectsAudioSource.volume = volume;
    }

    public float CalculateVolumeByCollisionForce(float collisionForce, float forceThreshold)
    {
        float volume = 1;

        if (collisionForce <= forceThreshold)
        {
            volume = collisionForce / forceThreshold;
        }

        Debug.Log("Volume: " + volume);

        return volume;
    }
}
