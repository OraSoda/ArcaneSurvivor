
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float invulnerabilityTime = 0.5f;
    [SerializeField] private float redFlashTime = 0.15f;

    [Header("Character Visual")]
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private Transform visualRoot;

    public event Action Died;
    public event Action<float, float> HealthChanged;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;
    public bool IsDead { get; private set; }

    private float invulnerableUntil;
    private float redUntil;
    private MaterialPropertyBlock propertyBlock;

    private static readonly int BaseColorId =
        Shader.PropertyToID("_BaseColor");

    private static readonly int ColorId =
        Shader.PropertyToID("_Color");

    private struct ColorTarget
    {
        public Renderer renderer;
        public int materialIndex;
        public int propertyId;
        public Color originalColor;
    }

    private readonly List<ColorTarget> colorTargets = new();

    private void Awake()
    {
        CurrentHealth = maxHealth;
        propertyBlock = new MaterialPropertyBlock();

        if (bodyRenderer == null)
            bodyRenderer = GetComponent<Renderer>();

        InitializeVisuals();
    }

    private void InitializeVisuals()
    {
        colorTargets.Clear();

        Renderer[] renderers;

        if (visualRoot != null)
        {
            renderers =
                visualRoot.GetComponentsInChildren<Renderer>(true);
        }
        else if (bodyRenderer != null)
        {
            renderers = new Renderer[] { bodyRenderer };
        }
        else
        {
            UnityEngine.Debug.LogWarning(
                "PlayerHealth: No character renderer found.");
            return;
        }

        foreach (Renderer targetRenderer in renderers)
        {
            if (targetRenderer == null)
                continue;

            Material[] materials =
                targetRenderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];

                if (material == null)
                    continue;

                int propertyId;

                if (material.HasProperty(BaseColorId))
                    propertyId = BaseColorId;
                else if (material.HasProperty(ColorId))
                    propertyId = ColorId;
                else
                    continue;

                colorTargets.Add(new ColorTarget
                {
                    renderer = targetRenderer,
                    materialIndex = i,
                    propertyId = propertyId,
                    originalColor = material.GetColor(propertyId)
                });
            }
        }

        if (colorTargets.Count == 0)
        {
            UnityEngine.Debug.LogWarning(
                "PlayerHealth: No compatible material color " +
                "properties found on the character model.");
        }
    }

    private void Update()
    {
        bool isFlashing = Time.time < redUntil;

        foreach (ColorTarget target in colorTargets)
        {
            if (target.renderer == null ||
                !target.renderer.enabled)
                continue;

            target.renderer.GetPropertyBlock(
                propertyBlock,
                target.materialIndex
            );

            Color displayColor = isFlashing
                ? Color.red
                : target.originalColor;

            propertyBlock.SetColor(
                target.propertyId,
                displayColor
            );

            target.renderer.SetPropertyBlock(
                propertyBlock,
                target.materialIndex
            );
        }
    }

    public void TakeDamage(float damage)
    {
        if (IsDead ||
            damage <= 0f ||
            Time.time < invulnerableUntil)
            return;

        CurrentHealth = Mathf.Max(
            0f,
            CurrentHealth - damage
        );

        invulnerableUntil =
            Time.time + invulnerabilityTime;

        redUntil =
            Time.time + redFlashTime;

        HealthChanged?.Invoke(
            CurrentHealth,
            maxHealth
        );

        if (CurrentHealth <= 0f)
        {
            IsDead = true;

            PlayerMovement movement =
                GetComponent<PlayerMovement>();

            PlayerAim aim =
                GetComponent<PlayerAim>();

            AutoAttackController attack =
                GetComponent<AutoAttackController>();

            if (movement != null)
                movement.enabled = false;

            if (aim != null)
                aim.enabled = false;

            if (attack != null)
                attack.enabled = false;

            Died?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f)
            return;

        CurrentHealth = Mathf.Min(
            maxHealth,
            CurrentHealth + amount
        );

        HealthChanged?.Invoke(
            CurrentHealth,
            maxHealth
        );
    }
}
