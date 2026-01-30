using UnityEngine;
using System;
using Image = UnityEngine.UI.Image;

public class VRMenuButton : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private Image gazeProgressCircle;

    public event Action OnButtonActivated;

    private bool isGazing = false;

    void Start()
    {
        if (gazeProgressCircle != null)
        {
            gazeProgressCircle.gameObject.SetActive(false);
            gazeProgressCircle.fillAmount = 0f;
        }
    }

    public void StartGaze()
    {
        isGazing = true;

        if (gazeProgressCircle != null)
        {
            gazeProgressCircle.gameObject.SetActive(true);
        }
    }

    public void UpdateGazeProgress(float progress)
    {
        if (gazeProgressCircle != null)
        {
            gazeProgressCircle.fillAmount = progress;
        }
    }

    public void ResetGaze()
    {
        isGazing = false;

        if (gazeProgressCircle != null)
        {
            gazeProgressCircle.gameObject.SetActive(false);
            gazeProgressCircle.fillAmount = 0f;
        }
    }

    public void ActivateButton()
    {
        OnButtonActivated?.Invoke();

        Invoke("ResetColor", 0.3f);
    }
}