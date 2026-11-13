
using System.Diagnostics;
using UnityEngine;

public class SlowImpactVFX : MonoBehaviour
{
    [SerializeField] private Transform slowDisk;
    [SerializeField] private MeshRenderer diskRenderer;

    private static SlowImpactVFX prefab;

    private const float Lifetime = 0.3f;
    private float elapsed;

    private MaterialPropertyBlock block;
    private Color originalColor = Color.white;

    public static void Spawn(Vector3 center, float radius)
    {
        if (radius <= 0f)
            return;

        if (prefab == null)
        {
            prefab = Resources.Load<SlowImpactVFX>(
                "VFX_SlowImpact");
        }

        if (prefab == null)
        {
            UnityEngine.Debug.LogWarning(
                "VFX_SlowImpact prefab not found.");
            return;
        }

        // Place the visual effect on the ground.
        Vector3 position = new Vector3(
            center.x, 0.03f, center.z);

        SlowImpactVFX effect = Instantiate(
            prefab, position, Quaternion.identity);

        // Cylinder default radius is 0.5.
        if (effect.slowDisk != null)
        {
            Vector3 scale = effect.slowDisk.localScale;
            scale.x = radius * 2f;
            scale.z = radius * 2f;
            effect.slowDisk.localScale = scale;
        }
    }

    private void Awake()
    {
        block = new MaterialPropertyBlock();

        if (diskRenderer != null &&
            diskRenderer.sharedMaterial != null &&
            diskRenderer.sharedMaterial.HasProperty("_BaseColor"))
        {
            originalColor =
                diskRenderer.sharedMaterial.GetColor("_BaseColor");
        }
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        if (elapsed >= Lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // One-time flash that fades away.
        if (diskRenderer != null)
        {
            Color color = originalColor;
            color.a *= 1f - elapsed / Lifetime;

            diskRenderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", color);
            diskRenderer.SetPropertyBlock(block);
        }
    }
}
