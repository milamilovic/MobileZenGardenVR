using UnityEngine;
using System.Collections;

public class Lamp : MonoBehaviour, IGazeInteractable
{
    [Header("Light Settings")]
    [SerializeField] private Light lampLight;

    [Header("Audio")]
    [SerializeField] private AudioClip switchOnSound;
    [SerializeField] private AudioClip switchOffSound;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float onSoundPauseDuration = 1f;
    [SerializeField] private float offSoundPauseDuration = 0.7f;
    [SerializeField] private float lightOffDelay = 0.1f;

    [Header("Gaze Visual Feedback")]
    public Canvas gazeCanvas;
    public UnityEngine.UI.Image fillCircle;

    [Header("Visual Effects")]
    [SerializeField] private GazeVisualEffect visualEffects;

    private bool isOn = false;
    private bool isWaitingForSound = false;

    public bool CanGaze => !isWaitingForSound;

    void Start()
    {
        // Setup audio source
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;

        // Setup light
        if (lampLight == null)
        {
            // Try to find a light in children
            lampLight = GetComponentInChildren<Light>();

            // If still null, create one
            if (lampLight == null)
            {
                GameObject lightObj = new GameObject("LampLight");
                lightObj.transform.SetParent(transform);
                lightObj.transform.localPosition = Vector3.zero;
                lampLight = lightObj.AddComponent<Light>();
                lampLight.type = LightType.Point;
                lampLight.range = 5f;
                lampLight.intensity = 1f;
                lampLight.color = new Color(1f, 0.9f, 0.7f);
            }
        }

        // Get or add visual effects component
        if (visualEffects == null)
        {
            visualEffects = GetComponent<GazeVisualEffect>();
        }

        // Initialize gaze UI
        if (gazeCanvas != null)
            gazeCanvas.enabled = false;
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;

        // Initialize state
        SetLampState(isOn, playSound: false);
    }

    public void OnGazeEnter()
    {
        if (isWaitingForSound)
            return;

        if (gazeCanvas != null)
            gazeCanvas.enabled = true;
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;

        // Show visual effects
        if (visualEffects != null)
        {
            visualEffects.ShowEffects();
        }

        Debug.Log("Looking at lamp");
    }

    public void OnGazeExit()
    {
        if (isWaitingForSound)
            return;

        if (gazeCanvas != null)
            gazeCanvas.enabled = false;
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;

        // Hide visual effects
        if (visualEffects != null)
        {
            visualEffects.HideEffects();
        }

        Debug.Log("Stopped looking at lamp");
    }

    public void UpdateGazeProgress(float progress)
    {
        if (isWaitingForSound)
            return;

        if (fillCircle != null)
            fillCircle.fillAmount = progress;
    }

    public void OnGazeActivate()
    {
        if (isWaitingForSound)
        {
            Debug.Log("Waiting for sound to finish, ignoring activation");
            return;
        }

        ToggleLamp();

        // Reset fill
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;
    }

    void ToggleLamp()
    {
        isOn = !isOn;

        if (isOn)
        {
            // Turning ON: immediate
            SetLampState(true, playSound: true);
            StartCoroutine(WaitForSound(onSoundPauseDuration));
        }
        else
        {
            // Turning OFF: play sound first, then turn off light after delay
            StartCoroutine(TurnOffSequence());
        }

        Debug.Log($"Lamp toggling to {(isOn ? "ON" : "OFF")}");
    }

    IEnumerator TurnOffSequence()
    {
        isWaitingForSound = true;

        // Hide gaze UI during pause
        if (gazeCanvas != null)
            gazeCanvas.enabled = false;
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;

        // Hide visual effects
        if (visualEffects != null)
        {
            visualEffects.HideEffects();
        }

        // Play the sound first
        if (audioSource != null && switchOffSound != null)
        {
            audioSource.PlayOneShot(switchOffSound);
            Debug.Log("Playing turn off sound");
        }

        // Wait before actually turning off the light
        yield return new WaitForSeconds(lightOffDelay);

        // Now turn off the light
        SetLampState(false, playSound: false);
        Debug.Log("Light turned off");

        // Wait for the rest of the sound to finish
        float remainingWait = offSoundPauseDuration - lightOffDelay;
        if (remainingWait > 0)
        {
            yield return new WaitForSeconds(remainingWait);
        }

        Debug.Log("Sound finished, lamp ready for interaction");
        isWaitingForSound = false;
    }

    IEnumerator WaitForSound(float duration)
    {
        isWaitingForSound = true;

        // Hide gaze UI during pause
        if (gazeCanvas != null)
            gazeCanvas.enabled = false;
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;

        // Hide visual effects
        if (visualEffects != null)
        {
            visualEffects.HideEffects();
        }

        Debug.Log($"Waiting for lamp sound to finish ({duration}s)...");

        yield return new WaitForSeconds(duration);

        Debug.Log("Sound finished, lamp ready for interaction");

        isWaitingForSound = false;
    }

    void SetLampState(bool turnOn, bool playSound)
    {
        // Toggle light
        if (lampLight != null)
        {
            lampLight.enabled = turnOn;
        }

        // Play sound
        if (playSound && audioSource != null)
        {
            AudioClip soundToPlay = turnOn ? switchOnSound : switchOffSound;
            if (soundToPlay != null)
            {
                audioSource.PlayOneShot(soundToPlay);
            }
        }
    }
}