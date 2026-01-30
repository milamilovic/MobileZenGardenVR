using UnityEngine;

public class RakeDeformer : MonoBehaviour
{
    [Header("References")]
    public SandDeformation sandDeformation;
    public Transform[] rakeForks;

    [Header("Deformation Settings")]
    public float deformRadius = 0.05f;
    public float deformStrength = 0.15f;
    public float minVelocityThreshold = 0.1f;
    public LayerMask sandLayer;

    [Header("Trail Settings")]
    public int forkPointsCount = 5;
    public float forkLength = 0.15f;

    private Vector3[] previousPositions;
    private bool isDeforming = false;

    void Start()
    {
        if (rakeForks == null || rakeForks.Length == 0)
        {
            Debug.LogError("No rake forks assigned to RakeDeformer!");
            return;
        }

        // Initialize previous positions
        previousPositions = new Vector3[rakeForks.Length];
        for (int i = 0; i < rakeForks.Length; i++)
        {
            if (rakeForks[i] != null)
                previousPositions[i] = rakeForks[i].position;
        }

        // Find sand deformation if not assigned
        if (sandDeformation == null)
        {
            sandDeformation = FindObjectOfType<SandDeformation>();
            if (sandDeformation == null)
            {
                Debug.LogError("No SandDeformation found in scene!");
            }
        }
    }

    void FixedUpdate()
    {
        if (sandDeformation == null || rakeForks == null)
            return;

        bool anyForkTouchingSand = false;

        for (int i = 0; i < rakeForks.Length; i++)
        {
            if (rakeForks[i] == null)
                continue;

            // Calculate velocity
            Vector3 velocity = (rakeForks[i].position - previousPositions[i]) / Time.fixedDeltaTime;
            float speed = velocity.magnitude;

            // Check multiple points along the fork for better trail continuity
            for (int j = 0; j < forkPointsCount; j++)
            {
                float t = j / (float)(forkPointsCount - 1);
                Vector3 checkPoint = Vector3.Lerp(rakeForks[i].position,
                                                   rakeForks[i].position - rakeForks[i].up * forkLength,
                                                   t);

                RaycastHit hit;
                // Cast a small sphere to detect sand contact
                if (Physics.SphereCast(checkPoint + Vector3.up * 0.05f, deformRadius * 0.5f,
                                       Vector3.down, out hit, 0.15f, sandLayer))
                {
                    anyForkTouchingSand = true;

                    // Only deform if moving fast enough
                    if (speed > minVelocityThreshold)
                    {
                        // Deform at contact point
                        sandDeformation.DeformSand(hit.point, deformRadius, deformStrength);
                    }
                }
            }

            // Update previous position
            previousPositions[i] = rakeForks[i].position;
        }

        isDeforming = anyForkTouchingSand;
    }

    // Visual debug helper
    void OnDrawGizmos()
    {
        if (rakeForks == null)
            return;

        Gizmos.color = isDeforming ? Color.green : Color.yellow;

        foreach (Transform fork in rakeForks)
        {
            if (fork != null)
            {
                // Draw fork points
                for (int i = 0; i < forkPointsCount; i++)
                {
                    float t = i / (float)(forkPointsCount - 1);
                    Vector3 point = Vector3.Lerp(fork.position,
                                                  fork.position - fork.up * forkLength,
                                                  t);
                    Gizmos.DrawWireSphere(point, deformRadius);
                }
            }
        }
    }

    public bool IsDeforming => isDeforming;
}