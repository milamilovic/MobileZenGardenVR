using System.Collections;
using UnityEngine;

public class GazeGrabbable : MonoBehaviour, IGazeInteractable
{
    [Header("Version Control")]
    [Tooltip("Check for Physics-based version, Uncheck for Simple transform version")]
    public bool usePhysicsVersion = true;

    [Header("Visual Feedback")]
    public Canvas gazeCanvas;
    public UnityEngine.UI.Image fillCircle;

    [Header("Visual Effects")]
    [SerializeField] private GazeVisualEffect visualEffects;

    [Header("Grab Settings")]
    public Transform holdPosition;
    public Vector3 holdOffset = new Vector3(0, -0.3f, 0.5f);
    public bool rotateToFacePlayer = true;

    [Header("Physics Version Settings")]
    [SerializeField] private float holdSmoothness = 15f;

    [Header("Drop Settings")]
    public float heightAboveTerrain = 0.05f;

    [Header("Player Collider Settings")]
    public bool addPlayerCollider = true;

    private bool isGrabbed = false;
    private BoxCollider boxCollider;
    private Rigidbody rb;
    private Collider playerCollider;
    private GameObject playerObject;
    private GameObject playerColliderObject;
    private BoxCollider dynamicPlayerCollider;

    public bool CanGaze => !isGrabbed;

    void Start()
    {
        if (gazeCanvas != null)
            gazeCanvas.enabled = false;
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;

        // Get the BoxCollider component
        boxCollider = GetComponent<BoxCollider>();

        // Get or add Rigidbody (only needed for physics version)
        rb = GetComponent<Rigidbody>();
        if (rb == null && usePhysicsVersion)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        playerObject = GameObject.FindGameObjectWithTag("Player");

        if (usePhysicsVersion && rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            if (boxCollider != null)
                boxCollider.isTrigger = true;

            if (playerObject != null)
            {
                playerCollider = playerObject.GetComponent<Collider>();
            }
        }

        // Get or add visual effects component
        if (visualEffects == null)
        {
            visualEffects = GetComponent<GazeVisualEffect>();
        }
    }

    void Update()
    {
        // Handle input for dropping
        if (Input.GetKeyDown(KeyCode.Space) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (isGrabbed)
            {
                DropObject();
            }
        }

        // Simple version movement (in Update)
        if (!usePhysicsVersion && isGrabbed && holdPosition != null)
        {
            // Follow the hold position
            transform.position = holdPosition.position + holdPosition.TransformDirection(holdOffset);

            if (rotateToFacePlayer)
            {
                transform.rotation = holdPosition.rotation;
            }
        }

        UpdatePlayerCollider();
    }

    void FixedUpdate()
    {
        // Physics version movement
        if (usePhysicsVersion && isGrabbed && holdPosition != null && rb != null)
        {
            // Calculate target position
            Vector3 targetPos = holdPosition.position + holdPosition.TransformDirection(holdOffset);

            // Smooth movement with force towards target
            Vector3 direction = targetPos - rb.position;
            rb.linearVelocity = direction * holdSmoothness;

            // Dampen any unwanted velocity
            rb.angularVelocity = Vector3.zero;

            if (rotateToFacePlayer)
            {
                rb.MoveRotation(holdPosition.rotation);
            }
        }
    }

    void UpdatePlayerCollider()
    {
        if (addPlayerCollider && isGrabbed && playerColliderObject != null && holdPosition != null)
        {
            // Calculate world position where the rake should be
            Vector3 worldRakePos = holdPosition.position + holdPosition.TransformDirection(holdOffset);

            // Position the collider object at the rake's world position
            playerColliderObject.transform.position = worldRakePos;

            // Rotate the collider object to match the rake's world rotation
            playerColliderObject.transform.rotation = holdPosition.rotation;
        }
    }

    void CreatePlayerCollider()
    {
        if (!addPlayerCollider || boxCollider == null)
            return;

        playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("Cannot find Player GameObject!");
            return;
        }

        // Create a child GameObject under the player
        playerColliderObject = new GameObject("RakePlayerCollider");
        playerColliderObject.transform.SetParent(playerObject.transform);

        // Add BoxCollider to this child object
        dynamicPlayerCollider = playerColliderObject.AddComponent<BoxCollider>();

        // Calculate the world-space size of the rake's collider
        Vector3 rakeScale = transform.lossyScale;
        Vector3 worldSize = new Vector3(
            boxCollider.size.x * rakeScale.x,
            boxCollider.size.y * rakeScale.y,
            boxCollider.size.z * rakeScale.z
        );

        // Set the size directly in world space since the child has scale (1,1,1)
        dynamicPlayerCollider.size = worldSize;
        dynamicPlayerCollider.center = new Vector3(0, 0, 0.67f);

        // Initial position/rotation update
        UpdatePlayerCollider();

        Debug.Log($"Created player collider - World size: {worldSize}");
    }

    void RemovePlayerCollider()
    {
        if (playerColliderObject != null)
        {
            Destroy(playerColliderObject);
            playerColliderObject = null;
            dynamicPlayerCollider = null;
            Debug.Log("Removed player collider for rake");
        }
    }

    public void OnGazeEnter()
    {
        if (isGrabbed) return;

        if (gazeCanvas != null)
            gazeCanvas.enabled = true;
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
        if (isGrabbed) return;

        if (gazeCanvas != null)
            gazeCanvas.enabled = false;
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
        if (isGrabbed) return;

        if (fillCircle != null)
            fillCircle.fillAmount = progress;
    }

    public void OnGazeActivate()
    {
        if (isGrabbed)
        {
            DropObject();
        }
        else
        {
            GrabObject();
        }

        if (fillCircle != null)
            fillCircle.fillAmount = 0f;
    }

    void GrabObject()
    {
        isGrabbed = true;

        if (gazeCanvas != null)
            gazeCanvas.enabled = false;

        // Hide visual effects when grabbed
        if (visualEffects != null)
        {
            visualEffects.HideEffects();
        }

        if (usePhysicsVersion)
        {
            // Physics version grab
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = false;
                rb.linearDamping = 5f;
                rb.angularDamping = 5f;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (boxCollider != null)
                boxCollider.isTrigger = false;

            // Ignore collision with player
            if (playerCollider != null && boxCollider != null)
            {
                Physics.IgnoreCollision(boxCollider, playerCollider, true);
            }
        }

        CreatePlayerCollider();

        Debug.Log("Grabbed: " + gameObject.name + " (Physics: " + usePhysicsVersion + ")");
    }

    void DropObject()
    {
        isGrabbed = false;

        RemovePlayerCollider();

        if (usePhysicsVersion && rb != null)
        {
            // Reset velocities before dropping
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Re-enable collision with player
            if (playerCollider != null && boxCollider != null)
            {
                Physics.IgnoreCollision(boxCollider, playerCollider, false);
            }
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100f, ~0))
        {
            Debug.Log("Hit: " + hit.collider.gameObject.name);
            StartCoroutine(FallToGround(hit.point, hit.normal));
        }
        else
        {
            Debug.LogWarning("No ground found below object");

            // Fallback for physics version
            if (usePhysicsVersion)
            {
                if (boxCollider != null)
                    boxCollider.isTrigger = true;
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
            }
        }

        Debug.Log("Dropped: " + gameObject.name + " (Physics: " + usePhysicsVersion + ")");
    }

    IEnumerator FallToGround(Vector3 targetPosition, Vector3 surfaceNormal)
    {
        // Physics version: make kinematic during animation
        if (usePhysicsVersion && rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        // Position slightly above the terrain
        targetPosition += surfaceNormal * heightAboveTerrain;

        // Calculate horizontal rotation (align with ground plane), then add 180° Z rotation
        Quaternion horizontalRotation = Quaternion.FromToRotation(Vector3.up, surfaceNormal);
        Quaternion targetRotation = horizontalRotation * Quaternion.Euler(0f, 0f, 180f);

        float fallDuration = 0.5f;
        float rotateDuration = 0.3f;
        float elapsed = 0f;

        // Fall down without rotating
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;
            float smoothT = t * t;

            transform.position = Vector3.Lerp(startPosition, targetPosition, smoothT);

            yield return null;
        }

        transform.position = targetPosition;

        // Rotate to horizontal orientation with 180° Z flip after landing
        elapsed = 0f;

        while (elapsed < rotateDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotateDuration;

            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        transform.rotation = targetRotation;

        // Re-enable trigger mode after landing for physics version
        if (usePhysicsVersion)
        {
            if (boxCollider != null)
                boxCollider.isTrigger = true;

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}