
using System.Diagnostics;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
[DefaultExecutionOrder(100)]
public class GroundGridVisual : MonoBehaviour
{
    [Header("Grid Style")]
    [SerializeField, Min(0.5f)]
    private float tileWorldSize = 4f;

    [SerializeField, Range(0.005f, 0.05f)]
    private float lineWidth = 0.018f;

    [SerializeField]
    private Color tileColor =
        new Color(0.84f, 0.89f, 0.90f, 1f);

    [SerializeField]
    private Color lineColor =
        new Color(0.71f, 0.79f, 0.82f, 1f);

    private const int TextureSize = 128;
    private const string BaseMap = "_BaseMap";

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    private Material originalMaterial;
    private Material generatedMaterial;
    private Texture2D generatedTexture;

    private Vector3 lastPosition;
    private Vector3 lastScale;
    private bool mappingInitialized;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();

        Shader shader =
            Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            UnityEngine.Debug.LogError("URP Lit shader not found.");
            enabled = false;
            return;
        }

        originalMaterial = meshRenderer.sharedMaterial;

        generatedMaterial = new Material(shader);
        generatedMaterial.name = "Runtime_GroundGrid";
        generatedMaterial.SetColor("_BaseColor", Color.white);
        generatedMaterial.SetFloat("_Metallic", 0f);
        generatedMaterial.SetFloat("_Smoothness", 0.1f);

        GenerateTexture();

        generatedMaterial.SetTexture(
            BaseMap, generatedTexture);

        meshRenderer.sharedMaterial = generatedMaterial;

        UpdateMapping();
    }

    private void GenerateTexture()
    {
        generatedTexture = new Texture2D(
            TextureSize,
            TextureSize,
            TextureFormat.RGBA32,
            true
        );

        generatedTexture.name = "Runtime_GridTexture";
        generatedTexture.wrapMode = TextureWrapMode.Repeat;
        generatedTexture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[TextureSize * TextureSize];

        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                float u = (x + 0.5f) / TextureSize;
                float v = (y + 0.5f) / TextureSize;

                float edgeX = Mathf.Min(u, 1f - u);
                float edgeY = Mathf.Min(v, 1f - v);
                float distance = Mathf.Min(edgeX, edgeY);

                float blend = Mathf.InverseLerp(
                    lineWidth * 0.5f,
                    lineWidth * 1.8f,
                    distance
                );

                blend = Mathf.SmoothStep(0f, 1f, blend);

                pixels[y * TextureSize + x] =
                    Color.Lerp(lineColor, tileColor, blend);
            }
        }

        generatedTexture.SetPixels(pixels);
        generatedTexture.Apply(true, false);
    }

    private void LateUpdate()
    {
        if (!mappingInitialized ||
            transform.position != lastPosition ||
            transform.lossyScale != lastScale)
        {
            UpdateMapping();
        }
    }

    private void UpdateMapping()
    {
        if (generatedMaterial == null ||
            meshFilter.sharedMesh == null)
            return;

        Vector3 meshSize = meshFilter.sharedMesh.bounds.size;
        Vector3 scale = transform.lossyScale;

        float worldWidth =
            Mathf.Abs(meshSize.x * scale.x);

        float worldDepth =
            Mathf.Abs(meshSize.z * scale.z);

        float tileSize = Mathf.Max(0.5f, tileWorldSize);

        generatedMaterial.SetTextureScale(
            BaseMap,
            new Vector2(
                worldWidth / tileSize,
                worldDepth / tileSize
            )
        );

        // Anchor the visual pattern in world space.
        // InfiniteGround can move the Plane without
        // moving the visible grid pattern.
        Vector2 offset = new Vector2(
            Mathf.Repeat(
                (transform.position.x - worldWidth * 0.5f)
                / tileSize, 1f),
            Mathf.Repeat(
                (transform.position.z - worldDepth * 0.5f)
                / tileSize, 1f)
        );

        generatedMaterial.SetTextureOffset(BaseMap, offset);

        lastPosition = transform.position;
        lastScale = transform.lossyScale;
        mappingInitialized = true;
    }

    private void OnDestroy()
    {
        if (meshRenderer != null &&
            meshRenderer.sharedMaterial == generatedMaterial)
        {
            meshRenderer.sharedMaterial = originalMaterial;
        }

        if (generatedTexture != null)
            Destroy(generatedTexture);

        if (generatedMaterial != null)
            Destroy(generatedMaterial);
    }
}
