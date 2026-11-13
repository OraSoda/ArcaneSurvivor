
using System;
using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private EnemyHealth normalPrefab;
    [SerializeField] private EnemyHealth fastPrefab;
    [SerializeField] private EnemyHealth tankPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private float minSpawnDistance = 20f;
    [SerializeField] private float maxSpawnDistance = 26f;
    [SerializeField] private int maxAliveEnemies = 80;

    [Header("Debug Settings")]
    [SerializeField] private bool debugSpawnMode = false;
    [SerializeField] private float debugSpawnRate = 120f;

    [Header("Normal Run Pacing")]
    [SerializeField, Range(1f, 5f)]

    private float normalStageSpeed = 3f;
    [Header("Debug Stage Preview")]
    [SerializeField] private bool previewSpawnStage = false;
    [SerializeField, Min(0f)]
    private float previewMinutes = 18f;

    [SerializeField] private EndlessDifficulty endlessDifficulty;
    [SerializeField] private GameManager gameManager;

    private float elapsedTime;
    private float spawnTimer;
    private int aliveCount;
    private readonly HashSet<EnemyHealth> activeEnemies = new();
    private readonly List<EnemyHealth> removalBuffer = new();
    private bool openingWaveSpawned;

    private float despawnCheckTimer;

    public event System.Action<EnemyHealth> EnemyKilled;

    private void Start()
    {
        if (player == null)
        {
            GameObject obj =
                GameObject.FindGameObjectWithTag("Player");

            if (obj != null)
                player = obj.transform;
        }

        if (endlessDifficulty == null)
            endlessDifficulty =
                FindFirstObjectByType<EndlessDifficulty>();
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
    }

    private void Update()
    {
        if (player == null)
            return;

        elapsedTime += Time.deltaTime;
        spawnTimer += Time.deltaTime;

        if (!openingWaveSpawned && Time.deltaTime > 0f)
        {
            openingWaveSpawned = true;

            for (int i = 0; i < 3 && aliveCount < maxAliveEnemies; i++)
                SpawnEnemy();
        }

        despawnCheckTimer += Time.deltaTime;

        if (despawnCheckTimer >= 1f)
        {
            despawnCheckTimer = 0f;
            DespawnDistantEnemies();
        }

        float rate = debugSpawnMode
            ? debugSpawnRate
            : GetSpawnRate();

        if (!debugSpawnMode &&
            endlessDifficulty != null)
        {
            rate *= endlessDifficulty.SpawnMultiplier;
        }
        // Limit spawning pressure in long Endless runs.
        if (!debugSpawnMode)
            rate = Mathf.Min(rate, 180f);

        if (rate <= 0f)
            return;

        float interval = 60f / rate;

        int spawnAttempts = 0;
        const int maxSpawnAttemptsPerFrame = 5;

        while (spawnTimer >= interval &&
               spawnAttempts < maxSpawnAttemptsPerFrame)
        {
            spawnTimer -= interval;
            spawnAttempts++;

            if (aliveCount < maxAliveEnemies)
                SpawnEnemy();
        }
    }

    // NEW: Only overrides the spawn stage for testing.
    private float GetSpawnMinutes()
    {
        float minutes = previewSpawnStage
            ? Mathf.Max(0f, previewMinutes)
            : elapsedTime / 60f * normalStageSpeed;

        // Endless must not return to early spawn stages.
        if (gameManager != null && gameManager.IsEndless)
            minutes = Mathf.Max(minutes, 18f);

        return minutes;
    }


    private float GetSpawnRate()
    {
        float minutes = GetSpawnMinutes();

        if (minutes < 1f) return 70f;
        if (minutes < 5f) return 85f;
        if (minutes < 10f) return 90f;
        if (minutes < 18f) return 105f;

        return 120f;
    }


    private EnemyHealth ChoosePrefab()
    {
        float minutes = GetSpawnMinutes();
        float roll = UnityEngine.Random.value;

        if (minutes < 1f)
            return roll < 0.85f ? normalPrefab : fastPrefab;

        if (minutes < 5f)
            return roll < 0.50f ? normalPrefab : fastPrefab;

        if (minutes < 10f)
        {
            if (roll < 0.50f) return normalPrefab;
            if (roll < 0.90f) return fastPrefab;
            return tankPrefab;
        }

        if (minutes < 18f)
        {
            if (roll < 0.55f) return normalPrefab;
            if (roll < 0.85f) return fastPrefab;
            return tankPrefab;
        }

        if (roll < 0.45f) return normalPrefab;
        if (roll < 0.80f) return fastPrefab;
        return tankPrefab;
    }


    private void SpawnEnemy()
    {
        EnemyHealth prefab = ChoosePrefab();
        if (prefab == null) return;

        float minutes = GetSpawnMinutes();

        float distance = UnityEngine.Random.Range(
            minSpawnDistance, maxSpawnDistance);


        float frontChance = 0f;

        if (minutes >= 18f)
            frontChance = 0.45f;
        else if (minutes >= 10f)
            frontChance = 0.30f;
        else if (minutes >= 5f)
            frontChance = 0.25f;
        else
            frontChance = 0.50f;


        Vector3 offset;
        CharacterController playerController =
            player.GetComponent<CharacterController>();

        Vector3 playerVelocity = playerController != null
            ? playerController.velocity
            : Vector3.zero;

        playerVelocity.y = 0f;

        if (playerVelocity.sqrMagnitude > 0.25f &&
            UnityEngine.Random.value < frontChance)
        {
            // Spawn in a broad forward sector,
            // still 20-26 units away.
            float angle = UnityEngine.Random.Range(-60f, 60f);

            offset = Quaternion.Euler(0f, angle, 0f) *
                     playerVelocity.normalized * distance;
        }
        else
        {
            float angle = UnityEngine.Random.Range(
                0f, Mathf.PI * 2f);

            offset = new Vector3(
                Mathf.Cos(angle), 0f, Mathf.Sin(angle)
            ) * distance;
        }

        Vector3 position = player.position + offset;
        position.y = 1f;

        EnemyHealth enemy = Instantiate(
            prefab, position, Quaternion.identity);

        float healthMultiplier = 1f;

        if (gameManager != null && gameManager.IsEndless)
        {
            // Endless uses its own HP difficulty system.
            if (endlessDifficulty != null)
                healthMultiplier = endlessDifficulty.HealthMultiplier;
        }
        else
        {
            // Normal mode: HP grows with the spawn stage.

            if (minutes >= 18f)
                healthMultiplier = 1.60f;
            else if (minutes >= 10f)
                healthMultiplier = 1.35f;
            else if (minutes >= 5f)
                healthMultiplier = 1.15f;
        }

        enemy.ApplySpawnHealthMultiplier(healthMultiplier);


        // Only Fast receives a late-stage speed bonus.
        if (prefab == fastPrefab)
        {
            float multiplier = minutes >= 18f
                ? 1.30f
                : minutes >= 1f ? 1.20f : 1f;

            EnemyController enemyController =
                enemy.GetComponent<EnemyController>();

            if (enemyController != null)
                enemyController.SetStageSpeedMultiplier(multiplier);
        }

        aliveCount++;
        activeEnemies.Add(enemy);
        enemy.Died += OnEnemyDied;
    }


    private void DespawnDistantEnemies()
    {
        if (player == null) return;

        const float despawnDistance = 60f;
        float maxDistanceSq =
            despawnDistance * despawnDistance;

        removalBuffer.Clear();

        foreach (EnemyHealth enemy in activeEnemies)
        {
            if (enemy == null)
            {
                removalBuffer.Add(enemy);
                continue;
            }

            Vector3 difference =
                enemy.transform.position - player.position;

            difference.y = 0f;

            if (difference.sqrMagnitude > maxDistanceSq)
                removalBuffer.Add(enemy);
        }

        foreach (EnemyHealth enemy in removalBuffer)
        {
            if (!activeEnemies.Remove(enemy))
                continue;

            aliveCount = Mathf.Max(0, aliveCount - 1);

            if (enemy != null)
            {
                enemy.Died -= OnEnemyDied;

                // Despawn is not a kill. No XP is awarded.
                Destroy(enemy.gameObject);
            }
        }

        removalBuffer.Clear();
    }


    private void OnEnemyDied(EnemyHealth enemy)
    {
        enemy.Died -= OnEnemyDied;
        activeEnemies.Remove(enemy);

        aliveCount = Mathf.Max(
            0, aliveCount - 1);

        EnemyKilled?.Invoke(enemy);
    }
}
