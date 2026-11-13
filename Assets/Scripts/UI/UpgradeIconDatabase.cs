using System.Collections.Generic;
using UnityEngine;

// Visuals only: returns the PNG matching an existing UpgradeId.
// Required location: Assets/Resources/UpgradeIcons/<UpgradeId>.png
public static class UpgradeIconDatabase
{
    private static readonly Dictionary<UpgradeId, Sprite> Cache = new();

    public static Sprite Get(UpgradeId id)
    {
        if (Cache.TryGetValue(id, out Sprite sprite))
            return sprite;

        sprite = Resources.Load<Sprite>("UpgradeIcons/" + id);
        Cache[id] = sprite;

        if (sprite == null)
            Debug.LogWarning("Upgrade icon not found: " + id);

        return sprite;
    }
}
