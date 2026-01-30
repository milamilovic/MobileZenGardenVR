using UnityEngine;

public class Bush : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip bushWalkingSound;
    [SerializeField] private float volume = 0.5f;
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    [Header("Visual Feedback")]
    [SerializeField] private float jiggleStrength = 0.1f;
    [SerializeField] private float jiggleDuration = 0.3f;

    [Header("Movement Slowdown")]
    [SerializeField] private float speedMultiplier = 0.6f; // 60% of speed

    private Vector3 originalPosition;
    private bool isJiggling = false;
    private Coroutine jiggleCoroutine;

    // Audio management
    private AudioSource bushAudioSource;
    private bool isPlayerInBush = false;
    private Coroutine fadeCoroutine;

    void Start()
    {
        originalPosition = transform.localPosition;

        // Create AudioSource for bush
        bushAudioSource = gameObject.AddComponent<AudioSource>();
        bushAudioSource.clip = bushWalkingSound;
        bushAudioSource.loop = true;
        bushAudioSource.volume = 0f;

        // Add a trigger collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            // Add a sphere collider
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.isTrigger = true;
            sphereCol.radius = 0.5f;
        }
        else
        {
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if it's the player
        LookToWalk playerController = other.GetComponent<LookToWalk>();
        if (playerController != null)
        {
            isPlayerInBush = true;
            StartBushSound();
            StartJiggle();
        }
    }

    void OnTriggerStay(Collider other)
    {
        // Apply slowdown to player
        LookToWalk playerController = other.GetComponent<LookToWalk>();
        if (playerController != null)
        {
            playerController.SetSpeedMultiplier(speedMultiplier);

            // Keep jiggling while player is moving through
            if (playerController.GetSpeedMultiplier() < 1f && !isJiggling)
            {
                StartJiggle();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Remove slowdown
        LookToWalk playerController = other.GetComponent<LookToWalk>();
        if (playerController != null)
        {
            isPlayerInBush = false;
            playerController.SetSpeedMultiplier(1f);
            StopBushSound();
        }
    }

    private void StartBushSound()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        if (!bushAudioSource.isPlaying)
        {
            bushAudioSource.Play();
        }

        fadeCoroutine = StartCoroutine(FadeAudio(bushAudioSource.volume, volume, fadeInDuration));
    }

    private void StopBushSound()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeAudio(bushAudioSource.volume, 0f, fadeOutDuration));
    }

    private System.Collections.IEnumerator FadeAudio(float startVolume, float targetVolume, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bushAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        bushAudioSource.volume = targetVolume;

        // Stop playing if volume reached zero
        if (targetVolume == 0f)
        {
            bushAudioSource.Stop();
        }
    }

    private void StartJiggle()
    {
        if (jiggleCoroutine != null)
        {
            StopCoroutine(jiggleCoroutine);
        }
        jiggleCoroutine = StartCoroutine(JiggleCoroutine());
    }

    private System.Collections.IEnumerator JiggleCoroutine()
    {
        isJiggling = true;
        float elapsed = 0f;

        while (elapsed < jiggleDuration)
        {
            // Create random jiggle offset
            float randomX = Random.Range(-jiggleStrength, jiggleStrength);
            float randomZ = Random.Range(-jiggleStrength, jiggleStrength);

            // Apply jiggle with falloff
            float falloff = 1f - (elapsed / jiggleDuration);
            Vector3 jiggleOffset = new Vector3(randomX, 0, randomZ) * falloff;

            transform.localPosition = originalPosition + jiggleOffset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Return to original position
        transform.localPosition = originalPosition;
        isJiggling = false;
    }

    void OnDestroy()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
    }
}