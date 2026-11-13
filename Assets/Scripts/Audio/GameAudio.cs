
using System.Diagnostics;
using UnityEngine;

public enum GameSound
{
    Shoot,
    Hit,
    PlayerHurt,
    EnemyDeath,
    LevelUp,
    UpgradeSelect,
    SkillCast,
    Victory,
    GameOver,
    Button,
    NuclearShoot,
    NuclearHit
}

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }

    [Header("Game References (auto-find if empty)")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerProgression progression;
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ActiveSkillController activeSkill;

    [Header("Original SFX - Drag WAV clips here")]
    [SerializeField] private AudioClip shoot;
    [SerializeField] private AudioClip hit;
    [SerializeField] private AudioClip playerHurt;
    [SerializeField] private AudioClip enemyDeath;
    [SerializeField] private AudioClip levelUp;
    [SerializeField] private AudioClip upgradeSelect;
    [SerializeField] private AudioClip skillCast;
    [SerializeField] private AudioClip victory;
    [SerializeField] private AudioClip gameOver;
    [SerializeField] private AudioClip button;

    [Header("Nuclear Damage SFX")]
    [SerializeField] private AudioClip nuclearShoot;
    [SerializeField] private AudioClip nuclearHit;

    [Header("Mix")]
    [SerializeField, Range(0f, 1f)] private float masterVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float gameplayVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float uiVolume = 0.9f;

    [Header("High-frequency sound limits (seconds)")]
    [SerializeField, Min(0f)] private float shootMinInterval = 0.075f;
    [SerializeField, Min(0f)] private float hitMinInterval = 0.055f;
    [SerializeField, Min(0f)] private float deathMinInterval = 0.10f;

    private AudioSource gameplaySource;
    private AudioSource uiSource;

    private float lastShootTime = -100f;
    private float lastHitTime = -100f;
    private float lastDeathTime = -100f;

    private float previousHealth;
    private GameState previousState;
    private bool wasCasting;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            UnityEngine.Debug.LogWarning(
                "Duplicate GameAudio in scene; disabling duplicate.");
            enabled = false;
            return;
        }

        Instance = this;

        gameplaySource = GetComponent<AudioSource>();
        uiSource = gameObject.AddComponent<AudioSource>();

        ConfigureSource(gameplaySource, false);
        ConfigureSource(uiSource, true);

        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (progression == null)
            progression = FindFirstObjectByType<PlayerProgression>();

        if (upgradeManager == null)
            upgradeManager = FindFirstObjectByType<UpgradeManager>();

        if (enemySpawner == null)
            enemySpawner = FindFirstObjectByType<EnemySpawner>();

        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (activeSkill == null)
            activeSkill = FindFirstObjectByType<ActiveSkillController>();

        previousHealth = playerHealth != null
            ? playerHealth.CurrentHealth
            : 0f;

        previousState = gameManager != null
            ? gameManager.State
            : GameState.Playing;

        wasCasting =
            activeSkill != null && activeSkill.IsCasting;
    }

    private static void ConfigureSource(
        AudioSource source, bool isUI)
    {
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.volume = 1f;
        source.ignoreListenerPause = isUI;
    }

    private void OnEnable()
    {
        if (Instance != this)
            return;

        if (playerHealth != null)
            playerHealth.HealthChanged += OnHealthChanged;

        if (progression != null)
            progression.LevelChanged += OnLevelChanged;

        if (upgradeManager != null)
            upgradeManager.UpgradeSelected += OnUpgradeSelected;

        if (enemySpawner != null)
            enemySpawner.EnemyKilled += OnEnemyKilled;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.HealthChanged -= OnHealthChanged;

        if (progression != null)
            progression.LevelChanged -= OnLevelChanged;

        if (upgradeManager != null)
            upgradeManager.UpgradeSelected -= OnUpgradeSelected;

        if (enemySpawner != null)
            enemySpawner.EnemyKilled -= OnEnemyKilled;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnHealthChanged(
        float current, float maximum)
    {
        if (current < previousHealth - 0.01f)
            Play(GameSound.PlayerHurt);

        previousHealth = current;
    }

    private void OnLevelChanged(int level)
    {
        if (level < 6 ||
            (gameManager != null && gameManager.IsEndless))
        {
            Play(GameSound.LevelUp);
        }
    }

    private void OnUpgradeSelected(UpgradeId selected)
    {
        Play(GameSound.UpgradeSelect);
    }

    private void OnEnemyKilled(EnemyHealth enemy)
    {
        Play(GameSound.EnemyDeath);
    }

    private void Update()
    {
        if (activeSkill != null)
        {
            bool casting = activeSkill.IsCasting;

            if (casting && !wasCasting)
                Play(GameSound.SkillCast);

            wasCasting = casting;
        }

        if (gameManager == null)
            return;

        GameState state = gameManager.State;

        if (state == previousState)
            return;

        previousState = state;

        if (state == GameState.Victory)
            Play(GameSound.Victory);
        else if (state == GameState.GameOver)
            Play(GameSound.GameOver);
    }

    public static void Play(GameSound sound)
    {
        if (Instance != null && Instance.isActiveAndEnabled)
            Instance.PlayInternal(sound);
    }

    private void PlayInternal(GameSound sound)
    {
        AudioClip clip = null;
        float level = 1f;
        bool playAsUI = false;

        float now = Time.unscaledTime;

        switch (sound)
        {
            case GameSound.Shoot:
                if (now - lastShootTime < shootMinInterval)
                    return;

                lastShootTime = now;
                clip = shoot;
                level = 0.30f;
                break;

            case GameSound.NuclearShoot:
                if (now - lastShootTime < shootMinInterval)
                    return;

                lastShootTime = now;
                clip = nuclearShoot != null
                    ? nuclearShoot
                    : shoot;
                level = 0.42f;
                break;

            case GameSound.Hit:
                if (now - lastHitTime < hitMinInterval)
                    return;

                lastHitTime = now;
                clip = hit;
                level = 0.24f;
                break;

            case GameSound.NuclearHit:
                if (now - lastHitTime < hitMinInterval)
                    return;

                lastHitTime = now;
                clip = nuclearHit != null
                    ? nuclearHit
                    : hit;
                level = 0.46f;
                break;

            case GameSound.PlayerHurt:
                clip = playerHurt;
                level = 0.65f;
                break;

            case GameSound.EnemyDeath:
                if (now - lastDeathTime < deathMinInterval)
                    return;

                lastDeathTime = now;
                clip = enemyDeath;
                level = 0.28f;
                break;

            case GameSound.LevelUp:
                clip = levelUp;
                level = 0.80f;
                playAsUI = true;
                break;

            case GameSound.UpgradeSelect:
                clip = upgradeSelect;
                level = 0.75f;
                playAsUI = true;
                break;

            case GameSound.SkillCast:
                clip = skillCast;
                level = 0.72f;
                break;

            case GameSound.Victory:
                clip = victory;
                level = 0.95f;
                playAsUI = true;
                break;

            case GameSound.GameOver:
                clip = gameOver;
                level = 0.8f;
                playAsUI = true;
                break;

            case GameSound.Button:
                clip = button;
                level = 0.48f;
                playAsUI = true;
                break;
        }

        if (clip == null)
            return;

        AudioSource source =
            playAsUI ? uiSource : gameplaySource;

        float categoryVolume =
            playAsUI ? uiVolume : gameplayVolume;

        source.PlayOneShot(
            clip,
            Mathf.Clamp01(
                masterVolume * categoryVolume * level));
    }
}
