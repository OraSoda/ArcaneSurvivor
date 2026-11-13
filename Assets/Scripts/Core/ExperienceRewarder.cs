
using UnityEngine;

public class ExperienceRewarder : MonoBehaviour
{
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private PlayerProgression progression;

    private void OnEnable()
    {
        if (enemySpawner != null)
            enemySpawner.EnemyKilled += OnEnemyKilled;
    }

    private void OnDisable()
    {
        if (enemySpawner != null)
            enemySpawner.EnemyKilled -= OnEnemyKilled;
    }

    private void OnEnemyKilled(EnemyHealth enemy)
    {
        if (progression == null) return;
        progression.AddXP(1);
    }
}
