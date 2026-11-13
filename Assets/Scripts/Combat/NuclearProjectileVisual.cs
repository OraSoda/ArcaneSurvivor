
using UnityEngine;

[RequireComponent(typeof(MagicProjectile))]
public class NuclearProjectileVisual : MonoBehaviour
{
    [Header("Existing Projectile")]
    [SerializeField] private MeshRenderer headRenderer;
    [SerializeField] private TrailRenderer trailRenderer;

    [Header("Nuclear Appearance")]
    [SerializeField] private GameObject nuclearCore;
    [SerializeField] private Material nuclearHeadMaterial;
    [SerializeField] private Material nuclearTrailMaterial;

    private Material originalHeadMaterial;
    private Material originalTrailMaterial;
    private Gradient originalTrailGradient;

    private void Awake()
    {
        if (headRenderer == null)
            headRenderer = GetComponent<MeshRenderer>();

        if (trailRenderer == null)
            trailRenderer = GetComponent<TrailRenderer>();

        if (headRenderer != null)
            originalHeadMaterial = headRenderer.sharedMaterial;

        if (trailRenderer != null)
        {
            originalTrailMaterial = trailRenderer.sharedMaterial;
            originalTrailGradient = trailRenderer.colorGradient;
        }

        if (nuclearCore != null)
            nuclearCore.SetActive(false);
    }

    public void Apply(bool isNuclear)
    {
        // Only switch materials. Never change scale or collider.
        if (headRenderer != null)
        {
            headRenderer.sharedMaterial =
                isNuclear && nuclearHeadMaterial != null
                    ? nuclearHeadMaterial
                    : originalHeadMaterial;
        }

        if (nuclearCore != null)
            nuclearCore.SetActive(isNuclear);

        if (trailRenderer != null)
        {
            trailRenderer.sharedMaterial =
                isNuclear && nuclearTrailMaterial != null
                    ? nuclearTrailMaterial
                    : originalTrailMaterial;

            trailRenderer.colorGradient =
                isNuclear
                    ? CreateNuclearGradient()
                    : originalTrailGradient;

            // Clear any trail left from a previous use.
            trailRenderer.Clear();
        }
    }


    private static Gradient CreateNuclearGradient()
    {
        Gradient gradient = new Gradient();

        gradient.SetKeys(
            new GradientColorKey[]
            {
            // Bright yellow-green near the projectile.
            new GradientColorKey(
                new Color(0.80f, 1.00f, 0.05f), 0f),

            // Strong saturated green.
            new GradientColorKey(
                new Color(0.40f, 0.85f, 0.00f), 0.45f),

            // Darker green toward the end.
            new GradientColorKey(
                new Color(0.20f, 0.48f, 0.00f), 1f)
            },
            new GradientAlphaKey[]
            {
            new GradientAlphaKey(1.00f, 0f),
            new GradientAlphaKey(0.90f, 0.45f),
            new GradientAlphaKey(0.00f, 1f)
            }
        );

        return gradient;
    }

}
