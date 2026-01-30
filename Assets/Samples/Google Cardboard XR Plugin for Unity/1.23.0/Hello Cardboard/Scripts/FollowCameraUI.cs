using UnityEngine;

public class FollowCameraUI : MonoBehaviour
{
    public Camera targetCamera;
    public float distance = 1.5f;

    void LateUpdate()
    {
        if (!targetCamera) return;

        // Position in front of camera
        transform.position =
            targetCamera.transform.position +
            targetCamera.transform.forward * distance;

        // Match camera rotation exactly
        transform.rotation = targetCamera.transform.rotation;
    }
}