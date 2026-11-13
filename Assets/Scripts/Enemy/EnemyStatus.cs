
using UnityEngine;

public class EnemyStatus : MonoBehaviour
{
    [Header("Status Visual")]
    [SerializeField] private Renderer statusRenderer;

    [SerializeField]
    private Color freezeColor =
        new Color(0.60f, 0.85f, 0.96f);

    [SerializeField]
    private Color slowColor =
        new Color(0.70f, 0.66f, 0.91f);

    private MaterialPropertyBlock statusBlock;
    private Color originalColor = Color.white;

    private EnemyHealth health;

    private float freezeUntil;
    private float slowUntil;
    private float slowMultiplier = 1f;

    public bool IsFrozen => Time.time < freezeUntil;
    public bool IsSlowed => Time.time < slowUntil;

    public float SpeedMultiplier
    {
        get
        {
            if (IsFrozen) return 0f;
            if (IsSlowed) return slowMultiplier;
            return 1f;
        }
    }

    public void ApplyFreeze(float duration)
    {
        if (duration <= 0f) return;

        freezeUntil = Mathf.Max(
            freezeUntil,
            Time.time + duration
        );
    }

    public void ApplySlow(float duration, float multiplier)
    {
        if (duration <= 0f) return;

        float newEnd = Time.time + duration;
        multiplier = Mathf.Clamp01(multiplier);

        if (IsSlowed)
            slowMultiplier = Mathf.Min(
                slowMultiplier, multiplier);
        else
            slowMultiplier = multiplier;

        slowUntil = Mathf.Max(slowUntil, newEnd);
    }

    public void ResetStatus()
    {
        freezeUntil = 0f;
        slowUntil = 0f;
        slowMultiplier = 1f;
    }

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();

        // Use the existing character model renderer.
        if (statusRenderer == null)
        {
            Transform visualRoot =
                transform.Find("VisualRoot");

            if (visualRoot != null)
            {
                statusRenderer =
                    visualRoot.GetComponentInChildren<Renderer>(true);
            }
            else
            {
                statusRenderer =
                    GetComponentInChildren<Renderer>();
            }
        }

        statusBlock = new MaterialPropertyBlock();

        if (statusRenderer != null &&
            statusRenderer.sharedMaterial != null &&
            statusRenderer.sharedMaterial.HasProperty("_BaseColor"))
        {
            originalColor =
                statusRenderer.sharedMaterial.GetColor("_BaseColor");
        }
    }

    private void LateUpdate()
    {
        // Do not update status colors after death.
        if (health != null && health.IsDead)
            return;

        if (statusRenderer == null)
            return;

        Color displayColor = originalColor;

        if (IsFrozen)
            displayColor = freezeColor;
        else if (IsSlowed)
            displayColor = slowColor;

        statusRenderer.GetPropertyBlock(statusBlock);
        statusBlock.SetColor("_BaseColor", displayColor);
        statusRenderer.SetPropertyBlock(statusBlock);
    }
}
