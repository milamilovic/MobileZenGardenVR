using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RecenterController : MonoBehaviour
{
    [Header("Activation Settings")]
    [SerializeField] private float tiltActivationAngle = 30f;
    [SerializeField] private float tiltActivationTime = 2f;

    [Header("Recenter Settings")]
    [SerializeField] private float holdTime = 2.5f;
    [SerializeField] private float rotationThreshold = 2f;

    [Header("Menu Settings")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private float menuDistance = 2f;
    [SerializeField] private VRMenuButton resumeButton;
    [SerializeField] private VRMenuButton pauseButton;
    [SerializeField] private VRMenuButton quitButton;
    [SerializeField] private float gazeActivationTime = 1.5f;

    [Header("Visual Feedback")]
    [SerializeField] private Image progressCircle;
    [SerializeField] private Text feedbackText;
    [SerializeField] private Image tiltIndicator;

    [Header("Debug Panel (Mobile)")]
    [SerializeField] private Text debugText;

    [Header("Editor Testing")]
    [SerializeField] private bool enableEditorSimulation = true;
    [SerializeField] private KeyCode tiltLeftKey = KeyCode.L;
    [SerializeField] private KeyCode tiltRightKey = KeyCode.R;

    private float currentHoldTime = 0f;
    private float currentTiltTime = 0f;
    private Quaternion lastRotation;
    private bool isHolding = false;
    private bool isRecenterActivated = false;
    private bool isMenuActivated = false;
    private Camera mainCamera;
    private float simulatedTilt = 0f;

    private float initialYaw = 0f;
    private bool isInitialized = false;

    // Menu gaze tracking
    private bool isMenuOpen = false;
    private VRMenuButton currentGazedButton = null;
    private float currentGazeTime = 0f;
    private bool isPaused = false;

    void Start()
    {
        mainCamera = Camera.main;

        // is gyroscope available
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            Input.gyro.updateInterval = 0.01f;
            Debug.Log("Gyroscope enabled!");
        }
        else
        {
            Debug.LogWarning("Gyroscope not supported on this device!");
            if (debugText != null)
                debugText.text = "Gyroscope NOT supported!";
        }

        lastRotation = mainCamera.transform.rotation;

        HideFeedback();
        HideTiltIndicator();

        // Setup menu
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        // Setup button listeners
        if (resumeButton != null)
            resumeButton.OnButtonActivated += ResumeGame;
        if (pauseButton != null)
            pauseButton.OnButtonActivated += PauseGame;
        if (quitButton != null)
            quitButton.OnButtonActivated += QuitGame;

        Invoke("InitializeRecenter", 0.5f);
    }

    private void InitializeRecenter()
    {
        initialYaw = mainCamera.transform.eulerAngles.y;
        isInitialized = true;
        Debug.Log($"Recenter initialized. Initial Yaw: {initialYaw}");
    }

    void Update()
    {
        if (!isInitialized)
            return;

        // If menu is open, handle gaze selection
        if (isMenuOpen)
        {
            HandleGazeSelection();
        }
        else
        {
            // Check for tilt activation
            if (!isRecenterActivated && !isMenuActivated)
            {
                CheckTiltActivation();
            }
            else if (isRecenterActivated)
            {
                CheckRecenter();
            }
            else if (isMenuActivated)
            {
                CheckMenuHold();
            }
        }
    }

    private void CheckTiltActivation()
    {
        // check head angle
        float rollAngle = GetRollAngle();

        // Left tilt - recenter
        if (rollAngle < -tiltActivationAngle)
        {
            currentTiltTime += Time.deltaTime;
            ShowTiltIndicator(currentTiltTime / tiltActivationTime, "left");

            if (currentTiltTime >= tiltActivationTime)
            {
                ActivateRecenter();
            }
        }
        // Right tilt - menu
        else if (rollAngle > tiltActivationAngle)
        {
            currentTiltTime += Time.deltaTime;
            ShowTiltIndicator(currentTiltTime / tiltActivationTime, "right");

            if (currentTiltTime >= tiltActivationTime)
            {
                ActivateMenu();
            }
        }
        else
        {
            currentTiltTime = 0f;
            HideTiltIndicator();
        }
    }

    private float GetRollAngle()
    {
#if UNITY_EDITOR
        if (enableEditorSimulation)
        {
            if (Input.GetKey(tiltLeftKey))
                simulatedTilt = Mathf.Lerp(simulatedTilt, -45f, Time.deltaTime * 3f);
            else if (Input.GetKey(tiltRightKey))
                simulatedTilt = Mathf.Lerp(simulatedTilt, 45f, Time.deltaTime * 3f);
            else
                simulatedTilt = Mathf.Lerp(simulatedTilt, 0f, Time.deltaTime * 5f);

            return simulatedTilt;
        }
        else
        {
            float roll = mainCamera.transform.localEulerAngles.z;
            return roll > 180f ? roll - 360f : roll;
        }
#else
        // use camera rotation
        float roll = mainCamera.transform.localEulerAngles.z;
        if (roll > 180f) roll -= 360f;
        return roll;
#endif
    }

    private void ActivateRecenter()
    {
        isRecenterActivated = true;
        currentTiltTime = 0f;
        simulatedTilt = 0f;

        HideTiltIndicator();
        ShowFeedback("recenter");

        Debug.Log("Recenter activated! Hold head still...");
    }

    private void ActivateMenu()
    {
        isMenuActivated = true;
        currentTiltTime = 0f;
        simulatedTilt = 0f;

        HideTiltIndicator();
        ShowFeedback("menu");

        Debug.Log("Menu opening activated! Hold head still...");
    }

    private void CheckRecenter()
    {
        // movement since the last frame
        float rotationDelta = Quaternion.Angle(lastRotation, mainCamera.transform.rotation);

        // is that still enough
        if (rotationDelta < rotationThreshold)
        {
            // still - increase the timer
            if (!isHolding)
            {
                isHolding = true;
            }

            currentHoldTime += Time.deltaTime;
            UpdateFeedback(currentHoldTime / holdTime);

            if (currentHoldTime >= holdTime)
            {
                RecenterView();
                ResetActivation();
            }
        }
        else
        {
            if (isHolding)
            {
                currentHoldTime = 0f;
                isHolding = false;
                UpdateFeedback(0f);
            }
        }

        lastRotation = mainCamera.transform.rotation;
    }

    private void CheckMenuHold()
    {
        float rotationDelta = Quaternion.Angle(lastRotation, mainCamera.transform.rotation);

        if (rotationDelta < rotationThreshold)
        {
            if (!isHolding)
            {
                isHolding = true;
            }

            currentHoldTime += Time.deltaTime;
            UpdateFeedback(currentHoldTime / holdTime);

            // has time passed
            if (currentHoldTime >= holdTime)
            {
                OpenMenu();
                ResetActivation();
            }
        }
        else
        {
            // reset timer
            if (isHolding)
            {
                currentHoldTime = 0f;
                isHolding = false;
                UpdateFeedback(0f);
            }
        }

        // remember rotation for the next frame
        lastRotation = mainCamera.transform.rotation;
    }

    private void RecenterView()
    {
        float currentYaw = mainCamera.transform.eulerAngles.y;

        // Rotate Player GameObject to compensate
        transform.Rotate(0, -currentYaw, 0, Space.World);

        Debug.Log($"Recentered! Yaw was: {currentYaw}");

        if (feedbackText != null)
        {
            feedbackText.text = "Recentered!";
        }

        Invoke("HideFeedbackDelayed", 1f);
    }

    private void OpenMenu()
    {
        if (menuPanel == null) return;

        isMenuOpen = true;

        // Position menu in front of player
        Vector3 menuPosition = mainCamera.transform.position + mainCamera.transform.forward * menuDistance;
        menuPanel.transform.position = menuPosition;

        // Make menu face the camera
        menuPanel.transform.LookAt(mainCamera.transform);
        menuPanel.transform.Rotate(0, 180, 0);

        menuPanel.SetActive(true);

        Debug.Log("Menu opened!");
    }

    private void HandleGazeSelection()
    {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;

        VRMenuButton hitButton = null;

        // Check if looking at a button
        if (Physics.Raycast(ray, out hit, menuDistance + 1f))
        {
            hitButton = hit.collider.GetComponent<VRMenuButton>();
        }

        // If looking at a button
        if (hitButton != null)
        {
            if (currentGazedButton == hitButton)
            {
                // Continue gazing at same button
                currentGazeTime += Time.unscaledDeltaTime;

                // Update button progress
                hitButton.UpdateGazeProgress(currentGazeTime / gazeActivationTime);

                // Activate button after gaze time
                if (currentGazeTime >= gazeActivationTime)
                {
                    hitButton.ActivateButton();
                    ResetGaze();
                }
            }
            else
            {
                // Started looking at new button
                if (currentGazedButton != null)
                {
                    currentGazedButton.ResetGaze();
                }

                currentGazedButton = hitButton;
                currentGazeTime = 0f;
                hitButton.StartGaze();
            }
        }
        else
        {
            // Not looking at any button
            if (currentGazedButton != null)
            {
                currentGazedButton.ResetGaze();
            }
            ResetGaze();
        }
    }

    private void ResetGaze()
    {
        currentGazedButton = null;
        currentGazeTime = 0f;
    }

    // Button functions
    public void ResumeGame()
    {
        CloseMenu();
        if (isPaused)
        {
            Time.timeScale = 1f;
            isPaused = false;
        }
        Debug.Log("Game resumed!");
    }

    public void PauseGame()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (pauseButton != null)
        {
            Text buttonText = pauseButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = isPaused ? "Resume" : "Pause";
            }
        }

        Debug.Log(isPaused ? "Game paused!" : "Game resumed!");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void CloseMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        isMenuOpen = false;
        ResetGaze();

        Debug.Log("Menu closed!");
    }

    private void ResetActivation()
    {
        currentHoldTime = 0f;
        isHolding = false;
        isRecenterActivated = false;
        isMenuActivated = false;

        HideFeedback();
    }

    private void ShowTiltIndicator(float progress, string direction)
    {
        if (tiltIndicator != null)
        {
            tiltIndicator.gameObject.SetActive(true);

            if (tiltIndicator.type == Image.Type.Filled)
            {
                tiltIndicator.fillAmount = progress;
            }
        }

        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = direction == "left" ?
                "Tilt left to recenter..." :
                "Tilt right to open menu...";
        }
    }

    private void HideTiltIndicator()
    {
        if (tiltIndicator != null)
        {
            tiltIndicator.gameObject.SetActive(false);
        }

        if (feedbackText != null && !isRecenterActivated && !isMenuActivated)
        {
            feedbackText.gameObject.SetActive(false);
        }
    }

    private void ShowFeedback(string action)
    {
        if (progressCircle != null)
            progressCircle.gameObject.SetActive(true);

        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = action == "recenter" ?
                "Hold still to recenter..." :
                "Hold still to open menu...";
        }
    }

    private void UpdateFeedback(float progress)
    {
        if (progressCircle != null)
        {
            progressCircle.fillAmount = progress;
        }
    }

    private void HideFeedback()
    {
        if (progressCircle != null)
            progressCircle.gameObject.SetActive(false);

        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);
    }

    private void HideFeedbackDelayed()
    {
        HideFeedback();
    }
}