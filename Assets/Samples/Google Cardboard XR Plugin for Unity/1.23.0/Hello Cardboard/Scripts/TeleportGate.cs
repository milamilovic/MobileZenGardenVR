using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TeleportGate : MonoBehaviour, IGazeInteractable
{
    [Header("Gate References")]
    [SerializeField] private string gateID;

    [Header("Visual Feedback")]
    public Canvas gazeCanvas;
    public UnityEngine.UI.Image fillCircle;

    [Header("Outline Settings")]
    [SerializeField] private Color highlightColor = Color.cyan;
    [SerializeField] private float outlineWidth = 0.05f;

    [Header("Scale Effect")]
    [SerializeField] private float scaleMultiplier = 1.15f;
    [SerializeField] private float scaleSpeed = 5f;

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem gazeParticles;
    [SerializeField] private Transform particleSpawnPoint;

    [Header("Audio")]
    [SerializeField] private AudioClip teleportSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Teleport Settings")]
    [SerializeField] private Transform teleportDestination;
    [SerializeField] private float teleportHeight = 0f;

    private bool isPlayerInside = false;
    private bool isGazingAtThis = false;
    private List<Renderer> renderersWithOutline = new List<Renderer>();
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private static TeleportGate activeGate = null;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Coroutine scaleCoroutine;

    // Outline shader
    private static Shader outlineShader;

    // Can only gaze at OTHER gates, not the one you're standing in
    public bool CanGaze => !isPlayerInside && activeGate != null;
    public string GateID => gateID;
    public Vector3 TeleportPosition => teleportDestination != null
        ? teleportDestination.position + Vector3.up * teleportHeight
        : transform.position + Vector3.up * teleportHeight;

    void Start()
    {
        // Setup audio
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;

        if (gazeCanvas != null)
            gazeCanvas.enabled = false;
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;

        if (gazeParticles != null)
        {
            gazeParticles.Stop();
        }
        else if (particleSpawnPoint != null)
        {
            CreateDefaultParticles();
        }

        if (string.IsNullOrEmpty(gateID))
        {
            gateID = "Gate_" + GetInstanceID();
        }

        if (teleportDestination == null)
        {
            GameObject destObj = new GameObject("TeleportDestination");
            destObj.transform.SetParent(transform);
            destObj.transform.localPosition = Vector3.zero;
            teleportDestination = destObj.transform;
        }

        // Store original scale
        originalScale = transform.localScale;
        targetScale = originalScale;

        // Try to find or create outline shader
        if (outlineShader == null)
        {
            outlineShader = Shader.Find("Custom/Outline");
            if (outlineShader == null)
            {
                // Fallback to a basic shader
                outlineShader = Shader.Find("Unlit/Color");
            }
        }
    }

    void Update()
    {
        // Smooth scale transition
        if (transform.localScale != targetScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
        }
    }

    void CreateDefaultParticles()
    {
        GameObject particlesObj = new GameObject("GazeParticles");
        particlesObj.transform.SetParent(particleSpawnPoint != null ? particleSpawnPoint : transform);
        particlesObj.transform.localPosition = Vector3.zero;

        gazeParticles = particlesObj.AddComponent<ParticleSystem>();
        var main = gazeParticles.main;
        main.startLifetime = 2f;
        main.startSpeed = 5f;
        main.startSize = 0.2f;
        main.startColor = new Color(0.3f, 0.8f, 1f, 0.8f);
        main.maxParticles = 50;
        main.loop = true;

        var emission = gazeParticles.emission;
        emission.rateOverTime = 20f;

        var shape = gazeParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 1f;

        var velocityOverLifetime = gazeParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.y = 3f;

        gazeParticles.Stop();
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger entered by: {other.gameObject.name} with tag: {other.tag}");

        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            activeGate = this;

            Debug.Log($"Player ENTERED gate: {gateID}");

            // Show outlines on all other gates
            if (TeleportGateManager.Instance != null)
            {
                TeleportGateManager.Instance.ShowOutlinesForAllGates(this);
                Debug.Log("Showing outlines on other gates");
            }
            else
            {
                Debug.LogError("TeleportGateManager.Instance is NULL!");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;

            Debug.Log($"Player EXITED gate: {gateID}");

            if (activeGate == this)
            {
                activeGate = null;

                // Hide all outlines
                if (TeleportGateManager.Instance != null)
                {
                    TeleportGateManager.Instance.HideAllOutlines();
                    Debug.Log("Hiding all outlines");
                }
            }

            // Stop particles if player leaves while gazing
            if (gazeParticles != null && gazeParticles.isPlaying)
            {
                gazeParticles.Stop();
            }
        }
    }

    public void OnGazeEnter()
    {
        Debug.Log($"OnGazeEnter called on {gateID}. isPlayerInside={isPlayerInside}, activeGate={activeGate?.gateID ?? "null"}");

        // Can only gaze at this gate if player is in a different gate
        if (isPlayerInside || activeGate == null)
        {
            Debug.Log($"Cannot gaze at {gateID} - wrong conditions");
            return;
        }

        isGazingAtThis = true;

        Debug.Log($"Started GAZING at gate: {gateID}");

        if (gazeCanvas != null)
            gazeCanvas.enabled = true;
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;

        // Start particle effect
        if (gazeParticles != null)
        {
            gazeParticles.Play();
            Debug.Log($"Particles STARTED for {gateID}");
        }
        else
        {
            Debug.LogWarning($"No particles assigned for {gateID}");
        }

        // Start scale up animation
        targetScale = originalScale * scaleMultiplier;
    }

    public void OnGazeExit()
    {
        if (isPlayerInside || activeGate == null) return;

        isGazingAtThis = false;

        Debug.Log($"Stopped GAZING at gate: {gateID}");

        if (gazeCanvas != null)
            gazeCanvas.enabled = false;
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;

        // Stop particle effect
        if (gazeParticles != null)
        {
            gazeParticles.Stop();
            Debug.Log($"Particles STOPPED for {gateID}");
        }

        // Reset scale
        targetScale = originalScale;
    }

    public void UpdateGazeProgress(float progress)
    {
        if (isPlayerInside || activeGate == null) return;

        if (fillCircle != null)
            fillCircle.fillAmount = progress;
    }

    public void OnGazeActivate()
    {
        if (isPlayerInside || activeGate == null)
        {
            Debug.LogWarning($"Cannot activate teleport on {gateID} - wrong conditions");
            return;
        }

        Debug.Log($"TELEPORTING to gate: {gateID} at position {TeleportPosition}");

        // Play sound
        if (audioSource != null && teleportSound != null)
        {
            audioSource.PlayOneShot(teleportSound);
            Debug.Log("Teleport sound played");
        }

        // Teleport player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 oldPos = player.transform.position;
            player.transform.position = TeleportPosition;
            Debug.Log($"Player teleported from {oldPos} to {TeleportPosition}");
        }
        else
        {
            Debug.LogError("Cannot find Player GameObject with 'Player' tag!");
        }

        // Reset UI
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;
        if (gazeCanvas != null)
            gazeCanvas.enabled = false;

        // Stop particles
        if (gazeParticles != null)
        {
            gazeParticles.Stop();
        }

        // Reset scale
        targetScale = originalScale;
    }

    public void ShowOutline(bool show)
    {
        if (show)
        {
            CreateOutline();
        }
        else
        {
            RemoveOutline();
        }
    }

    void CreateOutline()
    {
        // Remove existing outlines first
        RemoveOutline();

        Debug.Log($"Creating outline for gate: {gateID}");

        // Find all MeshRenderers in this gate (excluding UI elements)
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        SkinnedMeshRenderer[] skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        Debug.Log($"Found {renderers.Length} mesh renderers and {skinnedRenderers.Length} skinned mesh renderers in {gateID}");

        // Process MeshRenderers
        foreach (MeshRenderer renderer in renderers)
        {
            // Skip if this is a UI element
            if (renderer.GetComponent<Canvas>() != null)
                continue;

            ApplyOutlineMaterial(renderer);
        }

        // Process SkinnedMeshRenderers
        foreach (SkinnedMeshRenderer renderer in skinnedRenderers)
        {
            ApplyOutlineMaterial(renderer);
        }

        Debug.Log($"Total renderers with outline: {renderersWithOutline.Count}");
    }

    void ApplyOutlineMaterial(Renderer renderer)
    {
        // Store original materials
        if (!originalMaterials.ContainsKey(renderer))
        {
            originalMaterials[renderer] = renderer.materials;
        }

        // Get the mesh to create outline
        MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
        SkinnedMeshRenderer skinnedRenderer = renderer as SkinnedMeshRenderer;

        if (meshFilter == null && skinnedRenderer == null)
        {
            return;
        }

        // Create outline object as child
        GameObject outlineObj = new GameObject(renderer.gameObject.name + "_Outline");
        outlineObj.transform.SetParent(renderer.transform);
        outlineObj.transform.localPosition = Vector3.zero;
        outlineObj.transform.localRotation = Quaternion.identity;
        outlineObj.transform.localScale = Vector3.one;
        outlineObj.layer = renderer.gameObject.layer;

        // Copy the mesh
        if (meshFilter != null)
        {
            MeshFilter outlineMeshFilter = outlineObj.AddComponent<MeshFilter>();
            outlineMeshFilter.sharedMesh = meshFilter.sharedMesh;

            MeshRenderer outlineRenderer = outlineObj.AddComponent<MeshRenderer>();
            Material outlineMat = CreateOutlineMaterial();
            outlineRenderer.material = outlineMat;
            outlineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            outlineRenderer.receiveShadows = false;
        }
        else if (skinnedRenderer != null)
        {
            SkinnedMeshRenderer outlineSkinnedRenderer = outlineObj.AddComponent<SkinnedMeshRenderer>();
            outlineSkinnedRenderer.sharedMesh = skinnedRenderer.sharedMesh;
            outlineSkinnedRenderer.bones = skinnedRenderer.bones;
            outlineSkinnedRenderer.rootBone = skinnedRenderer.rootBone;

            Material outlineMat = CreateOutlineMaterial();
            outlineSkinnedRenderer.material = outlineMat;
            outlineSkinnedRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            outlineSkinnedRenderer.receiveShadows = false;
        }

        renderersWithOutline.Add(renderer);

        Debug.Log($"Applied outline to {renderer.gameObject.name}");
    }

    Material CreateOutlineMaterial()
    {
        Shader shader = Shader.Find("Custom/Outline");

        if (shader != null)
        {
            Material mat = new Material(shader);
            mat.SetColor("_OutlineColor", highlightColor);

            // Set the appropriate property based on shader type
            if (shader.name.Contains("Scale"))
            {
                mat.SetFloat("_OutlineScale", 1.0f + outlineWidth);
            }
            else
            {
                mat.SetFloat("_OutlineWidth", outlineWidth);
            }

            return mat;
        }

        // Fallback: create a simple unlit material
        Material fallbackMat = new Material(Shader.Find("Unlit/Color"));
        fallbackMat.color = highlightColor;
        fallbackMat.renderQueue = 2999;
        return fallbackMat;
    }

    void RemoveOutline()
    {
        // Remove outline GameObjects
        foreach (Renderer renderer in renderersWithOutline)
        {
            if (renderer != null)
            {
                // Find and destroy outline children
                Transform[] children = renderer.GetComponentsInChildren<Transform>();
                foreach (Transform child in children)
                {
                    if (child.name.Contains("_Outline"))
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }
        renderersWithOutline.Clear();
        originalMaterials.Clear();
    }

    void OnDestroy()
    {
        RemoveOutline();
    }
}