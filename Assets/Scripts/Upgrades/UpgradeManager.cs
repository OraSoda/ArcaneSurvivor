
using System;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeId
{
    Multishot,
    Split,
    BurstFire,

    LargerProjectile,
    ImpactExplosion,
    Pierce,

    DoubleFireRate,
    AreaFreeze,
    AreaSlow,

    DamageZone,
    ChainDamage,
    NuclearDamage
}

[RequireComponent(typeof(PlayerProgression))]
public class UpgradeManager : MonoBehaviour
{
    private static readonly UpgradeId[] Level1 =
    {
        UpgradeId.Multishot,
        UpgradeId.Split,
        UpgradeId.BurstFire
    };

    private static readonly UpgradeId[] Level2 =
    {
        UpgradeId.LargerProjectile,
        UpgradeId.ImpactExplosion,
        UpgradeId.Pierce
    };

    private static readonly UpgradeId[] Level4 =
    {
        UpgradeId.DoubleFireRate,
        UpgradeId.AreaFreeze,
        UpgradeId.AreaSlow
    };

    private static readonly UpgradeId[] Level5 =
    {
        UpgradeId.DamageZone,
        UpgradeId.ChainDamage,
        UpgradeId.NuclearDamage
    };

    private readonly HashSet<UpgradeId> selectedUpgrades = new();

    private readonly List<UpgradeId> selectedOrder = new();

    public IReadOnlyList<UpgradeId> SelectedUpgrades => selectedOrder;

    private PlayerProgression progression;

    public bool IsSelecting { get; private set; }
    public bool RadialBarrageUnlocked { get; private set; }

    public UpgradeId[] CurrentOptions { get; private set; }

    public event Action<UpgradeId> UpgradeSelected;
    public event Action RadialBarrageUnlockedEvent;

    public bool HasUpgrade(UpgradeId id)
    {
        return selectedUpgrades.Contains(id);
    }

    private void Awake()
    {
        progression = GetComponent<PlayerProgression>();
    }

    private void OnEnable()
    {
        progression.RewardRequested += HandleReward;
    }

    private void OnDisable()
    {
        progression.RewardRequested -= HandleReward;

        if (IsSelecting)
        {
            IsSelecting = false;
            Time.timeScale = 1f;
        }
    }

    private void HandleReward(int level)
    {
        if (level == 3)
        {
            RadialBarrageUnlocked = true;
            RadialBarrageUnlockedEvent?.Invoke();

            UnityEngine.Debug.Log(
                "Lv.3: Radial Barrage unlocked!");

            progression.CompleteReward();
            return;
        }

        CurrentOptions = level switch
        {
            1 => Level1,
            2 => Level2,
            4 => Level4,
            5 => Level5,
            _ => null
        };

        if (CurrentOptions == null)
        {
            progression.CompleteReward();
            return;
        }

        IsSelecting = true;
        Time.timeScale = 0f;
    }


    public void SelectUpgrade(int index)
    {
        if (!IsSelecting || CurrentOptions == null)
            return;

        if (index < 0 || index >= CurrentOptions.Length)
            return;

        UpgradeId selected = CurrentOptions[index];

        if (selectedUpgrades.Add(selected))
        {
            selectedOrder.Add(selected);
            UpgradeSelected?.Invoke(selected);
        }

        UnityEngine.Debug.Log(
            $"Upgrade selected: {selected}");

        IsSelecting = false;
        CurrentOptions = null;

        // Resume before processing any remaining XP.
        Time.timeScale = 1f;

        // This may trigger another upgrade and pause again.
        progression.CompleteReward();
    }


    public static string GetUpgradeName(UpgradeId id)
    {
        return id switch
        {
            UpgradeId.Multishot => "Multishot",
            UpgradeId.Split => "Split",
            UpgradeId.BurstFire => "Burst Fire",
            UpgradeId.LargerProjectile => "Larger Projectile",
            UpgradeId.ImpactExplosion => "Impact Explosion",
            UpgradeId.Pierce => "Infinite Pierce",
            UpgradeId.DoubleFireRate => "Double Fire Rate",
            UpgradeId.AreaFreeze => "Area Freeze",
            UpgradeId.AreaSlow => "Area Slow",
            UpgradeId.DamageZone => "Damage Zone",
            UpgradeId.ChainDamage => "Chain Damage",
            UpgradeId.NuclearDamage => "Nuclear Damage",
            _ => id.ToString()
        };
    }
}
