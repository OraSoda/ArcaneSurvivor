
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerAim))]
public class AutoAttackController : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private ProjectileSpawner projectileSpawner;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attacksPerSecond = 2f;
    [SerializeField] private float spawnDistance = 0.9f;
    [SerializeField] private float spawnHeight = 0.3f;

    private PlayerAim playerAim;
    private UpgradeManager upgrades;
    private float attackTimer;

    public float BaseDamage => attackDamage;

    private void Awake()
    {
        playerAim = GetComponent<PlayerAim>();
        upgrades = GetComponent<UpgradeManager>();
    }

    public void SetBaseDamage(float value)
    {
        attackDamage = Mathf.Max(0f, value);
    }

    private bool Has(UpgradeId id)
    {
        return upgrades != null &&
               upgrades.HasUpgrade(id);
    }

    private void Update()
    {
        if (projectileSpawner == null)
            return;

        // Do not attack before starting or while paused.
        if (Time.timeScale <= 0f)
            return;

        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
            return;

        float rate = attacksPerSecond;

        if (Has(UpgradeId.DoubleFireRate))
            rate *= 2f;

        attackTimer =
            1f / Mathf.Max(0.1f, rate);

        Vector3 direction = playerAim.AimDirection;

        // Snapshot at the beginning of the attack.
        ProjectileShotData data = BuildShotData();

        // Play once per attack cycle.
        GameAudio.Play(
            data.IsNuclear
                ? GameSound.NuclearShoot
                : GameSound.Shoot);

        if (Has(UpgradeId.BurstFire))
        {
            StartCoroutine(FireBurst(direction, data));
        }
        else if (Has(UpgradeId.Multishot))
        {
            FireMultishot(direction, data);
        }
        else
        {
            FireProjectile(direction, data);
        }
    }

    public ProjectileShotData BuildShotData()
    {
        return new ProjectileShotData
        {
            Damage = Has(UpgradeId.NuclearDamage)
                ? attackDamage * 5f
                : attackDamage,

            // New: mark nuclear projectiles for visual effects.
            IsNuclear = Has(UpgradeId.NuclearDamage),

            SizeMultiplier =
                Has(UpgradeId.LargerProjectile) ? 1.5f : 1f,

            CanSplit = Has(UpgradeId.Split),
            CanPierce = Has(UpgradeId.Pierce),
            HasExplosion = Has(UpgradeId.ImpactExplosion),
            IsChild = false,

            ExplosionRadius = 2f,
            ExplosionDamageMultiplier = 0.5f,

            HasFreeze = Has(UpgradeId.AreaFreeze),
            FreezeRadius = 2f,
            FreezeDuration = 0.8f,

            HasSlow = Has(UpgradeId.AreaSlow),
            SlowRadius = 3f,
            SlowDuration = 4f,
            SlowMultiplier = 0.5f,

            HasDamageZone = Has(UpgradeId.DamageZone),
            DamageZoneRadius = 2f,
            DamageZoneDuration = 3f,
            DamageZoneInterval = 0.5f,
            DamageZoneMultiplier = 0.3f,

            HasChainDamage = Has(UpgradeId.ChainDamage),
            ChainRadius = 6f,
            ChainMaxTargets = 5
        };
    }

    private IEnumerator FireBurst(
        Vector3 direction,
        ProjectileShotData data)
    {
        for (int i = 0; i < 3; i++)
        {
            FireProjectile(direction, data);

            if (i < 2)
                yield return new WaitForSeconds(0.1f);
        }
    }

    private void FireMultishot(
        Vector3 direction,
        ProjectileShotData data)
    {
        float[] angles = { -15f, 0f, 15f };

        foreach (float angle in angles)
        {
            Vector3 shotDirection =
                Quaternion.Euler(0f, angle, 0f) *
                direction;

            FireProjectile(shotDirection, data);
        }
    }

    private void FireProjectile(
        Vector3 direction,
        ProjectileShotData data)
    {
        Vector3 spawnPosition =
            transform.position +
            direction * spawnDistance +
            Vector3.up * spawnHeight;

        projectileSpawner.Spawn(
            spawnPosition,
            direction,
            data
        );
    }
}
