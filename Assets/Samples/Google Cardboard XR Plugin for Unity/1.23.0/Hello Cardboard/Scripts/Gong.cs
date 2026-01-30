using UnityEngine;

public class VRGong : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip gongSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Impact Settings")]
    [SerializeField] private float cooldownTime = 0.5f;

    private float lastHitTime = -999f;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.clip = gongSound;

        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check cooldown
        if (Time.time - lastHitTime < cooldownTime)
            return;

        PlayGong();
        lastHitTime = Time.time;
    }

    void PlayGong()
    {
        audioSource.Play();
        Debug.Log($"Gong hit");
    }
}