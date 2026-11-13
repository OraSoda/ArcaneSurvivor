
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class FreezeAreaVFX : MonoBehaviour
{
    [SerializeField] private Transform freezeDisk;
    [SerializeField] private ParticleSystem freezeParticles;

    private static FreezeAreaVFX prefab;

    private readonly HashSet<int> affectedEnemies = new();

    private Vector3 areaCenter;
    private float areaRadius;
    private float freezeDuration;
    private float expiresAt;
    private float nextCheckTime;

    private const float CheckInterval = 0.05f;

    public static void Spawn(
        Vector3 hitPosition,
        float radius,
        float duration)
    {
        if (radius <= 0f || duration <= 0f)
            return;

        if (prefab == null)
        {
            prefab = Resources.Load<FreezeAreaVFX>(
                "VFX_FreezeArea");
        }

        if (prefab == null)
        {
            UnityEngine.Debug.LogWarning(
                "VFX_FreezeArea prefab not found.");
            return;
        }

        Vector3 position = new Vector3(
            hitPosition.x,
            0.03f,
            hitPosition.z);

        FreezeAreaVFX effect = Instantiate(
            prefab,
            position,
            Quaternion.identity);

        effect.Initialize(hitPosition, radius, duration);

        Destroy(effect.gameObject, duration);
    }

    private void Initialize(
        Vector3 hitPosition,
        float radius,
        float duration)
    {
        areaCenter = hitPosition;
        areaRadius = radius;
        freezeDuration = duration;
        expiresAt = Time.time + duration;
        nextCheckTime = Time.time + CheckInterval;

        if (freezeDisk != null)
        {
            Vector3 scale = freezeDisk.localScale;
            scale.x = radius * 2f;
            scale.z = radius * 2f;
            freezeDisk.localScale = scale;
        }

        if (freezeParticles != null)
        {
            var shape = freezeParticles.shape;
            shape.radius = radius;

            freezeParticles.Play();
        }

        // HitProcessor already froze enemies at impact.
        // Record them so this field does not freeze them twice.
        CheckEnemies(false);
    }

    private void Update()
    {
        if (Time.time >= expiresAt)
            return;

        if (Time.time < nextCheckTime)
            return;

        nextCheckTime = Time.time + CheckInterval;

        // Freeze only newly entering enemies.
        CheckEnemies(true);
    }

    private void CheckEnemies(bool freezeNewEnemies)
    {
        Collider[] colliders = Physics.OverlapSphere(
            areaCenter,
            areaRadius,
            ~0,
            QueryTriggerInteraction.Collide);

        foreach (Collider collider in colliders)
        {
            EnemyHealth enemy =
                collider.GetComponentInParent<EnemyHealth>();

            if (enemy == null || enemy.IsDead)
                continue;

            int id = enemy.GetInstanceID();

            // Each field affects an enemy only once.
            if (!affectedEnemies.Add(id))
                continue;

            if (!freezeNewEnemies)
                continue;

            EnemyStatus status =
                enemy.GetComponent<EnemyStatus>();

            if (status != null)
                status.ApplyFreeze(freezeDuration);
        }
    }
}
