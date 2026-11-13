
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MagicChainVFX : MonoBehaviour
{
    private static Material sharedMaterial;

    private LineRenderer outerLine;
    private LineRenderer coreLine;

    private float duration;
    private float elapsed;

    private readonly Color outerColor =
        new Color(0.35f, 0.25f, 0.95f, 0.95f);

    private readonly Color coreColor =
        new Color(0.85f, 0.95f, 1f, 1f);

    public static void Spawn(
        Vector3 start,
        Vector3 end,
        float lifetime = 0.22f)
    {
        GameObject obj = new GameObject("MagicChainVFX");

        MagicChainVFX vfx =
            obj.AddComponent<MagicChainVFX>();

        vfx.Initialize(start, end, lifetime);
    }

    private void Initialize(
        Vector3 start,
        Vector3 end,
        float lifetime)
    {
        duration = Mathf.Max(0.05f, lifetime);

        if (sharedMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");

            if (shader == null)
                shader = Shader.Find(
                    "Universal Render Pipeline/Unlit");

            if (shader != null)
                sharedMaterial = new Material(shader);
        }

        Vector3 elevatedStart = start + Vector3.up * 0.5f;
        Vector3 elevatedEnd = end + Vector3.up * 0.5f;

        // Generate one jagged path for both layers.
        const int pointCount = 9;
        Vector3[] points = new Vector3[pointCount];

        Vector3 direction = elevatedEnd - elevatedStart;
        float distance = direction.magnitude;

        Vector3 side = Vector3.Cross(
            direction.normalized,
            Vector3.up);

        if (side.sqrMagnitude < 0.001f)
            side = Vector3.right;
        else
            side.Normalize();

        float zigzag = Mathf.Min(0.25f, distance * 0.09f);

        for (int i = 0; i < pointCount; i++)
        {
            float t = i / (float)(pointCount - 1);

            Vector3 position = Vector3.Lerp(
                elevatedStart,
                elevatedEnd,
                t);

            if (i > 0 && i < pointCount - 1)
            {
                float offset = (i % 2 == 0 ? 1f : -1f)
                    * zigzag;

                position += side * offset;
                position += Vector3.up
                    * Mathf.Sin(i * 2.1f) * 0.07f;
            }

            points[i] = position;
        }

        // Outer blue-purple lightning.
        outerLine = GetComponent<LineRenderer>();
        SetupLine(outerLine, points, 0.18f);

        // Inner bright core.
        GameObject coreObject = new GameObject("ChainCore");
        coreObject.transform.SetParent(transform, false);

        coreLine = coreObject.AddComponent<LineRenderer>();
        SetupLine(coreLine, points, 0.065f);

        UpdateVisual(0f);
    }

    private void SetupLine(
        LineRenderer line,
        Vector3[] points,
        float width)
    {
        line.useWorldSpace = true;
        line.positionCount = points.Length;
        line.SetPositions(points);

        line.widthMultiplier = width;
        line.alignment = LineAlignment.View;

        line.numCornerVertices = 3;
        line.numCapVertices = 3;

        line.loop = false;

        line.startColor = Color.white;
        line.endColor = Color.white;

        if (sharedMaterial != null)
            line.sharedMaterial = sharedMaterial;

        line.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;

        line.receiveShadows = false;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        float t = Mathf.Clamp01(elapsed / duration);

        UpdateVisual(t);

        if (t >= 1f)
            Destroy(gameObject);
    }

    private void UpdateVisual(float t)
    {
        float fade = 1f - t;

        if (outerLine != null)
        {
            Color color = outerColor;
            color.a *= fade;

            outerLine.startColor = color;
            outerLine.endColor = color;

            outerLine.widthMultiplier =
                Mathf.Lerp(0.18f, 0.02f, t);
        }

        if (coreLine != null)
        {
            Color color = coreColor;
            color.a *= fade;

            coreLine.startColor = color;
            coreLine.endColor = color;

            coreLine.widthMultiplier =
                Mathf.Lerp(0.065f, 0.005f, t);
        }
    }
}
