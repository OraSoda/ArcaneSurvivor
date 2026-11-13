
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MagicImpactVFX : MonoBehaviour
{
    private const int Segments = 48;

    private static Material sharedMaterial;

    private LineRenderer line;
    private float maxRadius;
    private float duration;
    private float elapsed;
    private Color baseColor;

    public static void Spawn(
        Vector3 position,
        float radius,
        Color color,
        float effectDuration = 0.35f)
    {
        if (radius <= 0f)
            return;

        GameObject obj = new GameObject("MagicImpactVFX");

        // A horizontal magic ring slightly above the ground.
        obj.transform.position =
            new Vector3(position.x, 0.12f, position.z);

        MagicImpactVFX effect =
            obj.AddComponent<MagicImpactVFX>();

        effect.Initialize(
            radius,
            color,
            effectDuration
        );
    }

    private void Initialize(
        float radius,
        Color color,
        float effectDuration)
    {
        maxRadius = radius;
        baseColor = color;
        duration = Mathf.Max(0.05f, effectDuration);

        line = GetComponent<LineRenderer>();

        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = Segments;
        line.widthMultiplier = 0.07f;
        line.numCornerVertices = 4;
        line.numCapVertices = 4;

        if (sharedMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");

            if (shader == null)
                shader = Shader.Find(
                    "Universal Render Pipeline/Unlit");

            if (shader != null)
                sharedMaterial = new Material(shader);
        }

        if (sharedMaterial != null)
            line.sharedMaterial = sharedMaterial;

        DrawRing(0.15f);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        float t = Mathf.Clamp01(elapsed / duration);

        float eased = 1f - (1f - t) * (1f - t);
        float radius = Mathf.Lerp(
            0.15f, maxRadius, eased);

        DrawRing(radius);

        Color fadedColor = baseColor;
        fadedColor.a = 1f - t;

        line.startColor = fadedColor;
        line.endColor = fadedColor;

        line.widthMultiplier =
            Mathf.Lerp(0.085f, 0.015f, t);

        if (t >= 1f)
            Destroy(gameObject);
    }

    private void DrawRing(float radius)
    {
        for (int i = 0; i < Segments; i++)
        {
            float angle =
                i * Mathf.PI * 2f / Segments;

            line.SetPosition(
                i,
                new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius
                )
            );
        }
    }
}
