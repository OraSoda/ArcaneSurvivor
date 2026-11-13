
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

public class DamageCrackVFX : MonoBehaviour
{
    private static Material crackMaterial;

    private readonly List<LineRenderer> lines =
        new List<LineRenderer>();

    private MaterialPropertyBlock block;

    private float elapsed;
    private float duration;
    private float pulseInterval;

    private static readonly Color CrackColor =
        new Color(1f, 0.16f, 0.42f, 1f);

    public static void Spawn(
        Vector3 center,
        float radius,
        float duration,
        float interval)
    {
        if (radius <= 0f || duration <= 0f)
            return;

        if (crackMaterial == null)
        {
            crackMaterial = Resources.Load<Material>(
                "Mat_DamageCracks");
        }

        if (crackMaterial == null)
        {
            UnityEngine.Debug.LogWarning(
                "Mat_DamageCracks not found in Resources.");
            return;
        }

        GameObject obj = new GameObject("VFX_DamageCracks");

        obj.transform.position = new Vector3(
            center.x, 0.065f, center.z);

        DamageCrackVFX effect =
            obj.AddComponent<DamageCrackVFX>();

        effect.Initialize(radius, duration, interval);
    }

    private void Initialize(
        float radius,
        float lifetime,
        float interval)
    {
        duration = lifetime;
        pulseInterval = Mathf.Max(0.01f, interval);
        block = new MaterialPropertyBlock();

        // Eight main cracks extend outwards.
        for (int i = 0; i < 8; i++)
        {
            float angle =
                i * Mathf.PI * 2f / 8f +
                Mathf.Sin(i * 2.7f) * 0.15f;

            Vector3[] points = new Vector3[5];

            for (int j = 0; j < 5; j++)
            {
                float t = j / 4f;
                float r = radius * (0.06f + 0.84f * t);

                float offset =
                    Mathf.Sin(i * 3.9f + j * 2.4f) *
                    0.16f * t;

                float a = angle + offset;

                points[j] = new Vector3(
                    Mathf.Cos(a) * r,
                    0f,
                    Mathf.Sin(a) * r);
            }

            CreateLine(points, 0.075f);

            // Small secondary crack branches.
            if (i % 2 == 0)
            {
                float branchAngle = angle + 0.55f;

                Vector3 start = points[2];

                Vector3 middle = start + new Vector3(
                    Mathf.Cos(branchAngle),
                    0f,
                    Mathf.Sin(branchAngle)
                ) * radius * 0.14f;

                Vector3 end = middle + new Vector3(
                    Mathf.Cos(branchAngle + 0.2f),
                    0f,
                    Mathf.Sin(branchAngle + 0.2f)
                ) * radius * 0.15f;

                CreateLine(
                    new Vector3[] { start, middle, end },
                    0.045f);
            }
        }

        UpdateVisual();
    }

    private void CreateLine(
        Vector3[] points,
        float width)
    {
        GameObject obj = new GameObject("MagicCrack");
        obj.transform.SetParent(transform, false);

        LineRenderer line = obj.AddComponent<LineRenderer>();

        line.sharedMaterial = crackMaterial;
        line.useWorldSpace = false;
        line.loop = false;
        line.positionCount = points.Length;
        line.SetPositions(points);

        line.startWidth = width;
        line.endWidth = width * 0.55f;

        line.numCornerVertices = 3;
        line.numCapVertices = 2;

        line.startColor = Color.white;
        line.endColor = Color.white;

        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;

        lines.Add(line);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        if (elapsed >= duration)
        {
            Destroy(gameObject);
            return;
        }

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        // A short light pulse every damage interval.
        float phase = Mathf.Repeat(
            elapsed, pulseInterval);

        float pulse = 1f - Mathf.Clamp01(
            phase / 0.14f);

        float fade = Mathf.Clamp01(
            (duration - elapsed) / 0.25f);

        Color color = CrackColor;
        color.a = (0.55f + pulse * 0.45f) * fade;

        block.SetColor("_BaseColor", color);

        foreach (LineRenderer line in lines)
        {
            if (line != null)
                line.SetPropertyBlock(block);
        }
    }
}
