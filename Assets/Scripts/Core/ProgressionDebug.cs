
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerProgression))]
public class ProgressionDebug : MonoBehaviour
{
    private PlayerProgression progression;

    private void Awake()
    {
        progression = GetComponent<PlayerProgression>();
    }

    private void OnEnable()
    {
        progression.RewardRequested += OnRewardRequested;
    }

    private void OnDisable()
    {
        progression.RewardRequested -= OnRewardRequested;
    }

    private void OnRewardRequested(int level)
    {
        UnityEngine.Debug.Log($"Level Up! Lv.{level}, Total XP: {progression.TotalXP}");
        progression.CompleteReward();
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.f9Key.wasPressedThisFrame)
        {
            progression.AddXP(1);
        }
    }
}
