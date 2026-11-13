
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class GameMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UpgradeManager upgrades;
    [SerializeField] private PlayerProgression progression;

    [Header("Localized Fonts")]
    [SerializeField] private TMP_FontAsset chineseFont;
    [SerializeField] private TMP_FontAsset japaneseFont;
    [SerializeField] private TMP_FontAsset englishFont;

    [Header("UI Art")]
    [SerializeField] private Sprite roundedPanelSprite;

    private TMP_FontAsset CurrentFont
    {
        get
        {
            return GameLocalization.Language switch
            {
                GameLanguage.Chinese => chineseFont,
                GameLanguage.Japanese => japaneseFont,
                _ => englishFont != null ? englishFont : chineseFont
            };
        }
    }

    private GameObject overlay;
    private RectTransform overlayRect;
    private View currentView = View.None;
    private bool gameStarted;
    private bool manuallyPaused;

    private enum View
    {
        None, Start, Upgrade, Pause,
        Victory, GameOver, Results
    }

    private readonly Color panelColor =
        new Color(0.973f, 0.98f, 0.988f, 1f);
    private readonly Color textColor =
        new Color(0.204f, 0.278f, 0.353f, 1f);
    private readonly Color accentColor =
        new Color(0.49f, 0.54f, 0.81f, 1f);

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (upgrades == null)
            upgrades = FindFirstObjectByType<UpgradeManager>();

        if (progression == null)
            progression = FindFirstObjectByType<PlayerProgression>();

        CreateCanvas();
    }

    private void Start()
    {
        gameStarted = false;
        manuallyPaused = false;
        Time.timeScale = 0f;
        RefreshView();
    }

    private void OnEnable()
    {
        GameLocalization.LanguageChanged += RefreshView;
    }

    private void OnDisable()
    {
        GameLocalization.LanguageChanged -= RefreshView;
    }

    private void Update()
    {
        bool blockingScreen = !gameStarted ||
            (upgrades != null && upgrades.IsSelecting) ||
            (gameManager != null &&
             gameManager.State != GameState.Playing &&
             gameManager.State != GameState.Endless);

        if (gameStarted && !blockingScreen &&
            Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            manuallyPaused = !manuallyPaused;
            Time.timeScale = manuallyPaused ? 0f : 1f;
        }

        View desired = DetermineView();

        if (desired != currentView)
            RefreshView();
    }

    private View DetermineView()
    {
        if (!gameStarted) return View.Start;

        if (gameManager != null)
        {
            switch (gameManager.State)
            {
                case GameState.Victory: return View.Victory;
                case GameState.GameOver: return View.GameOver;
                case GameState.Results: return View.Results;
            }
        }

        if (upgrades != null && upgrades.IsSelecting)
            return View.Upgrade;

        if (manuallyPaused) return View.Pause;

        return View.None;
    }

    private void CreateCanvas()
    {
        GameObject canvasObject = new GameObject(
            "MenuCanvas", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler =
            canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0f;

        overlayRect = CreateRect(
            "MenuOverlay",
            canvasObject.transform,
            Vector2.zero,
            Vector2.zero
        );

        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        UnityEngine.UI.Image image =
            overlayRect.gameObject.AddComponent<UnityEngine.UI.Image>();

        image.color = new Color(0.20f, 0.27f, 0.34f, 0.43f);
        overlay = overlayRect.gameObject;
        overlay.SetActive(false);
    }

    private void RefreshView()
    {
        if (overlay == null) return;

        currentView = DetermineView();

        for (int i = overlayRect.childCount - 1; i >= 0; i--)
            Destroy(overlayRect.GetChild(i).gameObject);

        overlay.SetActive(currentView != View.None);
        if (currentView == View.None) return;

        switch (currentView)
        {
            case View.Start: BuildStart(); break;
            case View.Pause: BuildPause(); break;
            case View.Upgrade: BuildUpgrade(); break;
            case View.Victory:
            case View.GameOver:
            case View.Results: BuildEnding(); break;
        }
    }

    private void BuildStart()
    {
        RectTransform panel = CreatePanel(760f, 610f);

        AddText(panel, GameLocalization.Get("title"),
            46, new Vector2(0, 195), new Vector2(690, 80), true);

        AddText(panel, GameLocalization.Get("subtitle"),
            23, new Vector2(0, 125), new Vector2(620, 48), true);

        AddButton(panel, GameLocalization.Get("start"),
            new Vector2(0, 35), new Vector2(420, 72),
            StartGame);

        AddLanguageButtons(panel, -85f);

        AddText(panel, GameLocalization.Get("controls"),
            19, new Vector2(0, -205),
            new Vector2(680, 52), true);
    }

    private void BuildPause()
    {
        RectTransform panel = CreatePanel(710f, 550f);

        AddText(panel, GameLocalization.Get("pause"),
            42, new Vector2(0, 180), new Vector2(620, 72), true);

        AddButton(panel, GameLocalization.Get("resume"),
            new Vector2(0, 75), new Vector2(430, 68),
            ResumeGame);

        AddButton(panel, GameLocalization.Get("title_return"),
            new Vector2(0, -10), new Vector2(430, 68),
            ReturnToTitle);

        AddLanguageButtons(panel, -145f);
    }

    private void BuildUpgrade()
    {
        RectTransform panel = CreatePanel(1110f, 570f);

        AddText(panel, GameLocalization.Get("choose"),
            40, new Vector2(0, 225), new Vector2(980, 65), true);

        AddText(panel,
            GameLocalization.Get("level") + " " +
            (progression != null ? progression.Level : 0) +
            "  -  " + GameLocalization.Get("choose_hint"),
            20, new Vector2(0, 177), new Vector2(960, 42), true);

        UpgradeId[] options =
            upgrades != null ? upgrades.CurrentOptions : null;

        if (options == null || options.Length != 3)
            return;

        for (int i = 0; i < 3; i++)
        {
            int index = i;
            UpgradeId id = options[i];
            float x = (i - 1) * 335f;

            RectTransform card = CreateRect(
                "Card_" + id,
                panel,
                new Vector2(x, -40),
                new Vector2(305, 345));


            UnityEngine.UI.Image background =
                card.gameObject.AddComponent<UnityEngine.UI.Image>();

            // Reuse the rounded panel sprite from the HUD.
            if (roundedPanelSprite != null)
            {
                background.sprite = roundedPanelSprite;
                background.type = UnityEngine.UI.Image.Type.Sliced;
            }

            // Soft blue-white card background.
            background.color =
                new Color(0.965f, 0.975f, 0.995f, 1f);

            // Button needs the background as its target graphic.
            background.raycastTarget = true;

            // Do not add Shadow or Outline to the card.
            // The outer menu panel already provides visual depth.


            Button button = card.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            ColorBlock colors = button.colors;
            colors.highlightedColor =
                new Color(0.91f, 0.94f, 1f);
            colors.pressedColor =
                new Color(0.82f, 0.86f, 0.96f);
            colors.selectedColor = Color.white;
            button.colors = colors;

            button.onClick.AddListener(
                () => upgrades.SelectUpgrade(index));


            RectTransform iconRect = CreateRect(
                "UpgradeIcon_" + id,
                card,
                new Vector2(0f, 100f),
                new Vector2(96f, 96f)
            );

            UnityEngine.UI.Image iconImage =
                iconRect.gameObject.AddComponent<UnityEngine.UI.Image>();

            iconImage.sprite = UpgradeIconDatabase.Get(id);
            iconImage.color = Color.white;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;


            AddText(card, GameLocalization.UpgradeName(id),
                30, new Vector2(0, 20),
                new Vector2(275, 75), true);

            AddText(card, GameLocalization.UpgradeDescription(id),
                21, new Vector2(0, -95),
                new Vector2(265, 115), true);
        }
    }

    private void BuildEnding()
    {
        RectTransform panel = CreatePanel(790f, 650f);

        string heading = currentView switch
        {
            View.Victory => "victory",
            View.GameOver => "gameover",
            _ => "results"
        };

        AddText(panel, GameLocalization.Get(heading),
            48, new Vector2(0, 245), new Vector2(720, 80), true);

        if (gameManager != null && progression != null)
        {
            int seconds = Mathf.FloorToInt(
                gameManager.ElapsedTime);

            string resultText =
                GameLocalization.Get("level") + ": " +
                progression.Level + "\n" +
                GameLocalization.Get("kills") + ": " +
                gameManager.KillCount + "\n" +
                GameLocalization.Get("time") + ": " +
                $"{seconds / 60:00}:{seconds % 60:00}" + "\n" +
                GameLocalization.Get("total_xp") + ": " +
                progression.TotalXP;

            AddText(panel, resultText,
                27, new Vector2(0, 83),
                new Vector2(620, 205), true);
        }

        if (currentView == View.Victory)
        {
            AddButton(panel, GameLocalization.Get("endless"),
                new Vector2(0, -110), new Vector2(470, 67),
                () => gameManager.StartEndlessMode());

            AddButton(panel, GameLocalization.Get("finish"),
                new Vector2(0, -198), new Vector2(470, 67),
                () => gameManager.FinishRun());
        }
        else
        {
            AddButton(panel, GameLocalization.Get("restart"),
                new Vector2(0, -140), new Vector2(470, 70),
                () => gameManager.RestartGame());

            AddButton(panel, GameLocalization.Get("title_return"),
                new Vector2(0, -230), new Vector2(470, 62),
                ReturnToTitle);
        }
    }

    private void StartGame()
    {
        gameStarted = true;
        manuallyPaused = false;
        Time.timeScale = 1f;
        RefreshView();
    }

    private void ResumeGame()
    {
        manuallyPaused = false;
        Time.timeScale = 1f;
        RefreshView();
    }

    private void ReturnToTitle()
    {
        Time.timeScale = 1f;
        gameManager.RestartGame();
    }


    private void AddLanguageButtons(
        RectTransform parent, float y)
    {
        AddText(
            parent,
            GameLocalization.Get("language"),
            21,
            new Vector2(0, y + 60),
            new Vector2(530, 35),
            true
        );

        // Chinese button always uses the Chinese font.
        AddButton(
            parent,
            "简体中文",
            new Vector2(-190, y),
            new Vector2(170, 55),
            () => GameLocalization.SetLanguage(
                GameLanguage.Chinese),
            chineseFont
        );

        // Japanese button always uses the Japanese font.
        AddButton(
            parent,
            "日本語",
            new Vector2(0, y),
            new Vector2(170, 55),
            () => GameLocalization.SetLanguage(
                GameLanguage.Japanese),
            japaneseFont
        );

        // English button uses its own font if assigned.
        AddButton(
            parent,
            "English",
            new Vector2(190, y),
            new Vector2(170, 55),
            () => GameLocalization.SetLanguage(
                GameLanguage.English),
            englishFont
        );
    }




    private RectTransform CreatePanel(float width, float height)
    {
        RectTransform rect = CreateRect(
            "MenuPanel",
            overlayRect,
            Vector2.zero,
            new Vector2(width, height)
        );

        UnityEngine.UI.Image image =
            rect.gameObject.AddComponent<UnityEngine.UI.Image>();

        // Same rounded sprite as HUD and upgrade cards.
        if (roundedPanelSprite != null)
        {
            image.sprite = roundedPanelSprite;
            image.type = UnityEngine.UI.Image.Type.Sliced;
        }

        image.color = panelColor;
        image.raycastTarget = false;

        // Soft shadow follows the rounded sprite.
        Shadow shadow =
            rect.gameObject.AddComponent<Shadow>();

        shadow.effectColor =
            new Color(0.20f, 0.27f, 0.40f, 0.16f);

        shadow.effectDistance =
            new Vector2(0f, -8f);

        shadow.useGraphicAlpha = true;

        // Keep the existing decorative accent line.
        float lineWidth = Mathf.Min(width - 96f, 620f);

        RectTransform line = CreateRect(
            "PanelAccent",
            rect,
            new Vector2(0f, height * 0.5f - 15f),
            new Vector2(lineWidth, 4f)
        );

        UnityEngine.UI.Image lineImage =
            line.gameObject.AddComponent<UnityEngine.UI.Image>();

        lineImage.color = new Color(
            accentColor.r,
            accentColor.g,
            accentColor.b,
            0.75f
        );

        lineImage.raycastTarget = false;

        return rect;
    }



    private void AddButton(
        RectTransform parent, string label,
        Vector2 position, Vector2 size,
        Action callback, TMP_FontAsset labelFont = null)
    {
        RectTransform rect =
            CreateRect("Button", parent, position, size);

        UnityEngine.UI.Image image =
            rect.gameObject.AddComponent<UnityEngine.UI.Image>();

        image.color = new Color(0.89f, 0.92f, 0.98f);

        // Subtle depth.
        Shadow shadow =
            rect.gameObject.AddComponent<Shadow>();

        shadow.effectColor =
            new Color(0.25f, 0.32f, 0.48f, 0.12f);
        shadow.effectDistance = new Vector2(0f, -3f);

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor =
            new Color(0.91f, 0.95f, 1f);
        colors.pressedColor =
            new Color(0.76f, 0.82f, 0.96f);
        colors.selectedColor = Color.white;
        colors.fadeDuration = 0.10f;

        button.colors = colors;

        // Keep the sound system from Phase 7.4.4-A.
        button.onClick.AddListener(() =>
        {
            GameAudio.Play(GameSound.Button);
            callback?.Invoke();
        });

        TMP_Text buttonText = AddText(
    rect, label, 27, Vector2.zero,
    size - new Vector2(20f, 10f), true);

        if (labelFont != null)
            buttonText.font = labelFont;
    }


    private TMP_Text AddText(
        RectTransform parent, string value, int fontSize,
        Vector2 position, Vector2 size,
        bool center, Color? colorOverride = null)
    {
        RectTransform rect =
            CreateRect("Text", parent, position, size);

        TextMeshProUGUI text =
            rect.gameObject.AddComponent<TextMeshProUGUI>();

        if (CurrentFont != null)
            text.font = CurrentFont;

        text.text = value;
        text.fontSize = fontSize;
        text.enableAutoSizing = false;
        text.color = colorOverride ?? textColor;
        text.alignment = center
            ? TextAlignmentOptions.Center
            : TextAlignmentOptions.MidlineLeft;
        text.textWrappingMode = TextWrappingModes.Normal; ;
        text.raycastTarget = false;
        return text;
    }

    private static RectTransform CreateRect(
        string name, Transform parent,
        Vector2 position, Vector2 size)
    {
        GameObject obj = new GameObject(
            name, typeof(RectTransform));

        obj.transform.SetParent(parent, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static string GetUpgradeSymbol(UpgradeId id)
    {
        return id switch
        {
            UpgradeId.Multishot => "III",
            UpgradeId.Split => "Y",
            UpgradeId.BurstFire => "3x",
            UpgradeId.LargerProjectile => "O",
            UpgradeId.ImpactExplosion => "*",
            UpgradeId.Pierce => ">>",
            UpgradeId.DoubleFireRate => "2x",
            UpgradeId.AreaFreeze => "*",
            UpgradeId.AreaSlow => "~",
            UpgradeId.DamageZone => "AoE",
            UpgradeId.ChainDamage => "Z",
            UpgradeId.NuclearDamage => "5x",
            _ => "+"
        };
    }
}
