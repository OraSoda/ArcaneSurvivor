
using UnityEngine;

public class UpgradeSelectionUI : MonoBehaviour
{
    [SerializeField] private UpgradeManager upgradeManager;

    private GUIStyle titleStyle;
    private GUIStyle buttonStyle;

    private void Awake()
    {
        if (upgradeManager == null)
            upgradeManager = FindFirstObjectByType<UpgradeManager>();
    }

    private void OnGUI()
    {
        if (upgradeManager == null ||
            !upgradeManager.IsSelecting)
            return;

        UpgradeId[] options = upgradeManager.CurrentOptions;

        if (options == null || options.Length != 3)
            return;

        float panelWidth = Mathf.Min(Screen.width - 20f, 560f);
        float panelHeight = 340f;

        Rect panel = new Rect(
            (Screen.width - panelWidth) / 2f,
            (Screen.height - panelHeight) / 2f,
            panelWidth,
            panelHeight
        );

        GUI.Box(panel, "");

        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.fontSize = 24;
            titleStyle.fontStyle = FontStyle.Bold;

            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 17;
            buttonStyle.alignment = TextAnchor.MiddleCenter;
        }

        GUI.Label(
            new Rect(panel.x, panel.y + 15, panel.width, 50),
            "CHOOSE AN UPGRADE",
            titleStyle
        );

        float buttonWidth = (panel.width - 50f) / 3f;

        for (int i = 0; i < 3; i++)
        {
            Rect buttonRect = new Rect(
                panel.x + 10f + i * (buttonWidth + 15f),
                panel.y + 100f,
                buttonWidth,
                170f
            );

            string label =
                UpgradeManager.GetUpgradeName(options[i]);

            if (GUI.Button(buttonRect, label, buttonStyle))
            {
                upgradeManager.SelectUpgrade(i);
                break;
            }
        }
    }
}
