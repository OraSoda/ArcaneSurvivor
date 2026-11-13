
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerProgression))]
public class ProgressionTestInput : MonoBehaviour
{
    private PlayerProgression progression;

    private void Awake()
    {
        progression = GetComponent<PlayerProgression>();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // Give 1 XP.
        if (Keyboard.current.f9Key.wasPressedThisFrame)
        {
            progression.AddXP(1);
        }

        // Give exactly enough XP for the next level.
        if (Keyboard.current.f10Key.wasPressedThisFrame)
        {
            int needed =
                progression.XPToNextLevel - progression.CurrentXP;

            if (needed > 0)
                progression.AddXP(needed);
        }
    }
}
