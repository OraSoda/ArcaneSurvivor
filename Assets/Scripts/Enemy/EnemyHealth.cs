
using System;
using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 20f;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float deathDuration = 0.5f;

    public event Action<EnemyHealth> Died;

    public bool IsDead { get; private set; }
    public float CurrentHealth { get; private set; }

    private CharacterController characterController;
    private EnemyController enemyController;
    private bool healthDifficultyApplied;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        enemyController = GetComponent<EnemyController>();
        CurrentHealth = maxHealth;
    }
    public void ApplySpawnHealthMultiplier(float multiplier)
    {
        if (IsDead || healthDifficultyApplied)
            return;

        healthDifficultyApplied = true;

        maxHealth *= Mathf.Max(1f, multiplier);
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);

        if (CurrentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        if (enemyController != null)
            enemyController.enabled = false;

        if (characterController != null)
            characterController.enabled = false;

        Died?.Invoke(this);
        StartCoroutine(DeathAnimation());
    }

    private IEnumerator DeathAnimation()
    {
        if (visualRoot != null)
        {
            Quaternion start = visualRoot.localRotation;
            Quaternion end = start * Quaternion.Euler(0f, 0f, 90f);

            float elapsed = 0f;

            while (elapsed < deathDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / deathDuration);
                visualRoot.localRotation =
                    Quaternion.Slerp(start, end, t);
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
