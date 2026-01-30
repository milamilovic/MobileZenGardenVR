using UnityEngine;

[ExecuteInEditMode]
public class MatchProBuilderPlane : MonoBehaviour
{
    [Header("Reference")]
    public GameObject proBuilderPlane;

    [Header("Mesh Settings")]
    public int subdivisionsX = 100;
    public int subdivisionsY = 100;
    public string meshName = "SandPlaneMesh_Fixed";

    [Header("Output")]
    public Material sandMaterial;

    [ContextMenu("Generate Matching Plane")]
    public void GenerateMatchingPlane()
    {
        if (proBuilderPlane == null)
        {
            Debug.LogError("Assign the ProBuilder plane reference!");
            return;
        }

        // Get the world-space bounds of the ProBuilder plane
        Renderer proRenderer = proBuilderPlane.GetComponent<Renderer>();
        if (proRenderer == null)
        {
            Debug.LogError("ProBuilder plane has no Renderer!");
            return;
        }

        Bounds bounds = proRenderer.bounds;
        Vector3 worldSize = bounds.size;
        Vector3 worldCenter = bounds.center;

        Debug.Log($"ProBuilder plane world size: {worldSize}");
        Debug.Log($"ProBuilder plane world center: {worldCenter}");

        // create mesh with the exact world size
        Mesh mesh = GenerateHighPolyMesh(worldSize.x, worldSize.z);

        // create GameObject at the exact same position
        GameObject planeObj = new GameObject("SandPlane_Fixed");
        planeObj.transform.position = worldCenter;
        planeObj.transform.rotation = proBuilderPlane.transform.rotation;
        // set scale to (1, 1, 1) since mesh is already the correct size
        planeObj.transform.localScale = Vector3.one;

        // Add components
        MeshFilter mf = planeObj.AddComponent<MeshFilter>();
        MeshRenderer mr = planeObj.AddComponent<MeshRenderer>();

        mf.mesh = mesh;

        if (sandMaterial != null)
        {
            mr.material = sandMaterial;
        }
        else
        {
            Renderer oldRenderer = proBuilderPlane.GetComponent<Renderer>();
            if (oldRenderer != null && oldRenderer.sharedMaterial != null)
            {
                mr.material = oldRenderer.sharedMaterial;
                Debug.Log("Copied material from ProBuilder plane");
            }
        }

        // Add collider
        MeshCollider mc = planeObj.AddComponent<MeshCollider>();
        mc.sharedMesh = mesh;

        // Copy layer
        planeObj.layer = proBuilderPlane.layer;

        Debug.Log($"Created plane with {mesh.vertexCount} vertices");
        Debug.Log($"World size matches: {worldSize.x:F2} x {worldSize.z:F2}");
        Debug.Log($"Position: {worldCenter}");

        // Save mesh as asset
#if UNITY_EDITOR
        string path = "Assets/" + meshName + ".asset";
        UnityEditor.AssetDatabase.CreateAsset(mesh, path);
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log($"Mesh saved to {path}");
#endif
    }

    Mesh GenerateHighPolyMesh(float width, float length)
    {
        Mesh mesh = new Mesh();
        mesh.name = meshName;

        int vertCountX = subdivisionsX + 1;
        int vertCountY = subdivisionsY + 1;
        int totalVerts = vertCountX * vertCountY;

        Vector3[] vertices = new Vector3[totalVerts];
        Vector2[] uvs = new Vector2[totalVerts];
        Vector3[] normals = new Vector3[totalVerts];

        // Generate vertices
        for (int y = 0; y < vertCountY; y++)
        {
            for (int x = 0; x < vertCountX; x++)
            {
                int index = y * vertCountX + x;

                // Vertex positions in world size
                float xPos = (x / (float)subdivisionsX - 0.5f) * width;
                float zPos = (y / (float)subdivisionsY - 0.5f) * length;

                vertices[index] = new Vector3(xPos, 0, zPos);

                // UVs in 0-1 range
                uvs[index] = new Vector2(x / (float)subdivisionsX, y / (float)subdivisionsY);

                normals[index] = Vector3.up;
            }
        }

        // Generate triangles
        int[] triangles = new int[subdivisionsX * subdivisionsY * 6];
        int triIndex = 0;

        for (int y = 0; y < subdivisionsY; y++)
        {
            for (int x = 0; x < subdivisionsX; x++)
            {
                int vertIndex = y * vertCountX + x;

                // First triangle
                triangles[triIndex++] = vertIndex;
                triangles[triIndex++] = vertIndex + vertCountX;
                triangles[triIndex++] = vertIndex + 1;

                // Second triangle
                triangles[triIndex++] = vertIndex + 1;
                triangles[triIndex++] = vertIndex + vertCountX;
                triangles[triIndex++] = vertIndex + vertCountX + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.normals = normals;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        // Verify UVs
        Vector2 minUV = uvs[0];
        Vector2 maxUV = uvs[0];
        foreach (Vector2 uv in uvs)
        {
            minUV.x = Mathf.Min(minUV.x, uv.x);
            minUV.y = Mathf.Min(minUV.y, uv.y);
            maxUV.x = Mathf.Max(maxUV.x, uv.x);
            maxUV.y = Mathf.Max(maxUV.y, uv.y);
        }
        Debug.Log($"UV Range: ({minUV.x:F3}, {minUV.y:F3}) to ({maxUV.x:F3}, {maxUV.y:F3})");

        return mesh;
    }

    [ContextMenu("Copy Components to New Plane")]
    public void CopyComponentsToNewPlane()
    {
        GameObject newPlane = GameObject.Find("SandPlane_Fixed");
        if (newPlane == null)
        {
            Debug.LogError("Can't find SandPlane_Fixed! Generate it first.");
            return;
        }

        if (proBuilderPlane == null)
        {
            Debug.LogError("ProBuilder plane not assigned!");
            return;
        }

        // Copy SandDeformation component
        var oldSandDeform = proBuilderPlane.GetComponent<SandDeformation>();
        var oldSandDeformDebug = proBuilderPlane.GetComponent<SandDeformationDebug>();

        if (oldSandDeform != null)
        {
            var newSandDeform = newPlane.AddComponent<SandDeformation>();
            newSandDeform.resolution = oldSandDeform.resolution;
            newSandDeform.deformationCompute = oldSandDeform.deformationCompute;
            Debug.Log("Copied SandDeformation component");
        }
        else if (oldSandDeformDebug != null)
        {
            var newSandDeform = newPlane.AddComponent<SandDeformationDebug>();
            newSandDeform.resolution = oldSandDeformDebug.resolution;
            newSandDeform.deformationStrength = oldSandDeformDebug.deformationStrength;
            newSandDeform.smoothingSpeed = oldSandDeformDebug.smoothingSpeed;
            newSandDeform.smoothingInterval = oldSandDeformDebug.smoothingInterval;
            newSandDeform.deformationCompute = oldSandDeformDebug.deformationCompute;
            newSandDeform.enableDebugLogs = oldSandDeformDebug.enableDebugLogs;
            Debug.Log("Copied SandDeformationDebug component");
        }

        // Update RakeDeformer references
        RakeDeformer[] rakes = FindObjectsOfType<RakeDeformer>();
        foreach (var rake in rakes)
        {
            if (rake.sandDeformation != null && rake.sandDeformation.gameObject == proBuilderPlane)
            {
                var newSandDeform = newPlane.GetComponent<SandDeformation>();
                if (newSandDeform != null)
                {
                    rake.sandDeformation = newSandDeform;
                    Debug.Log($"Updated RakeDeformer on {rake.gameObject.name}");
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (proBuilderPlane != null)
        {
            Renderer rend = proBuilderPlane.GetComponent<Renderer>();
            if (rend != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(rend.bounds.center, rend.bounds.size);

#if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    rend.bounds.center + Vector3.up * 0.5f,
                    $"Will generate plane:\n{rend.bounds.size.x:F2} x {rend.bounds.size.z:F2}\n" +
                    $"{subdivisionsX}x{subdivisionsY} subdivisions\n" +
                    $"{(subdivisionsX + 1) * (subdivisionsY + 1)} vertices"
                );
#endif
            }
        }
    }
}