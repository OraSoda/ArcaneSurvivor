
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    Victory,
    Endless,
    Results,
    GameOver
}

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerProgression progression;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerDeathVisual deathVisual;
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Death Settings")]
    [SerializeField] private float deathAnimationDelay = 0.65f;

    [SerializeField] private bool useLegacyUI = false;

    private GameState state = GameState.Playing;
    private float elapsedTime;
    private float endlessElapsedTime;

    public float EndlessElapsedTime => endlessElapsedTime;
    private int killCount;
    private bool deathStarted;

    private GUIStyle titleStyle;
    private GUIStyle buttonStyle;
    private GUIStyle textStyle;

    public GameState State => state;
    public float ElapsedTime => elapsedTime;
    public int KillCount => killCount;
    public bool IsEndless => state == GameState.Endless;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (progression == null)
            progression =
                FindFirstObjectByType<PlayerProgression>();

        if (playerHealth == null)
            playerHealth =
                FindFirstObjectByType<PlayerHealth>();

        if (deathVisual == null)
            deathVisual =
                FindFirstObjectByType<PlayerDeathVisual>();

        if (enemySpawner == null)
            enemySpawner =
                FindFirstObjectByType<EnemySpawner>();
    }

    private void OnEnable()
    {
        if (progression != null)
            progression.LevelChanged += OnLevelChanged;

        if (enemySpawner != null)
            enemySpawner.EnemyKilled += OnEnemyKilled;
    }

    private void OnDisable()
    {
        if (progression != null)
            progression.LevelChanged -= OnLevelChanged;

        if (enemySpawner != null)
            enemySpawner.EnemyKilled -= OnEnemyKilled;
    }

    private void OnEnemyKilled(EnemyHealth enemy)
    {
        if (state == GameState.Playing ||
            state == GameState.Endless)
        {
            killCount++;
        }
    }

    private void OnLevelChanged(int level)
    {
        if (state != GameState.Playing || level != 6)
            return;

        if (playerHealth != null && playerHealth.IsDead)
            return;

        EnterVictory();
    }

    private void EnterVictory()
    {
        if (state != GameState.Playing || deathStarted)
            return;

        state = GameState.Victory;
        Time.timeScale = 0f;

        UnityEngine.Debug.Log("Victory! Lv.5 completed.");
    }

    private void Update()
    {
        bool isRunning =
            state == GameState.Playing ||
            state == GameState.Endless;

        if (!isRunning)
            return;

        // Detect death before adding more survival time.
        if (!deathStarted &&
            playerHealth != null &&
            playerHealth.IsDead)
        {
            StartCoroutine(HandlePlayerDeath());
            return;
        }

        elapsedTime += Time.deltaTime;
        if (state == GameState.Endless)
            endlessElapsedTime += Time.deltaTime;
    }

    private IEnumerator HandlePlayerDeath()
    {
        deathStarted = true;

        if (deathVisual != null)
            deathVisual.PlayDeath();

        // Allow the player to fall before pausing.
        yield return new WaitForSecondsRealtime(
            deathAnimationDelay);

        state = GameState.GameOver;
        Time.timeScale = 0f;

        UnityEngine.Debug.Log(
            $"Game Over! Kills: {killCount}, " +
            $"Time: {FormatTime(elapsedTime)}");
    }

    public void StartEndlessMode()
    {
        if (state != GameState.Victory)
            return;

        state = GameState.Endless;
        endlessElapsedTime = 0f;

        progression?.EnableEndlessProgression();

        Time.timeScale = 1f;

        UnityEngine.Debug.Log(
            "Endless Mode started at Lv.6.");
    }

    public void FinishRun()
    {
        if (state != GameState.Victory)
            return;

        state = GameState.Results;
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name);
    }

    private void OnGUI()
    {
        if (!useLegacyUI)
            return;
        if (state != GameState.Victory &&
            state != GameState.Results &&
            state != GameState.GameOver)
            return;

        InitializeStyles();

        float width =
            Mathf.Min(460f, Screen.width - 20f);

        float height = 350f;

        Rect panel = new Rect(
            (Screen.width - width) * 0.5f,
            (Screen.height - height) * 0.5f,
            width,
            height
        );

        GUI.Box(panel, "");

        string title = state switch
        {
            GameState.Victory => "VICTORY!",
            GameState.Results => "RUN COMPLETE",
            GameState.GameOver => "GAME OVER",
            _ => ""
        };

        GUI.Label(
            new Rect(panel.x, panel.y + 18f,
                     panel.width, 55f),
            title,
            titleStyle);

        string stats =
            $"Level: {progression?.Level ?? 0}\n" +
            $"Kills: {killCount}\n" +
            $"Survival: {FormatTime(elapsedTime)}\n" +
            $"Total XP: {progression?.TotalXP ?? 0}";

        GUI.Label(
            new Rect(panel.x + 20f, panel.y + 85f,
                     panel.width - 40f, 95f),
            stats,
            textStyle);

        if (state == GameState.Victory)
        {
            if (GUI.Button(
                new Rect(panel.x + 45f, panel.y + 200f,
                         panel.width - 90f, 50f),
                "CONTINUE - ENDLESS MODE",
                buttonStyle))
            {
                StartEndlessMode();
            }

            if (GUI.Button(
                new Rect(panel.x + 45f, panel.y + 265f,
                         panel.width - 90f, 50f),
                "FINISH RUN",
                buttonStyle))
            {
                FinishRun();
            }
        }
        else
        {
            if (GUI.Button(
                new Rect(panel.x + 45f, panel.y + 235f,
                         panel.width - 90f, 60f),
                "RESTART GAME",
                buttonStyle))
            {
                RestartGame();
            }
        }
    }

    private void InitializeStyles()
    {
        if (titleStyle != null)
            return;

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 30,
            fontStyle = FontStyle.Bold
        };

        textStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 17
        };

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 17
        };
    }

    private static string FormatTime(float seconds)
    {
        int total =
            Mathf.FloorToInt(seconds);

        return $"{total / 60:00}:{total % 60:00}";
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
