using UnityEngine;

public class GazeTutorialSkip : MonoBehaviour, IGazeInteractable
{
    [Header("References")]
    public TutorialController tutorialController;
    public UnityEngine.UI.Image fillCircle;

    public bool CanGaze => true;

    void Start()
    {
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;
    }

    public void OnGazeEnter()
    {
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;
    }

    public void OnGazeExit()
    {
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;
    }

    public void UpdateGazeProgress(float progress)
    {
        if (fillCircle != null)
            fillCircle.fillAmount = progress;
    }

    public void OnGazeActivate()
    {
        Debug.Log("Tutorial skipped!");
        if (tutorialController != null)
            tutorialController.DismissTutorial();
    }
}