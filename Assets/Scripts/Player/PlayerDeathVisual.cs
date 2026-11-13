
using System.Collections;
using System.Diagnostics;
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerDeathVisual : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Transform visualRoot;

    [Header("Fall Animation")]
    [SerializeField] private float fallDuration = 0.5f;
    [SerializeField] private float fallAngle = 85f;
    [SerializeField] private Vector3 fallAxis = Vector3.forward;

    private PlayerHealth health;
    private bool deathStarted;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();

        if (visualRoot == null)
            visualRoot = transform.Find("VisualRoot");
    }

    private void OnEnable()
    {
        if (health == null)
            health = GetComponent<PlayerHealth>();

        health.Died += PlayDeath;
    }

    private void OnDisable()
    {
        if (health != null)
            health.Died -= PlayDeath;
    }

    public void PlayDeath()
    {
        if (deathStarted)
            return;

        if (visualRoot == null)
        {
            UnityEngine.Debug.LogError(
                "PlayerDeathVisual: VisualRoot is missing.");
            return;
        }

        deathStarted = true;

        // Stop skeletal movement before falling.
        Animator[] animators =
            visualRoot.GetComponentsInChildren<Animator>(true);

        foreach (Animator animator in animators)
            animator.speed = 0f;

        StartCoroutine(AnimateFall());
    }

    private IEnumerator AnimateFall()
    {
        Quaternion startRotation = visualRoot.localRotation;

        Vector3 axis = fallAxis.sqrMagnitude > 0.0001f
            ? fallAxis.normalized
            : Vector3.forward;

        Quaternion targetRotation =
            startRotation *
            Quaternion.AngleAxis(fallAngle, axis);

        float duration = Mathf.Max(0.01f, fallDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);

            visualRoot.localRotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    t
                );

            yield return null;
        }

        visualRoot.localRotation = targetRotation;
    }
}
