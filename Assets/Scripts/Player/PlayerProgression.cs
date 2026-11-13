
using System;
using UnityEngine;

public class PlayerProgression : MonoBehaviour
{
    private static readonly int[] NormalXP =
    {
        3, 15, 30, 65, 120
    };

    public const int NormalMaxLevel = 5;

    public int Level { get; private set; }
    public int CurrentXP { get; private set; }
    public int TotalXP { get; private set; }

    public int XPToNextLevel => GetRequiredXP(Level);

    public event Action<int, int> XPChanged;
    public event Action<int> LevelChanged;
    public event Action<int> RewardRequested;

    private PlayerHealth health;
    private AutoAttackController attack;

    private bool rewardPending;
    private bool endlessUnlocked;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
        attack = GetComponent<AutoAttackController>();
    }

    private static int GetRequiredXP(int level)
    {
        if (level < NormalXP.Length)
            return NormalXP[level];

        return level switch
        {
            5 => 200,
            6 => 300,
            7 => 450,
            8 => 650,
            9 => 900,
            _ => (int)Math.Min(
                1000000000L,
                900L + (long)(level - 9) * 300L
            )
        };
    }

    public void AddXP(int amount)
    {
        if (amount <= 0)
            return;

        if (health != null && health.IsDead)
            return;

        // No XP gain while Victory is awaiting a decision.
        if (Level >= 6 && !endlessUnlocked)
            return;

        int accepted = (int)Math.Min(
            (long)amount,
            int.MaxValue - (long)CurrentXP
        );

        if (accepted <= 0)
            return;

        CurrentXP += accepted;
        TotalXP = (int)Math.Min(
            int.MaxValue,
            (long)TotalXP + accepted
        );

        ProcessLevelUps();
    }

    private void ProcessLevelUps()
    {
        while (!rewardPending &&
               CurrentXP >= XPToNextLevel)
        {
            // Lv.6 is the normal-mode victory boundary.
            // Further level-ups require Endless Mode.
            if (Level >= 6 && !endlessUnlocked)
                break;

            CurrentXP -= XPToNextLevel;
            Level++;

            health?.Heal(15f);
            attack?.SetBaseDamage(10f + Level);

            bool needsReward =
    Level >= 1 && Level <= 5;

            if (needsReward)
                rewardPending = true;

            LevelChanged?.Invoke(Level);

            if (needsReward)
            {
                RewardRequested?.Invoke(Level);
            }

            // Lv.6 triggers Victory through GameManager,
            // not a new three-choice reward.
            if (Level == 6 && !endlessUnlocked)
                break;
        }

        XPChanged?.Invoke(CurrentXP, XPToNextLevel);
    }

    public void CompleteReward()
    {
        rewardPending = false;
        ProcessLevelUps();
    }

    public void EnableEndlessProgression()
    {
        if (endlessUnlocked)
            return;

        endlessUnlocked = true;
        ProcessLevelUps();
    }
}
