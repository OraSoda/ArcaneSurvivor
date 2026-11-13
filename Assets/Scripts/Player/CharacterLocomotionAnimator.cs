
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterLocomotionAnimator : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float movingThreshold = 0.15f;

    private CharacterController controller;
    private PlayerHealth playerHealth;
    private EnemyHealth enemyHealth;
    private EnemyStatus enemyStatus;

    private static readonly int IsMoving =
        Animator.StringToHash("IsMoving");

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerHealth = GetComponent<PlayerHealth>();
        enemyHealth = GetComponent<EnemyHealth>();
        enemyStatus = GetComponent<EnemyStatus>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
            animator.applyRootMotion = false;
    }

    private void LateUpdate()
    {
        if (animator == null)
            return;

        bool dead =
            (playerHealth != null && playerHealth.IsDead) ||
            (enemyHealth != null && enemyHealth.IsDead);

        if (dead)
        {
            animator.speed = 0f;
            return;
        }

        float speedFactor = enemyStatus != null
            ? enemyStatus.SpeedMultiplier
            : 1f;

        animator.speed = Mathf.Max(0f, speedFactor);

        Vector3 velocity = controller.velocity;
        velocity.y = 0f;

        bool moving =
            velocity.sqrMagnitude >
            movingThreshold * movingThreshold;

        animator.SetBool(
            IsMoving,
            moving && speedFactor > 0f
        );
    }
}
