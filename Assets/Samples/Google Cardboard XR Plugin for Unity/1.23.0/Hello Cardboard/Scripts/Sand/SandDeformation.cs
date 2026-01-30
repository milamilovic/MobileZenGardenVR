using UnityEngine;
using System.Collections.Generic;

public class SandDeformation : MonoBehaviour
{
    [Header("Sand Settings")]
    public int resolution = 512;

    [Header("Trail/Darkness Settings")]
    [Range(0f, 1f)]
    public float trailDarkness = 0.7f;
    public float trailFadeSpeed = 0.2f;

    [Header("Compute Shader")]
    public ComputeShader deformationCompute;

    [Header("Performance")]
    [Tooltip("Apply darkness fading every N frames")]
    public int darknessFadeInterval = 10;

    private RenderTexture darknessMap;
    private RenderTexture tempDarknessMap;
    private Material sandMaterial;

    private int kernelMarkDarkness;
    private int kernelFadeDarkness;
    private int darknessFrameCounter = 0;

    // Cache mesh bounds for accurate UV calculations
    private Vector3 meshSize;
    private Vector3 meshCenter;

    private struct DeformationPoint
    {
        public Vector2 uv;
        public float radius;
        public float strength;
    }

    private List<DeformationPoint> pendingDeformations = new List<DeformationPoint>();

    void Start()
    {
        InitializeDarknessMap();
        SetupSandMaterial();
        InitializeComputeShader();
        CacheMeshBounds();
    }

    void CacheMeshBounds()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            Bounds localBounds = meshFilter.sharedMesh.bounds;
            meshSize = Vector3.Scale(localBounds.size, transform.lossyScale);
            meshCenter = transform.TransformPoint(localBounds.center);
        }
        else
        {
            // using transform scale as fallback
            meshSize = transform.lossyScale;
            meshCenter = transform.position;
        }
    }

    void InitializeDarknessMap()
    {
        darknessMap = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBHalf);
        darknessMap.enableRandomWrite = true;
        darknessMap.filterMode = FilterMode.Bilinear;
        darknessMap.wrapMode = TextureWrapMode.Clamp;
        darknessMap.useMipMap = false;
        darknessMap.autoGenerateMips = false;
        darknessMap.anisoLevel = 0;
        darknessMap.Create();

        tempDarknessMap = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBHalf);
        tempDarknessMap.enableRandomWrite = true;
        tempDarknessMap.filterMode = FilterMode.Bilinear;
        tempDarknessMap.wrapMode = TextureWrapMode.Clamp;
        tempDarknessMap.useMipMap = false;
        tempDarknessMap.autoGenerateMips = false;
        tempDarknessMap.anisoLevel = 0;
        tempDarknessMap.Create();

        // initialization without trails
        RenderTexture.active = darknessMap;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;

        Graphics.Blit(darknessMap, tempDarknessMap);
    }

    void SetupSandMaterial()
    {
        sandMaterial = GetComponent<Renderer>().material;
        sandMaterial.SetTexture("_DarknessMap", darknessMap);
    }

    void InitializeComputeShader()
    {
        if (deformationCompute == null)
        {
            Debug.LogError("Deformation Compute Shader not assigned!");
            return;
        }

        kernelMarkDarkness = deformationCompute.FindKernel("MarkDarkness");
        kernelFadeDarkness = deformationCompute.FindKernel("FadeDarkness");
    }

    void LateUpdate()
    {
        if (deformationCompute == null)
            return;

        // Apply all pending trail marks
        if (pendingDeformations.Count > 0)
        {
            ApplyDarkness();
            pendingDeformations.Clear();
        }

        // Fade darkness periodically
        darknessFrameCounter++;
        if (darknessFrameCounter >= darknessFadeInterval)
        {
            FadeDarkness();
            darknessFrameCounter = 0;
        }
    }

    public void DeformSand(Vector3 worldPosition, float radius, float strength)
    {
        // Convert world position to UV
        Vector3 relativePos = worldPosition - meshCenter;
        Vector2 uv = new Vector2(
            (relativePos.x / meshSize.x) + 0.5f,
            (relativePos.z / meshSize.z) + 0.5f
        );

        if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
            return;

        float uvRadius = radius / Mathf.Max(meshSize.x, meshSize.z);

        pendingDeformations.Add(new DeformationPoint
        {
            uv = uv,
            radius = uvRadius,
            strength = strength
        });
    }

    void ApplyDarkness()
    {
        deformationCompute.SetTexture(kernelMarkDarkness, "DarknessMap", darknessMap);
        deformationCompute.SetTexture(kernelMarkDarkness, "Result", tempDarknessMap);

        foreach (var deform in pendingDeformations)
        {
            deformationCompute.SetVector("DeformCenter", new Vector4(deform.uv.x, deform.uv.y, 0, 0));
            deformationCompute.SetFloat("DeformRadius", deform.radius);
            deformationCompute.SetFloat("DarknessStrength", trailDarkness);
            deformationCompute.SetInt("Resolution", resolution);

            int threadGroups = Mathf.CeilToInt(resolution / 8.0f);
            deformationCompute.Dispatch(kernelMarkDarkness, threadGroups, threadGroups, 1);

            Graphics.Blit(tempDarknessMap, darknessMap);
        }
    }

    void FadeDarkness()
    {
        deformationCompute.SetTexture(kernelFadeDarkness, "DarknessMap", darknessMap);
        deformationCompute.SetTexture(kernelFadeDarkness, "Result", tempDarknessMap);
        deformationCompute.SetFloat("DarknessFadeSpeed", trailFadeSpeed * Time.deltaTime);
        deformationCompute.SetInt("Resolution", resolution);

        int threadGroups = Mathf.CeilToInt(resolution / 8.0f);
        deformationCompute.Dispatch(kernelFadeDarkness, threadGroups, threadGroups, 1);

        Graphics.Blit(tempDarknessMap, darknessMap);
    }

    public void ResetSand()
    {
        RenderTexture.active = darknessMap;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;
        Graphics.Blit(darknessMap, tempDarknessMap);
    }

    void OnDestroy()
    {
        if (darknessMap != null) darknessMap.Release();
        if (tempDarknessMap != null) tempDarknessMap.Release();
    }

    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(meshCenter, meshSize);
        }
    }
}