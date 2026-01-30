using UnityEngine;

public class GazeInteractor : MonoBehaviour
{
    public float gazeTime = 1.5f;
    public LayerMask interactLayer;
    [Header("Raycast Distance")]
    public float maxGazeDistance = 100f;

    private float timer;
    private IGazeInteractable currentInteractable;

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxGazeDistance, interactLayer))
        {

            IGazeInteractable interactable = hit.collider.GetComponent<IGazeInteractable>();

            // No gaze interactable on this object
            if (interactable == null)
            {
                if (currentInteractable != null)
                {
                    currentInteractable.OnGazeExit();
                    currentInteractable = null;
                }
                timer = 0;
                return;
            }

            // Gaze is temporarily blocked (doors animating)
            if (!interactable.CanGaze)
            {
                if (currentInteractable != null)
                {
                    currentInteractable.OnGazeExit();
                    currentInteractable = null;
                }
                timer = 0;
                return;
            }


            if (interactable != null)
            {

                // New object
                if (currentInteractable != interactable)
                {
                    // Exit previous
                    if (currentInteractable != null)
                        currentInteractable.OnGazeExit();

                    currentInteractable = interactable;
                    currentInteractable.OnGazeEnter();
                    timer = 0;
                }

                // update progress
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / gazeTime);
                currentInteractable.UpdateGazeProgress(progress);

                // activete when filled
                if (timer >= gazeTime)
                {
                    currentInteractable.OnGazeActivate();
                    timer = 0;
                }
            }
            else
            {
                Debug.LogWarning("No IGazeInteractable on " + hit.collider.name);
            }
        }
        else
        {
            // exit gaze
            if (currentInteractable != null)
            {
                currentInteractable.OnGazeExit();
                currentInteractable = null;
            }
            timer = 0;
        }
    }
}