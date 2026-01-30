using UnityEngine;

public class KoiFishMovement : MonoBehaviour
{
    public GameObject waterObject;
    public float swimSpeed = 1f;
    public float turnSpeed = 2f;
    public float margin = 0.5f;

    private Vector3 waterCenter;
    private Vector3 targetPosition;
    private Bounds waterBounds;

    void Start()
    {
        // Automatically get water center and bounds from the water object
        if (waterObject != null)
        {
            waterCenter = waterObject.transform.position;

            // Get the water's renderer or collider to find its size
            Renderer waterRenderer = waterObject.GetComponent<Renderer>();
            if (waterRenderer != null)
            {
                waterBounds = waterRenderer.bounds;
            }
            else
            {
                Collider waterCollider = waterObject.GetComponent<Collider>();
                if (waterCollider != null)
                {
                    waterBounds = waterCollider.bounds;
                }
                else
                {
                    Debug.LogWarning("Water object has no Renderer or Collider. Using default bounds.");
                    waterBounds = new Bounds(waterCenter, new Vector3(10, 1, 10));
                }
            }
        }

        SetNewRandomTarget();
    }

    void Update()
    {
        // Move towards target
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, swimSpeed * Time.deltaTime);

        // Keep fish within bounds
        KeepInBounds();

        // Rotate towards target
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            targetRotation *= Quaternion.Euler(0, 180, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        // Get new target when close
        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            SetNewRandomTarget();
        }
    }

    void SetNewRandomTarget()
    {
        // Calculate usable area (water bounds minus margin)
        float minX = waterBounds.min.x + margin;
        float maxX = waterBounds.max.x - margin;
        float minZ = waterBounds.min.z + margin;
        float maxZ = waterBounds.max.z - margin;

        // Random position within water bounds
        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);

        targetPosition = new Vector3(randomX, transform.position.y, randomZ);
    }

    void KeepInBounds()
    {
        // Clamp fish position to water bounds
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, waterBounds.min.x + margin, waterBounds.max.x - margin);
        pos.z = Mathf.Clamp(pos.z, waterBounds.min.z + margin, waterBounds.max.z - margin);
        transform.position = pos;

        // If fish is at boundary, get new target
        if (pos != transform.position)
        {
            SetNewRandomTarget();
        }
    }

}