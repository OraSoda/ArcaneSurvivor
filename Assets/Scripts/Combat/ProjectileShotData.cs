
public struct ProjectileShotData
{
    public float Damage;
    public float SizeMultiplier;

    // Projectile behavior
    public bool CanSplit;
    public bool CanPierce;
    public bool HasExplosion;
    public bool IsChild;

    // Lv.2 Explosion
    public float ExplosionRadius;
    public float ExplosionDamageMultiplier;

    // Lv.4 Freeze
    public bool HasFreeze;
    public float FreezeRadius;
    public float FreezeDuration;

    // Lv.4 Slow
    public bool HasSlow;
    public float SlowRadius;
    public float SlowDuration;
    public float SlowMultiplier;

    // Lv.5 Damage Zone
    public bool HasDamageZone;
    public float DamageZoneRadius;
    public float DamageZoneDuration;
    public float DamageZoneInterval;
    public float DamageZoneMultiplier;

    // Lv.5 Chain Damage
    public bool HasChainDamage;
    public float ChainRadius;
    public int ChainMaxTargets;

    public bool IsNuclear;

    public ProjectileShotData CreateChild(float damageMultiplier)
    {
        ProjectileShotData child = this;
        child.Damage *= damageMultiplier;
        child.CanSplit = false;
        child.IsChild = true;
        return child;
    }
}
