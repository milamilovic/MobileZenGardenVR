using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookToWalk : MonoBehaviour
{
    private bool isWalking = false;
    private Camera mainCamera;

    [Header("Movement Settings")]
    [SerializeField] private float walkingSpeed = 3.0f;
    [SerializeField] private float minimumAngleTreshold = 35.0f;
    [SerializeField] private float maximumAngleTreshold = 90.0f;

    [Header("Dead Zone Settings")]
    [SerializeField] private float deadZoneAngle = 30.0f;
    [SerializeField] private float fullSpeedAngle = 45.0f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip walkingAudioEffect;
    [SerializeField] private AudioClip sandWalkingAudioEffect;
    [SerializeField] private float sandSoundSpeed = 1.5f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 2f;

    [Header("Vignette")]
    [SerializeField] private VignetteController vignetteController;

    private AudioSource walkingAudioSource;
    private Rigidbody rb;
    private bool isOnSand = true;
    private float speedMultiplier = 1f;
    private float currentSpeedFactor = 0f;

    void Start()
    {
        mainCamera = Camera.main;
        walkingAudioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float headTiltAngle = GetNormalizedHeadTilt();

        bool wasWalking = isWalking;

        if (headTiltAngle < deadZoneAngle)
        {
            isWalking = false;
            currentSpeedFactor = 0f;
        }
        else if (headTiltAngle >= deadZoneAngle && headTiltAngle <= maximumAngleTreshold)
        {
            isWalking = true;

            if (headTiltAngle < fullSpeedAngle)
            {
                // gradual speed up from dead zone to full speed
                currentSpeedFactor = Mathf.InverseLerp(deadZoneAngle, fullSpeedAngle, headTiltAngle);
            }
            else
            {
                // max speed
                currentSpeedFactor = 1f;
            }

            // show vignette
            if (vignetteController != null && !isWalking)
            {
                vignetteController.ShowVignette();
            }
        }
        else
        {
            // out of bounds
            isWalking = false;
            currentSpeedFactor = 0f;
        }

        if (vignetteController != null)
        {
            if (isWalking && !wasWalking)
            {
                vignetteController.ShowVignette();
            }
            else if (!isWalking && wasWalking)
            {
                vignetteController.HideVignette();
            }
        }

        CheckGroundSurface();
    }

    private void FixedUpdate()
    {
        if (isWalking)
        {
            MovePlayer();

            AudioClip currentFootstepSound = isOnSand ? sandWalkingAudioEffect : walkingAudioEffect;
            float basePitch = isOnSand ? sandSoundSpeed : 1.0f;
            float adjustedPitch = basePitch * currentSpeedFactor;

            if (!walkingAudioSource.isPlaying || walkingAudioSource.clip != currentFootstepSound)
            {
                walkingAudioSource.clip = currentFootstepSound;
                walkingAudioSource.pitch = adjustedPitch;
                walkingAudioSource.Play();
            }
            else
            {
                walkingAudioSource.pitch = adjustedPitch;
            }
        }
        else
        {
            walkingAudioSource.Stop();
        }
    }

    private void CheckGroundSurface()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance))
        {
            isOnSand = hit.collider.gameObject.name.Contains("SandPlane") ||
                       hit.collider.gameObject.name.Contains("Sand");
        }
        else
        {
            isOnSand = false;
        }
    }

    private void MovePlayer()
    {
        Vector3 movementVector = new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z);

        // currentSpeedFactor (dead zone factor) + speedMultiplier (bush factor)
        Vector3 movement = movementVector.normalized * walkingSpeed * currentSpeedFactor * speedMultiplier * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);
    }

    // 0-360 -> absolute tilt value
    private float GetNormalizedHeadTilt()
    {
        float angle = mainCamera.transform.eulerAngles.x;

        // Unity returns 0-360, convert to -180 to 180 and take the abs
        if (angle > 180f)
            angle -= 360f;

        // Only allow looking down
        return Mathf.Max(0f, angle);
    }

    // For bush slowed down speed
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    public float GetSpeedMultiplier()
    {
        return speedMultiplier;
    }
}