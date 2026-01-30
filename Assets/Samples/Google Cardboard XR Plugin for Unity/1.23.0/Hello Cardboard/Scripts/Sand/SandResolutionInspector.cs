using UnityEngine;

public class SandDeformationInspector : MonoBehaviour
{
    [Header("Visualization")]
    public bool showUVGrid = true;
    public bool showDeformationZone = true;
    public Color gridColor = Color.yellow;

    private SandDeformation sandDeform;
    private SandDeformationDebug sandDeformDebug;
    private Material sandMaterial;

    void Start()
    {
        sandDeform = GetComponent<SandDeformation>();
        sandDeformDebug = GetComponent<SandDeformationDebug>();
        sandMaterial = GetComponent<Renderer>().material;

        PrintResolutionInfo();
    }

    [ContextMenu("Print Resolution Info")]
    public void PrintResolutionInfo()
    {
        Debug.Log("=== SAND DEFORMATION RESOLUTION INFO ===");

        // Get resolution from component
        int resolution = 0;
        if (sandDeform != null)
        {
            resolution = sandDeform.resolution;
            Debug.Log($"SandDeformation Resolution: {resolution}x{resolution}");
        }
        else if (sandDeformDebug != null)
        {
            resolution = sandDeformDebug.resolution;
            Debug.Log($"SandDeformationDebug Resolution: {resolution}x{resolution}");
        }
        else
        {
            Debug.LogError("No SandDeformation component found!");
            return;
        }

        // Get heightmap from material
        Texture heightTex = sandMaterial.GetTexture("_HeightMap");
        if (heightTex != null)
        {
            Debug.Log($"HeightMap Texture Size: {heightTex.width}x{heightTex.height}");

            if (heightTex.width != resolution || heightTex.height != resolution)
            {
                Debug.LogWarning($"? MISMATCH! Component resolution ({resolution}) != Texture size ({heightTex.width})");
            }
        }
        else
        {
            Debug.LogWarning("HeightMap texture not assigned to material yet");
        }

        // Get sand plane size
        Vector3 scale = transform.localScale;
        Debug.Log($"Sand Plane Scale: {scale}");
        Debug.Log($"Sand Plane World Size: {scale.x} x {scale.z} units");

        // Calculate texel density
        float texelsPerUnit = resolution / Mathf.Max(scale.x, scale.z);
        Debug.Log($"Texel Density: {texelsPerUnit:F2} pixels per world unit");

        if (texelsPerUnit < 10)
        {
            Debug.LogWarning($"? LOW resolution! Only {texelsPerUnit:F2} pixels per unit.");
            Debug.LogWarning("Consider increasing resolution or decreasing plane scale.");
        }

        // Check UV mapping
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            Vector2[] uvs = mf.sharedMesh.uv;
            if (uvs.Length > 0)
            {
                Vector2 minUV = uvs[0];
                Vector2 maxUV = uvs[0];

                foreach (Vector2 uv in uvs)
                {
                    minUV.x = Mathf.Min(minUV.x, uv.x);
                    minUV.y = Mathf.Min(minUV.y, uv.y);
                    maxUV.x = Mathf.Max(maxUV.x, uv.x);
                    maxUV.y = Mathf.Max(maxUV.y, uv.y);
                }

                Debug.Log($"Mesh UV Range: ({minUV.x:F3}, {minUV.y:F3}) to ({maxUV.x:F3}, {maxUV.y:F3})");

                if (minUV.x < -0.01f || minUV.y < -0.01f || maxUV.x > 1.01f || maxUV.y > 1.01f)
                {
                    Debug.LogWarning("? UVs are outside 0-1 range! This might cause issues.");
                }
                else
                {
                    Debug.Log("? UVs are in correct 0-1 range");
                }
            }
        }

        Debug.Log("======================================");
    }

    [ContextMenu("Test Deformation at World Origin")]
    public void TestAtOrigin()
    {
        TestDeformationAt(Vector3.zero);
    }

    [ContextMenu("Test Deformation at Sand Center")]
    public void TestAtCenter()
    {
        TestDeformationAt(transform.position);
    }

    [ContextMenu("Test Deformation at Sand Corner")]
    public void TestAtCorner()
    {
        Vector3 corner = transform.position + new Vector3(
            transform.localScale.x * 0.4f,
            0,
            transform.localScale.z * 0.4f
        );
        TestDeformationAt(corner);
    }

    void TestDeformationAt(Vector3 worldPos)
    {
        Debug.Log($"Testing deformation at world position: {worldPos}");

        // Convert to UV
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        Vector2 uv = new Vector2(
            (localPos.x / transform.localScale.x) + 0.5f,
            (localPos.z / transform.localScale.z) + 0.5f
        );

        Debug.Log($"Local position: {localPos}");
        Debug.Log($"UV coordinates: {uv}");

        if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
        {
            Debug.LogWarning("? UV is OUTSIDE 0-1 range! Deformation will fail.");
        }
        else
        {
            Debug.Log("? UV is valid, applying deformation...");

            if (sandDeform != null)
            {
                sandDeform.DeformSand(worldPos, 0.2f, 0.3f);
            }
            else if (sandDeformDebug != null)
            {
                sandDeformDebug.DeformSand(worldPos, 0.2f, 0.3f);
            }

            Debug.Log("Deformation applied! Check if it appears at the correct location.");
        }
    }

    [ContextMenu("Print Transform Scale Issue")]
    public void CheckScaleIssue()
    {
        Debug.Log("=== CHECKING FOR SCALE ISSUES ===");

        Vector3 scale = transform.localScale;
        Debug.Log($"Sand Plane Scale: {scale}");

        // Check if scale is very large
        if (scale.x > 50 || scale.z > 50)
        {
            Debug.LogWarning("? VERY LARGE SCALE DETECTED!");
            Debug.LogWarning($"Scale X: {scale.x}, Z: {scale.z}");
            Debug.LogWarning("This might cause UV calculation issues.");
            Debug.LogWarning("Recommended scale: 1-20 for each axis");
            Debug.LogWarning("");
            Debug.LogWarning("SOLUTION:");
            Debug.LogWarning("1. Scale down the plane in Unity");
            Debug.LogWarning("2. OR adjust the mesh size in your 3D software");
            Debug.LogWarning("3. OR modify the UV calculation in RakeDeformer.cs");
        }
        else if (scale.x < 0.1f || scale.z < 0.1f)
        {
            Debug.LogWarning("? VERY SMALL SCALE DETECTED!");
            Debug.LogWarning("Scale might be too small for accurate deformation.");
        }
        else
        {
            Debug.Log("? Scale looks reasonable");
        }

        // Check rotation
        Vector3 euler = transform.eulerAngles;
        if (Mathf.Abs(euler.x) > 1f || Mathf.Abs(euler.z) > 1f)
        {
            Debug.LogWarning("? Sand plane is ROTATED!");
            Debug.LogWarning($"Rotation: {euler}");
            Debug.LogWarning("For best results, keep rotation at (0, Y, 0)");
        }
        else
        {
            Debug.Log("? Rotation looks good");
        }

        Debug.Log("================================");
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (!showUVGrid && !showDeformationZone) return;

        // Draw UV grid on the sand plane
        if (showUVGrid)
        {
            Gizmos.color = gridColor;

            Vector3 scale = transform.localScale;
            Vector3 center = transform.position;

            // Draw grid lines
            int gridLines = 10;
            for (int i = 0; i <= gridLines; i++)
            {
                float t = i / (float)gridLines;

                // Vertical lines
                Vector3 start = center + new Vector3(
                    (t - 0.5f) * scale.x,
                    0.01f,
                    -0.5f * scale.z
                );
                Vector3 end = center + new Vector3(
                    (t - 0.5f) * scale.x,
                    0.01f,
                    0.5f * scale.z
                );
                Gizmos.DrawLine(start, end);

                // Horizontal lines
                start = center + new Vector3(
                    -0.5f * scale.x,
                    0.01f,
                    (t - 0.5f) * scale.z
                );
                end = center + new Vector3(
                    0.5f * scale.x,
                    0.01f,
                    (t - 0.5f) * scale.z
                );
                Gizmos.DrawLine(start, end);
            }

            // Draw corners with labels
            Vector3[] corners = new Vector3[]
            {
                center + new Vector3(-scale.x/2, 0.01f, -scale.z/2),
                center + new Vector3(scale.x/2, 0.01f, -scale.z/2),
                center + new Vector3(scale.x/2, 0.01f, scale.z/2),
                center + new Vector3(-scale.x/2, 0.01f, scale.z/2),
            };

            string[] labels = new string[] { "UV(0,0)", "UV(1,0)", "UV(1,1)", "UV(0,1)" };

            for (int i = 0; i < corners.Length; i++)
            {
                Gizmos.DrawWireSphere(corners[i], 0.1f);
#if UNITY_EDITOR
                UnityEditor.Handles.Label(corners[i] + Vector3.up * 0.2f, labels[i]);
#endif
            }
        }

        // Draw deformation zone markers
        if (showDeformationZone)
        {
            RakeDeformer[] rakes = FindObjectsOfType<RakeDeformer>();
            foreach (var rake in rakes)
            {
                if (rake.rakeForks != null)
                {
                    Gizmos.color = Color.cyan;
                    foreach (var fork in rake.rakeForks)
                    {
                        if (fork != null)
                        {
                            // Draw line to where deformation would happen
                            RaycastHit hit;
                            if (Physics.Raycast(fork.position, Vector3.down, out hit, 10f))
                            {
                                Gizmos.DrawLine(fork.position, hit.point);
                                Gizmos.DrawWireSphere(hit.point, rake.deformRadius);
                            }
                        }
                    }
                }
            }
        }
    }
}