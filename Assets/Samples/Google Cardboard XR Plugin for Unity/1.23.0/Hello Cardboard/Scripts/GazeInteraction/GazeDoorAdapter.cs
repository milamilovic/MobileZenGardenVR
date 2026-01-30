using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class GazeDoorAdapter : MonoBehaviour, IGazeInteractable
{
    public VRDoubleDoor door;
    public Canvas gazeCanvas;
    public Image fillCircle;
    [Header("Loader Settings")]
    public GameObject loaderToActivate;
    public bool deactivateLoaderWhenDoorsFinish = false;
    [Header("Visual Effects")]
    [SerializeField] private GazeVisualEffect visualEffects;
    private float fillAmount = 0f;
    private bool isWaitingForDoors = false;

    public bool CanGaze => !isWaitingForDoors && !door.IsAnimating();

    void Start()
    {
        Debug.Log("GazeDoorAdapter Start on: " + gameObject.name);
        if (gazeCanvas != null)
            gazeCanvas.enabled = false;
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;

        // Get or add visual effects component
        if (visualEffects == null)
        {
            visualEffects = GetComponent<GazeVisualEffect>();
        }
    }
    public void OnGazeEnter()
    {
        if (isWaitingForDoors)
            return;
        if (gazeCanvas != null)
            gazeCanvas.enabled = true;
        fillAmount = 0f;
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;

        // Show visual effects
        if (visualEffects != null)
        {
            visualEffects.ShowEffects();
        }
    }
    public void OnGazeExit()
    {
        if (isWaitingForDoors)
            return;
        if (gazeCanvas != null)
            gazeCanvas.enabled = false;
        fillAmount = 0f;
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;

        // Hide visual effects
        if (visualEffects != null)
        {
            visualEffects.HideEffects();
        }
    }
    public void UpdateGazeProgress(float progress)
    {
        if (isWaitingForDoors)
            return;
        if (gazeCanvas != null)
            gazeCanvas.enabled = true;
        fillAmount = progress;
        if (fillCircle != null)
            fillCircle.fillAmount = progress;
    }
    public void OnGazeActivate()
    {
        if (isWaitingForDoors)
        {
            Debug.Log("Already waiting for doors, ignoring activation");
            return;
        }
        Debug.Log("Activating doors!");
        door.ToggleDoor();
        // Reset fill
        fillAmount = 0f;
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;
        StartCoroutine(WaitForDoorsAndActivateLoader());
    }
    IEnumerator WaitForDoorsAndActivateLoader()
    {
        isWaitingForDoors = true;

        gazeCanvas.enabled = false;
        fillCircle.enabled = false;

        // Hide visual effects while waiting
        if (visualEffects != null)
        {
            visualEffects.HideEffects();
        }

        Debug.Log("Waiting for doors to finish...");

        while (door.IsAnimating())
        {
            yield return null;
        }
        Debug.Log("Doors finished animating!");

        isWaitingForDoors = false;

        gazeCanvas.enabled = true;
        fillCircle.enabled = true;
        fillCircle.fillAmount = 0f;
    }
}