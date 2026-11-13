
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(EnemyStatus))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float contactDistance = 1.1f;
    [SerializeField] private float gravity = -9.81f;

    // NEW: Enemy facing
    [Header("Facing")]
    [SerializeField] private float rotationSpeed = 12f;

    private Transform target;
    private PlayerHealth playerHealth;
    private CharacterController controller;
    private EnemyHealth health;
    private EnemyStatus status;
    private float verticalVelocity;
    private EndlessDifficulty endlessDifficulty;
    private float stageSpeedMultiplier = 1f;

    public void SetStageSpeedMultiplier(float value)
    {
        stageSpeedMultiplier = Mathf.Max(0f, value);
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<EnemyHealth>();
        status = GetComponent<EnemyStatus>();
    }

    private void Start()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            target = player.transform;
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        endlessDifficulty =
            FindFirstObjectByType<EndlessDifficulty>();
    }

    private void Update()
    {
        if (Time.timeScale <= 0f ||
            target == null ||
            health.IsDead)
            return;

        if (playerHealth != null && playerHealth.IsDead)
            return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        // NEW: Smoothly face the player while not frozen.
        if (distance > 0.05f &&
            (status == null || !status.IsFrozen))
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(
                    direction.normalized,
                    Vector3.up
                );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Mathf.Clamp01(rotationSpeed * Time.deltaTime)
            );
        }

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        float speedMultiplier =
            status != null ? status.SpeedMultiplier : 1f;

        float difficultySpeed =
            endlessDifficulty != null
                ? endlessDifficulty.SpeedMultiplier
                : 1f;

        Vector3 movement = Vector3.zero;

        if (distance > 0.05f)
            movement = direction.normalized *
                moveSpeed *
                stageSpeedMultiplier *
                speedMultiplier *
                difficultySpeed;

        movement.y = verticalVelocity;

        controller.Move(movement * Time.deltaTime);

        if (status != null && status.IsFrozen)
            return;

        if (distance <= contactDistance && playerHealth != null)
        {
            float difficultyDamage =
                endlessDifficulty != null
                    ? endlessDifficulty.DamageMultiplier
                    : 1f;

            playerHealth.TakeDamage(
                contactDamage * difficultyDamage
            );
        }
    }
}
