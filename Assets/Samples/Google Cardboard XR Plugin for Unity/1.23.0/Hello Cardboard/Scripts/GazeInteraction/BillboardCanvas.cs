using UnityEngine;

public class BillboardCanvas : MonoBehaviour
{
    private Camera mainCamera;

    [Header("Settings")]
    [SerializeField] private bool lockY = true;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera == null)
            return;

        Vector3 direction = mainCamera.transform.position - transform.position;

        if (lockY)
        {
            direction.y = 0;
        }

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(-direction);
        }
    }
}