using UnityEngine;
using System.Collections.Generic;

public class GazeVisualEffect : MonoBehaviour
{
    [Header("Outline Settings")]
    [SerializeField] private bool enableOutline = true;
    [SerializeField] private Color outlineColor = Color.cyan;
    [SerializeField] private float outlineWidth = 0.05f;

    [Header("Scale Settings")]
    [SerializeField] private bool enableScale = true;
    [SerializeField] private float scaleMultiplier = 1.15f;
    [SerializeField] private float scaleSpeed = 5f;

    private List<Renderer> renderersWithOutline = new List<Renderer>();
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private Vector3 originalScale;
    private Vector3 targetScale;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        // Smooth scale transition
        if (enableScale && transform.localScale != targetScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
        }
    }

    public void ShowEffects()
    {
        if (enableOutline)
        {
            CreateOutline();
        }

        if (enableScale)
        {
            targetScale = originalScale * scaleMultiplier;
        }
    }

    public void HideEffects()
    {
        if (enableOutline)
        {
            RemoveOutline();
        }

        if (enableScale)
        {
            targetScale = originalScale;
        }
    }

    void CreateOutline()
    {
        // Remove existing outlines first
        RemoveOutline();

        // Find all MeshRenderers in this object (excluding UI elements)
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        SkinnedMeshRenderer[] skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

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
    }

    Material CreateOutlineMaterial()
    {
        Shader shader = Shader.Find("Custom/Outline");

        if (shader != null)
        {
            Material mat = new Material(shader);
            mat.SetColor("_OutlineColor", outlineColor);

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
        fallbackMat.color = outlineColor;
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