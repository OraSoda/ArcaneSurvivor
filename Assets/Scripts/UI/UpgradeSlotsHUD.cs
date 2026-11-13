
using System.Collections.Generic;
using UnityEngine;
using UIImage = UnityEngine.UI.Image;

public class UpgradeSlotsHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UpgradeManager upgrades;

    [Header("Dynamic Layout")]
    [SerializeField] private float iconSize = 72f;
    [SerializeField] private float spacing = 12f;
    [SerializeField] private float padding = 16f;
    [SerializeField, Min(1)] private int maxColumns = 4;

    private readonly List<UIImage> icons = new();

    private RectTransform panelRect;
    private UIImage panelBackground;
    private Vector2 initialSize;
    private Vector2 initialPosition;

    private void Awake()
    {
        if (upgrades == null)
            upgrades = FindFirstObjectByType<UpgradeManager>();

        panelRect = GetComponent<RectTransform>();
        panelBackground = GetComponent<UIImage>();

        initialSize = panelRect.sizeDelta;
        initialPosition = panelRect.anchoredPosition;
    }

    private void OnEnable()
    {
        if (upgrades != null)
            upgrades.UpgradeSelected += OnUpgradeSelected;
    }

    private void Start()
    {
        Refresh();
    }

    private void OnDisable()
    {
        if (upgrades != null)
            upgrades.UpgradeSelected -= OnUpgradeSelected;
    }

    private void OnUpgradeSelected(UpgradeId id)
    {
        Refresh();
    }

    public void Refresh()
    {
        if (upgrades == null)
            return;

        IReadOnlyList<UpgradeId> selected =
            upgrades.SelectedUpgrades;

        int count = selected.Count;
        int columns = Mathf.Min(
            Mathf.Max(1, maxColumns),
            Mathf.Max(1, count)
        );

        int rows = count == 0
            ? 0
            : (count + columns - 1) / columns;

        float width = count == 0
            ? initialSize.x
            : padding * 2f
              + columns * iconSize
              + (columns - 1) * spacing;

        float height = count == 0
            ? initialSize.y
            : padding * 2f
              + rows * iconSize
              + (rows - 1) * spacing;

        // Keep the original left and bottom edges in place.
        panelRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal, width);

        panelRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical, height);

        panelRect.anchoredPosition = initialPosition
            + new Vector2(
                (width - initialSize.x) * panelRect.pivot.x,
                (height - initialSize.y) * panelRect.pivot.y
            );

        // Empty selection should not leave a white rectangle.
        if (panelBackground != null)
            panelBackground.enabled = count > 0;

        while (icons.Count < count)
            CreateIcon();

        float left = -width * 0.5f + padding;
        float top = height * 0.5f - padding;

        for (int i = 0; i < icons.Count; i++)
        {
            bool visible = i < count;
            UIImage image = icons[i];

            image.gameObject.SetActive(visible);

            if (!visible)
                continue;

            int col = i % columns;
            int row = i / columns;

            RectTransform rect =
                image.GetComponent<RectTransform>();

            rect.anchoredPosition = new Vector2(
                left + iconSize * 0.5f
                     + col * (iconSize + spacing),
                top - iconSize * 0.5f
                    - row * (iconSize + spacing)
            );

            Sprite sprite = UpgradeIconDatabase.Get(selected[i]);

            image.sprite = sprite;
            image.enabled = sprite != null;
            image.gameObject.name =
                "UpgradeIcon_" + selected[i];
        }
    }

    private void CreateIcon()
    {
        GameObject obj = new GameObject(
            "UpgradeIcon",
            typeof(RectTransform),
            typeof(UIImage)
        );

        obj.transform.SetParent(transform, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(iconSize, iconSize);

        UIImage image = obj.GetComponent<UIImage>();
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;

        icons.Add(image);
    }
}
