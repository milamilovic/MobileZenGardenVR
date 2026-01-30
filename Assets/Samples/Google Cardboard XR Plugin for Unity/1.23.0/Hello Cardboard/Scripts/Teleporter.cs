using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [Header("Teleporter Colors")]
    [SerializeField] private Color inactiveColor = Color.white;
    [SerializeField] private Color gazeColor = Color.yellow;

    [Header("Effects")]
    [SerializeField] private AudioClip teleportationSoundEffect;

    [Header("Teleportation Settings")]
    [SerializeField] private GameObject player;
    [SerializeField] private float maxGazeDetectionTime = 2f;
    private float elapsedGazeDetectionTime = 0f;

    private MeshRenderer meshRenderer;
    private bool isColorChanging = false;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Start is called before the first frame update
    private void Start()
    {
        meshRenderer.material.color = inactiveColor;   
    }

    // Update is called once per frame
    private void Update()
    {
        // Gradual color change before teleportation to give visual feedback to the user.
        if (isColorChanging)
        {
            meshRenderer.material.color = Color.Lerp(inactiveColor, gazeColor, elapsedGazeDetectionTime/maxGazeDetectionTime);
            elapsedGazeDetectionTime += Time.deltaTime;

            if(elapsedGazeDetectionTime >= maxGazeDetectionTime)
            {
                isColorChanging = false;
                AudioManager.Instance.PlaySound(teleportationSoundEffect);
                TeleportPlayerToPosition(transform.position);
                meshRenderer.material.color = inactiveColor;
            }
        }
    }

    private void TeleportPlayerToPosition(Vector3 targetPosition)
    {
        Vector3 teleportPosition = new Vector3(targetPosition.x, player.transform.position.y, targetPosition.z);
        player.transform.position = teleportPosition;
    }

    // This method is called by the Main Camera when it starts gazing at this GameObject.
    public void OnPointerEnter()
    {
        Gaze(true);
    }

    // This method is called by the Main Camera when it stops gazing at this GameObject.
    public void OnPointerExit()
    {
        Gaze(false);
    }

    public void Gaze(bool isGazing)
    {
        if (isGazing)
        {
            isColorChanging = true;
            // Instant color change.
            // meshRenderer.material.color = gazeColor;
        }
        else
        {
            elapsedGazeDetectionTime = 0f;
            isColorChanging = false;
            meshRenderer.material.color = inactiveColor;
        }
    }
}
