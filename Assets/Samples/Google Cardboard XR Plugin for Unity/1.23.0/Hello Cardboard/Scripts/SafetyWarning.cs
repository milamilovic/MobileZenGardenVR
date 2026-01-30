using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SafetyWarning : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject safetyCanvas;
    [SerializeField] private Image loaderCircle;

    [Header("Settings")]
    [SerializeField] private float displayTime = 8f;
    [SerializeField] private string playerPrefKey = "SafetyWarningShown";
    [SerializeField] private bool showOnlyFirstTime = false;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (showOnlyFirstTime && PlayerPrefs.GetInt(playerPrefKey, 0) == 1)
        {
            safetyCanvas.SetActive(false);
            return;
        }

        ShowWarning();
    }

    private void ShowWarning()
    {
        safetyCanvas.SetActive(true);

        if (loaderCircle != null)
        {
            loaderCircle.fillAmount = 0f;
        }

        StartCoroutine(CountdownAndDismiss());
    }

    private IEnumerator CountdownAndDismiss()
    {
        float elapsedTime = 0f;

        while (elapsedTime < displayTime)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsedTime / displayTime);

            if (loaderCircle != null)
            {
                loaderCircle.fillAmount = progress;
            }

            yield return null; // wait for next frame
        }

        // close after 8s
        DismissWarning();
    }

    public void DismissWarning()
    {
        // Save that the user has seen the warning
        if (showOnlyFirstTime)
        {
            PlayerPrefs.SetInt(playerPrefKey, 1);
            PlayerPrefs.Save();
        }

        safetyCanvas.SetActive(false);
    }

    public void ResetWarning()
    {
        PlayerPrefs.DeleteKey(playerPrefKey);
        PlayerPrefs.Save();
    }
}