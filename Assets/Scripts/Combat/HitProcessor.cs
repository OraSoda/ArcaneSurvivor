
using System;
using System.Collections.Generic;
using UnityEngine;

public static class HitProcessor
{
    // Explosion particle prefab loaded from Resources.
    private static ParticleSystem explosionVfxPrefab;
    private static ParticleSystem freezeVfxPrefab;
    private static ParticleSystem nuclearHitVfxPrefab;

    public static void ProcessDirectHit(
        MagicProjectile projectile,
        EnemyHealth target,
        Vector3 hitPosition)
    {
        if (projectile == null ||
            target == null ||
            target.IsDead)
            return;

        ProjectileShotData data = projectile.ShotData;
        Vector3 center = hitPosition;

        // 1. Direct damage.
        target.TakeDamage(data.Damage);

        // Play once for each valid direct hit.
        GameAudio.Play(
    data.IsNuclear
        ? GameSound.NuclearHit
        : GameSound.Hit);



        if (data.IsNuclear)
        {
            if (nuclearHitVfxPrefab == null)
            {
                nuclearHitVfxPrefab =
                    Resources.Load<ParticleSystem>(
                        "VFX_NuclearHit");
            }

            if (nuclearHitVfxPrefab != null)
            {
                UnityEngine.Object.Instantiate(
                    nuclearHitVfxPrefab,
                    center + Vector3.up * 0.5f,
                    Quaternion.identity);
            }
        }


        // 2. Split (main projectile only).
        if (data.CanSplit && !data.IsChild)
        {
            projectile.Spawner?.SpawnSplit(
                projectile, target);
        }

        // 3. Explosion: secondary damage.
        if (data.HasExplosion)
        {
            // Original explosion damage remains unchanged.
            ApplyExplosion(
                center,
                data.Damage * data.ExplosionDamageMultiplier,
                data.ExplosionRadius,
                target);

            // Load the particle prefab once.
            if (explosionVfxPrefab == null)
            {
                explosionVfxPrefab =
                    Resources.Load<ParticleSystem>(
                        "VFX_Explosion");
            }

            // Spawn purple explosion particles.
            if (explosionVfxPrefab != null)
            {
                UnityEngine.Object.Instantiate(
                    explosionVfxPrefab,
                    center + Vector3.up * 0.25f,
                    Quaternion.identity);
            }
        }



        // 4. One-time area freeze.
        if (data.HasFreeze)
        {
            // Apply the original freeze to all enemies in range.
            ApplyAreaStatus(
                center,
                data.FreezeRadius,
                data.FreezeDuration,
                true,
                1f);

            // One visual area centered at the direct hit.
            FreezeAreaVFX.Spawn(
                center,
                data.FreezeRadius,
                data.FreezeDuration);
        }



        // 5. One-time area slow.
        if (data.HasSlow)
        {
            // Original one-time area slow.
            ApplyAreaStatus(
                center,
                data.SlowRadius,
                data.SlowDuration,
                false,
                data.SlowMultiplier);

            // One-time purple visual impact.
            SlowImpactVFX.Spawn(
                center,
                data.SlowRadius);
        }


        // 6. Persistent damage zone.

        if (data.HasDamageZone)
        {
            // Keep the original damage calculation.
            DamageArea.Create(
                center,
                data.DamageZoneRadius,
                data.DamageZoneDuration,
                data.DamageZoneInterval,
                data.Damage * data.DamageZoneMultiplier);

            // Visual-only ground cracks.
            DamageCrackVFX.Spawn(
                center,
                data.DamageZoneRadius,
                data.DamageZoneDuration,
                data.DamageZoneInterval);
        }


        // 7. Chain damage: non-recursive.
        if (data.HasChainDamage)
        {
            ApplyChainDamage(
                center,
                target,
                data.Damage,
                data.ChainRadius,
                data.ChainMaxTargets);
        }
    }

    private static void ApplyExplosion(
        Vector3 center,
        float damage,
        float radius,
        EnemyHealth directTarget)
    {
        if (damage <= 0f || radius <= 0f)
            return;

        Collider[] colliders = Physics.OverlapSphere(
            center,
            radius,
            ~0,
            QueryTriggerInteraction.Collide);

        HashSet<int> damaged = new();

        foreach (Collider collider in colliders)
        {
            EnemyHealth enemy =
                collider.GetComponentInParent<EnemyHealth>();

            if (enemy == null ||
                enemy.IsDead ||
                enemy == directTarget)
                continue;

            if (!damaged.Add(enemy.GetInstanceID()))
                continue;

            enemy.TakeDamage(damage);
        }
    }

    private static void ApplyAreaStatus(
        Vector3 center,
        float radius,
        float duration,
        bool freeze,
        float slowMultiplier)
    {
        if (radius <= 0f || duration <= 0f)
            return;

        Collider[] colliders = Physics.OverlapSphere(
            center,
            radius,
            ~0,
            QueryTriggerInteraction.Collide);

        HashSet<int> affected = new();

        foreach (Collider collider in colliders)
        {
            EnemyHealth enemy =
                collider.GetComponentInParent<EnemyHealth>();

            if (enemy == null || enemy.IsDead)
                continue;

            if (!affected.Add(enemy.GetInstanceID()))
                continue;

            EnemyStatus status =
                enemy.GetComponent<EnemyStatus>();

            if (status == null)
                continue;

            if (freeze)
                status.ApplyFreeze(duration);
            else
                status.ApplySlow(
                    duration,
                    slowMultiplier);
        }
    }

    private static void ApplyChainDamage(
        Vector3 center,
        EnemyHealth directTarget,
        float damage,
        float radius,
        int maxTargets)
    {
        if (damage <= 0f ||
            radius <= 0f ||
            maxTargets <= 0)
            return;

        Collider[] colliders = Physics.OverlapSphere(
            center,
            radius,
            ~0,
            QueryTriggerInteraction.Collide);

        HashSet<int> seen = new();
        List<EnemyHealth> candidates = new();

        foreach (Collider collider in colliders)
        {
            EnemyHealth enemy =
                collider.GetComponentInParent<EnemyHealth>();

            if (enemy == null ||
                enemy.IsDead ||
                enemy == directTarget)
                continue;

            if (seen.Add(enemy.GetInstanceID()))
                candidates.Add(enemy);
        }

        // Target nearest eligible enemies first.
        candidates.Sort((a, b) =>
        {
            float da = (
                a.transform.position - center
            ).sqrMagnitude;

            float db = (
                b.transform.position - center
            ).sqrMagnitude;

            return da.CompareTo(db);
        });

        int count = Mathf.Min(
            maxTargets,
            candidates.Count);

        for (int i = 0; i < count; i++)
        {
            EnemyHealth enemy = candidates[i];

            if (enemy == null || enemy.IsDead)
                continue;

            Vector3 endPosition =
                enemy.transform.position;

            // Original secondary damage.
            enemy.TakeDamage(damage);

            // Visual only. No additional damage.
            MagicChainVFX.Spawn(
                center,
                endPosition);
        }
    }
}
