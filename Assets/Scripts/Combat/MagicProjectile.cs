
using System.Collections.Generic;
using UnityEngine;

public class MagicProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 3f;

    [Header("Collision")]
    [SerializeField] private float hitRadius = 0.125f;
    [SerializeField] private LayerMask hitLayers = ~0;

    private Vector3 direction;
    private float remainingLifetime;
    private float radiusMultiplier = 1f;
    private Vector3 originalScale;

    private ProjectileShotData shotData;
    private ProjectileSpawner spawner;

    // NEW: Visual controller for nuclear projectiles.
    private NuclearProjectileVisual nuclearVisual;

    private readonly HashSet<int> hitTargets = new();

    public Vector3 Direction => direction;
    public ProjectileShotData ShotData => shotData;
    public ProjectileSpawner Spawner => spawner;

    private void Awake()
    {
        originalScale = transform.localScale;

        // NEW: Cache the optional nuclear visual component.
        nuclearVisual = GetComponent<NuclearProjectileVisual>();
    }

    public void Initialize(
        Vector3 fireDirection,
        ProjectileShotData data,
        ProjectileSpawner owner,
        int ignoredEnemyId,
        float sizeMultiplier)
    {
        direction = fireDirection.normalized;
        shotData = data;
        spawner = owner;

        remainingLifetime = lifetime;
        radiusMultiplier = Mathf.Max(0.1f, sizeMultiplier);

        transform.localScale =
            originalScale * radiusMultiplier;

        hitTargets.Clear();

        if (ignoredEnemyId != 0)
            hitTargets.Add(ignoredEnemyId);

        // NEW: Apply nuclear appearance once per projectile.
        if (nuclearVisual != null)
            nuclearVisual.Apply(shotData.IsNuclear);
    }

    private void Update()
    {
        if (remainingLifetime <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 displacement =
            direction * speed * Time.deltaTime;

        float distance = displacement.magnitude;

        RaycastHit[] hits = Physics.SphereCastAll(
            transform.position,
            hitRadius * radiusMultiplier,
            direction,
            distance,
            hitLayers,
            QueryTriggerInteraction.Collide
        );

        System.Array.Sort(
            hits,
            (a, b) => a.distance.CompareTo(b.distance)
        );

        foreach (RaycastHit hit in hits)
        {
            EnemyHealth enemy =
                hit.collider.GetComponentInParent<EnemyHealth>();

            if (enemy == null || enemy.IsDead)
                continue;

            int enemyId = enemy.GetInstanceID();

            if (!hitTargets.Add(enemyId))
                continue;

            // One valid direct hit per target.
            Vector3 hitPosition =
                hit.collider.bounds.center;

            HitProcessor.ProcessDirectHit(
                this,
                enemy,
                hitPosition
            );

            // Infinite piercing if unlocked.
            if (!shotData.CanPierce)
            {
                Destroy(gameObject);
                return;
            }
        }

        transform.position += displacement;
        remainingLifetime -= Time.deltaTime;

        if (remainingLifetime <= 0f)
            Destroy(gameObject);
    }
}
