
using System.Collections.Generic;
using UnityEngine;

public class DamageArea : MonoBehaviour
{
    private float radius;
    private float damage;
    private float interval;
    private float expireTime;
    private float nextTickTime;

    private static Material sharedVisualMaterial;

    public static DamageArea Create(
        Vector3 position,
        float radius,
        float duration,
        float tickInterval,
        float damage)
    {
        GameObject areaObject = new GameObject("DamageArea");
        position.y = 0.05f;
        areaObject.transform.position = position;

        DamageArea area = areaObject.AddComponent<DamageArea>();

        area.radius = Mathf.Max(0.1f, radius);
        area.damage = Mathf.Max(0f, damage);
        area.interval = Mathf.Max(0.05f, tickInterval);
        area.expireTime = Time.time + Mathf.Max(0.1f, duration);
        area.nextTickTime = Time.time + area.interval;

        area.CreateVisual();
        return area;
    }

    private void CreateVisual()
    {
        // Outer boundary - red
        CreateRing(
            "OuterRing",
            radius,
            0.065f,
            new Color(0.902f, 0.180f, 0.341f, 0.90f)
        );

        // Inner decorative ring - bright pink
        CreateRing(
            "InnerRing",
            radius * 0.72f,
            0.025f,
            new Color(1f, 0.478f, 0.620f, 0.75f)
        );
    }

    private void Update()
    {
        if (Time.time >= expireTime)
        {
            Destroy(gameObject);
            return;
        }

        if (Time.time < nextTickTime)
            return;

        ApplyDamage();
        nextTickTime += interval;
    }

    private void ApplyDamage()
    {
        Vector3 center =
            transform.position + Vector3.up;

        Collider[] colliders = Physics.OverlapSphere(
            center,
            radius,
            ~0,
            QueryTriggerInteraction.Collide
        );

        HashSet<int> damaged = new();

        foreach (Collider collider in colliders)
        {
            EnemyHealth enemy =
                collider.GetComponentInParent<EnemyHealth>();

            if (enemy == null || enemy.IsDead)
                continue;

            if (!damaged.Add(enemy.GetInstanceID()))
                continue;

            // Secondary damage only.
            enemy.TakeDamage(damage);
        }
    }

    private void CreateRing(
        string objectName,
        float ringRadius,
        float width,
        Color color)
    {
        GameObject ring = new GameObject(objectName);
        ring.transform.SetParent(transform, false);
        ring.transform.localPosition = new Vector3(0f, 0.05f, 0f);

        LineRenderer line = ring.AddComponent<LineRenderer>();

        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 64;
        line.widthMultiplier = width;
        line.numCornerVertices = 3;
        line.startColor = color;
        line.endColor = color;
        line.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;

        if (sharedVisualMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");

            if (shader == null)
                shader = Shader.Find(
                    "Universal Render Pipeline/Particles/Unlit");

            if (shader != null)
                sharedVisualMaterial = new Material(shader);
        }

        if (sharedVisualMaterial != null)
            line.sharedMaterial = sharedVisualMaterial;

        for (int i = 0; i < 64; i++)
        {
            float angle = i * Mathf.PI * 2f / 64f;

            line.SetPosition(
                i,
                new Vector3(
                    Mathf.Cos(angle) * ringRadius,
                    0f,
                    Mathf.Sin(angle) * ringRadius
                )
            );
        }
    }
}
