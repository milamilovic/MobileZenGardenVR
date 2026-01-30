using UnityEngine;
using UnityEngine.UI;

public class VignetteController : MonoBehaviour
{
    [Header("Vignette Settings")]
    public Image vignetteImage;
    public float maxAlpha = 0.7f; // max vignette visibility
    public float fadeSpeed = 2f;

    private float targetAlpha = 0f;
    private Color vignetteColor;

    void Start()
    {
        if (vignetteImage == null)
        {
            vignetteImage = GetComponent<Image>();
        }

        vignetteColor = vignetteImage.color;
        vignetteColor.a = 0f;
        vignetteImage.color = vignetteColor;
    }

    void Update()
    {
        // Smooth transition to target alpha value
        vignetteColor.a = Mathf.Lerp(vignetteColor.a, targetAlpha, Time.deltaTime * fadeSpeed);
        vignetteImage.color = vignetteColor;
    }

    public void ShowVignette()
    {
        targetAlpha = maxAlpha;
    }

    public void HideVignette()
    {
        targetAlpha = 0f;
    }
}