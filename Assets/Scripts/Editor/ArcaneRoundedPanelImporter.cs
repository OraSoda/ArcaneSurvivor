#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

// Editor-only import settings for the single rounded HUD panel sprite.
// No runtime component, no new Canvas, no change to HUD data logic.
public sealed class ArcaneRoundedPanelImporter : AssetPostprocessor
{
    private const string SpritePath = "Assets/Art/UI/HUD_RoundedPanel.png";
    private const float Border = 36f;

    private void OnPreprocessTexture()
    {
        if (!assetPath.Replace('\\', '/').EndsWith(
                "/HUD_RoundedPanel.png", StringComparison.OrdinalIgnoreCase))
            return;

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;

        // Sliced Image needs a full-rect mesh and a 9-slice border.
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteBorder = new Vector4(Border, Border, Border, Border);
        importer.SetTextureSettings(settings);
        importer.spriteBorder = new Vector4(Border, Border, Border, Border);
    }

    [MenuItem("Tools/ArcaneSurvivor/Reimport Rounded Panel Sprite")]
    private static void ReimportRoundedPanelSprite()
    {
        if (AssetDatabase.LoadAssetAtPath<Texture2D>(SpritePath) == null)
        {
            Debug.LogError("Rounded HUD panel not found at: " + SpritePath);
            return;
        }

        AssetDatabase.ImportAsset(SpritePath, ImportAssetOptions.ForceUpdate);
        Debug.Log("ArcaneSurvivor: rounded panel sprite reimported with 36px 9-slice borders.");
    }
}
#endif
