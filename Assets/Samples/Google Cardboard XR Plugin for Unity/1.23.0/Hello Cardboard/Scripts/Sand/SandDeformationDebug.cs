using UnityEngine;
using System.Collections.Generic;

public class SandDeformationDebug : MonoBehaviour
{
    [Header("Sand Settings")]
    public int resolution = 512;
    public float deformationStrength = 0.1f;
    public float smoothingSpeed = 2.0f;

    [Header("Compute Shader")]
    public ComputeShader deformationCompute;

    [Header("Performance")]
    public int smoothingInterval = 5;

    [Header("Debug")]
    public bool enableDebugLogs = true;
    public bool visualizeHeightMap = false;
    public Renderer debugQuad;

    private RenderTexture heightMap;
    private RenderTexture tempHeightMap;
    private Material sandMaterial;

    private int kernelDeform;
    private int kernelSmooth;
    private int frameCounter = 0;
    private int deformationCount = 0;

    private struct DeformationPoint
    {
        public Vector2 uv;
        public float radius;
        public float strength;
    }

    private List<DeformationPoint> pendingDeformations = new List<DeformationPoint>();

    void Start()
    {
        Debug.Log("=== SandDeformation Debug Start ===");
        InitializeHeightMap();
        SetupSandMaterial();
        InitializeComputeShader();
        Debug.Log("=== SandDeformation Initialized ===");
    }

    void InitializeHeightMap()
    {
        Debug.Log($"Creating heightmap with resolution: {resolution}");

        heightMap = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.RFloat);
        heightMap.enableRandomWrite = true;
        heightMap.filterMode = FilterMode.Bilinear;
        heightMap.wrapMode = TextureWrapMode.Clamp;
        heightMap.Create();

        tempHeightMap = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.RFloat);
        tempHeightMap.enableRandomWrite = true;
        tempHeightMap.filterMode = FilterMode.Bilinear;
        tempHeightMap.wrapMode = TextureWrapMode.Clamp;
        tempHeightMap.Create();

        // Initialize with 0.5
        RenderTexture.active = heightMap;
        GL.Clear(true, true, new Color(0.5f, 0.5f, 0.5f, 1f));
        RenderTexture.active = null;

        Graphics.Blit(heightMap, tempHeightMap);

        Debug.Log($"Heightmap created successfully. Format: {heightMap.format}, Size: {heightMap.width}x{heightMap.height}");

        // Visualize on debug quad if assigned
        if (debugQuad != null)
        {
            debugQuad.material.mainTexture = heightMap;
            Debug.Log("Debug quad assigned - you should see the heightmap there");
        }
    }

    void SetupSandMaterial()
    {
        sandMaterial = GetComponent<Renderer>().material;

        if (sandMaterial == null)
        {
            Debug.LogError("NO MATERIAL FOUND on sand plane!");
            return;
        }

        Debug.Log($"Sand material: {sandMaterial.name}, Shader: {sandMaterial.shader.name}");

        // Check if shader has _HeightMap property
        if (!sandMaterial.HasProperty("_HeightMap"))
        {
            Debug.LogError("SHADER DOES NOT HAVE _HeightMap PROPERTY! Check your shader!");
            return;
        }

        sandMaterial.SetTexture("_HeightMap", heightMap);
        Debug.Log("HeightMap texture assigned to material");

        // Check displacement strength
        if (sandMaterial.HasProperty("_DisplacementStrength"))
        {
            float dispStrength = sandMaterial.GetFloat("_DisplacementStrength");
            Debug.Log($"Current Displacement Strength in shader: {dispStrength}");
            if (dispStrength < 0.01f)
            {
                Debug.LogWarning("Displacement Strength is very low! Increase it in the material.");
            }
        }
    }

    void InitializeComputeShader()
    {
        if (deformationCompute == null)
        {
            Debug.LogError("COMPUTE SHADER NOT ASSIGNED!");
            return;
        }

        Debug.Log($"Compute Shader: {deformationCompute.name}");

        // Check if kernels exist
        if (!HasKernel(deformationCompute, "DeformSand"))
        {
            Debug.LogError("Compute shader missing 'DeformSand' kernel!");
            return;
        }
        if (!HasKernel(deformationCompute, "SmoothSand"))
        {
            Debug.LogError("Compute shader missing 'SmoothSand' kernel!");
            return;
        }

        kernelDeform = deformationCompute.FindKernel("DeformSand");
        kernelSmooth = deformationCompute.FindKernel("SmoothSand");

        Debug.Log($"Kernels found - DeformSand: {kernelDeform}, SmoothSand: {kernelSmooth}");
    }

    bool HasKernel(ComputeShader shader, string kernelName)
    {
        try
        {
            shader.FindKernel(kernelName);
            return true;
        }
        catch
        {
            return false;
        }
    }

    void LateUpdate()
    {
        if (deformationCompute == null)
            return;

        if (pendingDeformations.Count > 0)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"Applying {pendingDeformations.Count} deformations");
            }

            ApplyDeformations();
            pendingDeformations.Clear();
        }

        frameCounter++;
        if (frameCounter >= smoothingInterval)
        {
            ApplySmoothing();
            frameCounter = 0;
        }
    }

    public void DeformSand(Vector3 worldPosition, float radius, float strength)
    {
        // Convert world position to UV coordinates
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);
        Vector2 uv = new Vector2(
            (localPos.x / transform.localScale.x) + 0.5f,
            (localPos.z / transform.localScale.z) + 0.5f
        );

        // Check bounds
        if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"Deformation out of bounds! UV: {uv}, Local: {localPos}");
            }
            return;
        }

        float uvRadius = radius / Mathf.Max(transform.localScale.x, transform.localScale.z);

        deformationCount++;

        if (enableDebugLogs && deformationCount % 10 == 0)
        {
            Debug.Log($"Deformation #{deformationCount} - UV: {uv}, Radius: {uvRadius}, Strength: {strength}");
        }

        pendingDeformations.Add(new DeformationPoint
        {
            uv = uv,
            radius = uvRadius,
            strength = strength
        });
    }

    void ApplyDeformations()
    {
        if (!heightMap.IsCreated())
        {
            Debug.LogError("HeightMap is not created!");
            return;
        }

        deformationCompute.SetTexture(kernelDeform, "HeightMap", heightMap);
        deformationCompute.SetTexture(kernelDeform, "Result", tempHeightMap);

        int appliedCount = 0;
        foreach (var deform in pendingDeformations)
        {
            deformationCompute.SetVector("DeformCenter", new Vector4(deform.uv.x, deform.uv.y, 0, 0));
            deformationCompute.SetFloat("DeformRadius", deform.radius);
            deformationCompute.SetFloat("DeformStrength", deform.strength);
            deformationCompute.SetInt("Resolution", resolution);

            int threadGroups = Mathf.CeilToInt(resolution / 8.0f);

            try
            {
                deformationCompute.Dispatch(kernelDeform, threadGroups, threadGroups, 1);
                appliedCount++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Compute shader dispatch failed: {e.Message}");
            }

            Graphics.Blit(tempHeightMap, heightMap);
        }

        if (enableDebugLogs && appliedCount > 0)
        {
            Debug.Log($"Successfully applied {appliedCount} deformations via compute shader");
        }
    }

    void ApplySmoothing()
    {
        if (!heightMap.IsCreated())
            return;

        deformationCompute.SetTexture(kernelSmooth, "HeightMap", heightMap);
        deformationCompute.SetTexture(kernelSmooth, "Result", tempHeightMap);
        deformationCompute.SetFloat("SmoothingStrength", smoothingSpeed * Time.deltaTime);
        deformationCompute.SetInt("Resolution", resolution);

        int threadGroups = Mathf.CeilToInt(resolution / 8.0f);

        try
        {
            deformationCompute.Dispatch(kernelSmooth, threadGroups, threadGroups, 1);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Smoothing dispatch failed: {e.Message}");
        }

        Graphics.Blit(tempHeightMap, heightMap);
    }

    public void ResetSand()
    {
        RenderTexture.active = heightMap;
        GL.Clear(true, true, new Color(0.5f, 0.5f, 0.5f, 1f));
        RenderTexture.active = null;

        Graphics.Blit(heightMap, tempHeightMap);

        Debug.Log("Sand reset to flat");
    }

    // Manual test - call this from inspector or button
    [ContextMenu("Test Deformation at Center")]
    public void TestDeformationAtCenter()
    {
        Vector3 centerWorld = transform.position;
        Debug.Log($"Testing deformation at center: {centerWorld}");
        DeformSand(centerWorld, 0.2f, 0.5f);
    }

    [ContextMenu("Print Current State")]
    public void PrintCurrentState()
    {
        Debug.Log("=== CURRENT STATE ===");
        Debug.Log($"HeightMap created: {heightMap != null && heightMap.IsCreated()}");
        Debug.Log($"Material assigned: {sandMaterial != null}");
        Debug.Log($"Compute shader assigned: {deformationCompute != null}");
        Debug.Log($"Total deformations applied: {deformationCount}");
        Debug.Log($"Pending deformations: {pendingDeformations.Count}");

        if (sandMaterial != null)
        {
            Texture assignedTex = sandMaterial.GetTexture("_HeightMap");
            Debug.Log($"Material _HeightMap texture: {(assignedTex != null ? assignedTex.name : "NULL")}");
        }
    }

    void OnDestroy()
    {
        if (heightMap != null) heightMap.Release();
        if (tempHeightMap != null) tempHeightMap.Release();
    }

    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && heightMap != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}