
using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class GameHUDController : MonoBehaviour
{
    [Header("Localized Fonts")]
    [SerializeField] private TMP_FontAsset chineseFont;
    [SerializeField] private TMP_FontAsset japaneseFont;
    [SerializeField] private TMP_FontAsset englishFont;

    [Header("Game References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerProgression progression;
    [SerializeField] private AutoAttackController autoAttack;
    [SerializeField] private ActiveSkillController activeSkill;
    [SerializeField] private UpgradeManager upgrades;
    [SerializeField] private GameManager gameManager;

    [Header("HP")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Image hpFill;

    [Header("Level and Attack")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text attackText;

    [Header("XP")]
    [SerializeField] private TMP_Text xpText;
    [SerializeField] private Image xpFill;

    [Header("Statistics")]
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text killsText;

    [Header("Active Skill")]
    [SerializeField] private TMP_Text skillText;

    [Header("Upgrades")]
    [SerializeField] private TMP_Text upgradesText;

    private static readonly UpgradeId[] UpgradeOrder =
    {
        UpgradeId.Multishot,
        UpgradeId.Split,
        UpgradeId.BurstFire,
        UpgradeId.LargerProjectile,
        UpgradeId.ImpactExplosion,
        UpgradeId.Pierce,
        UpgradeId.DoubleFireRate,
        UpgradeId.AreaFreeze,
        UpgradeId.AreaSlow,
        UpgradeId.DamageZone,
        UpgradeId.ChainDamage,
        UpgradeId.NuclearDamage
    };

    private readonly StringBuilder upgradeBuilder = new();

    private void Awake()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (progression == null)
            progression = FindFirstObjectByType<PlayerProgression>();

        if (autoAttack == null)
            autoAttack = FindFirstObjectByType<AutoAttackController>();

        if (activeSkill == null)
            activeSkill = FindFirstObjectByType<ActiveSkillController>();

        if (upgrades == null)
            upgrades = FindFirstObjectByType<UpgradeManager>();

        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
    }

    private void Update()
    {
        UpdateHealth();
        UpdateProgression();
        UpdateStatistics();
        UpdateActiveSkill();
        UpdateUpgrades();
    }


    private void UpdateHealth()
    {
        if (playerHealth == null) return;

        float current = Mathf.Max(
            0f, playerHealth.CurrentHealth);

        float maximum = Mathf.Max(
            1f, playerHealth.MaxHealth);

        if (hpText != null)
        {
            hpText.text =
                $"{GameLocalization.Get("health")} " +
                $"{Mathf.CeilToInt(current)} / " +
                $"{Mathf.CeilToInt(maximum)}";
        }

        if (hpFill != null)
        {
            float ratio = Mathf.Clamp01(
                current / maximum);

            RectTransform fillRect =
                hpFill.rectTransform;

            RectTransform backgroundRect =
                fillRect.parent as RectTransform;

            if (backgroundRect != null)
            {
                const float inset = 3f;

                float maxWidth = Mathf.Max(
                    0f,
                    backgroundRect.rect.width - inset * 2f
                );

                float width = ratio <= 0f
                    ? 0f
                    : Mathf.Min(
                        maxWidth,
                        Mathf.Max(16f, maxWidth * ratio)
                    );

                fillRect.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal,
                    width
                );
            }

            hpFill.gameObject.SetActive(ratio > 0f);
        }
    }


    private void UpdateProgression()
    {
        if (progression == null) return;

        string level = GameLocalization.Get("level");

        if (levelText != null)
            levelText.text = $"{level} {progression.Level}";

        if (attackText != null && autoAttack != null)
            attackText.text =
                $"{GameLocalization.Get("attack")} " +
                $"{autoAttack.BaseDamage:0.#}";

        int required = Mathf.Max(1, progression.XPToNextLevel);
        int current = progression.CurrentXP;

        if (xpText != null)
            xpText.text =
                $"{level} {progression.Level} - " +
                $"{current} / {required} XP";


        if (xpFill != null)
        {
            float ratio = Mathf.Clamp01(
                (float)current / required
            );

            RectTransform fillRect =
                xpFill.rectTransform;

            RectTransform backgroundRect =
                fillRect.parent as RectTransform;

            if (backgroundRect != null)
            {
                const float inset = 3f;

                float maxWidth = Mathf.Max(
                    0f,
                    backgroundRect.rect.width - inset * 2f
                );

                float width = ratio <= 0f
                    ? 0f
                    : Mathf.Min(
                        maxWidth,
                        Mathf.Max(16f, maxWidth * ratio)
                    );

                fillRect.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal,
                    width
                );
            }

            xpFill.gameObject.SetActive(ratio > 0f);
        }

    }

    private void UpdateStatistics()
    {
        if (gameManager == null) return;

        int seconds = Mathf.FloorToInt(
            gameManager.ElapsedTime
        );

        if (timeText != null)
            timeText.text =
                $"{seconds / 60:00}:{seconds % 60:00}";

        if (killsText != null)
            killsText.text =
        $"{gameManager.KillCount} " +
        GameLocalization.Get("kills");
    }

    private void UpdateActiveSkill()
    {
        if (skillText == null) return;

        if (activeSkill == null || !activeSkill.IsUnlocked)
        {
            skillText.text =
                "SPACE\n" + GameLocalization.Get("locked");
            return;
        }

        if (activeSkill.IsCasting)
        {
            skillText.text =
                "SPACE\n" + GameLocalization.Get("casting");
            return;
        }

        float remaining = activeSkill.CooldownRemaining;

        skillText.text = remaining <= 0f
            ? "SPACE\n" + GameLocalization.Get("ready")
            : $"SPACE\n{remaining:0.0}s";
    }

    private void UpdateUpgrades()
    {
        if (upgradesText == null || upgrades == null)
            return;

        upgradeBuilder.Clear();

        foreach (UpgradeId id in UpgradeOrder)
        {
            if (!upgrades.HasUpgrade(id))
                continue;

            if (upgradeBuilder.Length > 0)
                upgradeBuilder.Append(" / ");

            upgradeBuilder.Append(
                GameLocalization.UpgradeName(id)
            );
        }

        upgradesText.text = upgradeBuilder.Length == 0
            ? GameLocalization.Get("upgrades") +
              ": " + GameLocalization.Get("none")
            : upgradeBuilder.ToString();
    }


    private void ApplyLanguageFont()
    {
        TMP_FontAsset font = GameLocalization.Language switch
        {
            GameLanguage.Chinese => chineseFont,
            GameLanguage.Japanese => japaneseFont,
            _ => englishFont != null ? englishFont : chineseFont
        };

        if (font == null)
            return;

        TMP_Text[] labels =
        {
        hpText, levelText, attackText, xpText,
        timeText, killsText, skillText, upgradesText
    };

        foreach (TMP_Text label in labels)
        {
            if (label != null)
                label.font = font;
        }
    }

    private void OnEnable()
    {
        GameLocalization.LanguageChanged += ApplyLanguageFont;
        ApplyLanguageFont();
    }

    private void OnDisable()
    {
        GameLocalization.LanguageChanged -= ApplyLanguageFont;
    }

}
