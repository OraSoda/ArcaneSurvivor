
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private MagicProjectile projectilePrefab;

    [Header("Split Settings")]
    [SerializeField] private float childSizeMultiplier = 0.55f;
    [SerializeField] private float childDamageMultiplier = 0.4f;
    [SerializeField] private float splitSpawnOffset = 0.85f;

    public MagicProjectile Spawn(
        Vector3 position,
        Vector3 direction,
        ProjectileShotData data,
        int ignoredEnemyId = 0)
    {
        if (projectilePrefab == null)
            return null;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector3.forward;

        direction.Normalize();

        MagicProjectile projectile = Instantiate(
            projectilePrefab,
            position,
            Quaternion.LookRotation(direction)
        );

        float finalSize = data.SizeMultiplier;

        if (data.IsChild)
            finalSize *= childSizeMultiplier;

        projectile.Initialize(
            direction,
            data,
            this,
            ignoredEnemyId,
            finalSize
        );

        return projectile;
    }

    public void SpawnSplit(
        MagicProjectile parent,
        EnemyHealth hitEnemy)
    {
        if (parent == null || hitEnemy == null)
            return;

        if (!parent.ShotData.CanSplit ||
            parent.ShotData.IsChild)
            return;

        float[] angles = { -30f, 0f, 30f };

        ProjectileShotData childData =
            parent.ShotData.CreateChild(
                childDamageMultiplier
            );

        int ignoredId = hitEnemy.GetInstanceID();

        foreach (float angle in angles)
        {
            Vector3 direction =
                Quaternion.Euler(0f, angle, 0f) *
                parent.Direction;

            Vector3 position =
                hitEnemy.transform.position +
                direction.normalized * splitSpawnOffset;

            Spawn(
                position,
                direction,
                childData,
                ignoredId
            );
        }
    }
}
