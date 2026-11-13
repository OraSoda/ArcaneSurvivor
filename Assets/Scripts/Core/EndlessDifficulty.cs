
using UnityEngine;

public class EndlessDifficulty : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerProgression progression;

    [Header("Difficulty Growth Per Minute")]
    [SerializeField] private float speedGrowth = 0.08f;
    [SerializeField] private float damageGrowth = 0.12f;
    [SerializeField] private float spawnGrowth = 0.15f;

    [Header("Maximum Multipliers")]
    [SerializeField] private float maxSpeed = 1.8f;
    [SerializeField] private float maxDamage = 3f;
    [SerializeField] private float maxSpawn = 3f;

    [Header("Endless Enemy HP Growth")]
    [SerializeField] private float healthGrowthPerMinute = 0.15f;
    [SerializeField] private float healthGrowthPerLevel = 0.10f;
    [SerializeField] private float maxHealthMultiplier = 3f;
    [SerializeField, Min(1f)] private float endlessBaseHealthMultiplier = 2f;

    public float SpeedMultiplier =>
        GetMultiplier(speedGrowth, maxSpeed);

    public float DamageMultiplier =>
        GetMultiplier(damageGrowth, maxDamage);

    public float SpawnMultiplier =>
        GetMultiplier(spawnGrowth, maxSpawn);

    public float HealthMultiplier
    {
        get
        {
            if (gameManager == null || !gameManager.IsEndless)
                return 1f;

            float minutes =
                gameManager.EndlessElapsedTime / 60f;

            int extraLevels = progression != null
                ? Mathf.Max(0, progression.Level - 6)
                : 0;

            float timeMultiplier =
                1f + minutes * Mathf.Max(
                    0f, healthGrowthPerMinute);

            float levelMultiplier =
                1f + extraLevels * Mathf.Max(
                    0f, healthGrowthPerLevel);

            float growth = Mathf.Max(
                timeMultiplier, levelMultiplier) - 1f;

            return Mathf.Clamp(
                endlessBaseHealthMultiplier + growth,
                1f,
                Mathf.Max(1f, maxHealthMultiplier)
            );
        }
    }

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (progression == null)
            progression = FindFirstObjectByType<PlayerProgression>();
    }

    private float GetMultiplier(float growth, float maximum)
    {
        if (gameManager == null || !gameManager.IsEndless)
            return 1f;

        float minutes = gameManager.EndlessElapsedTime / 60f;

        return Mathf.Min(
            Mathf.Max(1f, maximum),
            1f + minutes * Mathf.Max(0f, growth)
        );
    }
}
