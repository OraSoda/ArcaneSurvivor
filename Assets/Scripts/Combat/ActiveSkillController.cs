
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(UpgradeManager))]
[RequireComponent(typeof(AutoAttackController))]
public class ActiveSkillController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ProjectileSpawner projectileSpawner;

    [Header("Radial Barrage")]
    [SerializeField] private float cooldown = 8f;
    [SerializeField] private int waveCount = 5;
    [SerializeField] private int projectilesPerWave = 12;
    [SerializeField] private float waveInterval = 0.15f;
    [SerializeField] private float rotationPerWave = 6f;
    [SerializeField] private float spawnDistance = 0.9f;
    [SerializeField] private float spawnHeight = 0.3f;

    private UpgradeManager upgrades;
    private AutoAttackController autoAttack;
    private PlayerHealth playerHealth;

    private float nextReadyTime;
    private bool isCasting;

    public bool IsUnlocked =>
        upgrades != null && upgrades.RadialBarrageUnlocked;

    public bool IsCasting => isCasting;

    public float CooldownRemaining =>
        Mathf.Max(0f, nextReadyTime - Time.time);

    public bool IsReady =>
        IsUnlocked &&
        !isCasting &&
        CooldownRemaining <= 0f &&
        (playerHealth == null || !playerHealth.IsDead);

    private void Awake()
    {
        upgrades = GetComponent<UpgradeManager>();
        autoAttack = GetComponent<AutoAttackController>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (!IsReady || projectileSpawner == null)
            return;

        if (Time.timeScale <= 0f || MouseOrKeyboardUnavailable())
            return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            // Snapshot once at the beginning of the cast.
            ProjectileShotData shotData = autoAttack.BuildShotData();

            nextReadyTime = Time.time + cooldown;
            StartCoroutine(CastRadialBarrage(shotData));
        }
    }

    private bool MouseOrKeyboardUnavailable()
    {
        return Keyboard.current == null;
    }

    private IEnumerator CastRadialBarrage(
        ProjectileShotData shotData)
    {
        isCasting = true;

        int waves = Mathf.Max(1, waveCount);
        int shots = Mathf.Max(1, projectilesPerWave);

        for (int wave = 0; wave < waves; wave++)
        {
            if (playerHealth != null && playerHealth.IsDead)
                break;

            float offsetAngle = wave * rotationPerWave;

            for (int i = 0; i < shots; i++)
            {
                float angle =
                    offsetAngle + i * 360f / shots;

                Vector3 direction =
                    Quaternion.Euler(0f, angle, 0f) *
                    Vector3.forward;

                Vector3 spawnPosition =
                    transform.position +
                    direction * spawnDistance +
                    Vector3.up * spawnHeight;

                projectileSpawner.Spawn(
                    spawnPosition,
                    direction,
                    shotData
                );
            }

            if (wave < waves - 1)
                yield return new WaitForSeconds(waveInterval);
        }

        isCasting = false;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isCasting = false;
    }
}
