using UnityEngine;

public class TutorialController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject tutorialCanvas;

    [Header("Settings")]
    [SerializeField] private string tutorialShownKey = "TutorialShown";
    [SerializeField] private bool showOnlyFirstTime = false;

    void Start()
    {
        if (showOnlyFirstTime && PlayerPrefs.GetInt(tutorialShownKey, 0) == 1)
        {
            tutorialCanvas.SetActive(false);
            return;
        }

        ShowTutorial();
    }

    private void ShowTutorial()
    {
        tutorialCanvas.SetActive(true);
    }

    public void DismissTutorial()
    {
        if (showOnlyFirstTime)
        {
            PlayerPrefs.SetInt(tutorialShownKey, 1);
            PlayerPrefs.Save();
        }

        tutorialCanvas.SetActive(false);

        Debug.Log("Tutorial dismissed");
    }
}