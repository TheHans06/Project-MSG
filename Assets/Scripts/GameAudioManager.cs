using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameAudioManager : MonoBehaviour
{
    private static GameAudioManager instance;

    [Header("Scene Names")]
    [SerializeField] private string gameplaySceneName = "Main Gameplay";

    [Header("Audio Clips")]
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private AudioClip moneySfxClip;
    [SerializeField] private AudioClip ambientClip;

    [Header("Resources Fallback Paths")]
    [SerializeField] private string bgmResourcePath = "Audio/BGM/main_bgm";
    [SerializeField] private string moneySfxResourcePath = "Audio/SFX/money";
    [SerializeField] private string ambientResourcePath = "Audio/Ambient/main_gameplay_ambient";

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.45f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.9f;
    [SerializeField, Range(0f, 1f)] private float ambientVolume = 0.35f;

    [Header("Debug")]
    [SerializeField] private bool logMissingClips = true;

    private AudioSource bgmSource;
    private AudioSource sfxSource;
    private AudioSource ambientSource;
    private bool missingBgmLogged;
    private bool missingMoneySfxLogged;
    private bool missingAmbientLogged;

    public static GameAudioManager Instance => EnsureInstance();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoStartAfterFirstSceneLoad()
    {
        GameAudioManager manager = EnsureInstance();
        manager.LoadClipsIfNeeded();
        manager.PlayBgm();
        manager.UpdateAmbientForScene(SceneManager.GetActiveScene().name);
    }

    public static void PlayMoneySfx()
    {
        EnsureInstance().PlayMoneySfxInternal();
    }

    private static GameAudioManager EnsureInstance()
    {
        if (instance != null)
        {
            return instance;
        }

        GameAudioManager existingManager = FindAnyObjectByType<GameAudioManager>(FindObjectsInactive.Include);
        if (existingManager != null)
        {
            return existingManager;
        }

        GameObject managerObject = new GameObject("Game Audio Manager");
        return managerObject.AddComponent<GameAudioManager>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureSources();
        LoadClipsIfNeeded();
        ApplyVolumes();
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void Start()
    {
        PlayBgm();
        UpdateAmbientForScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            instance = null;
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadClipsIfNeeded();
        ApplyVolumes();
        PlayBgm();
        UpdateAmbientForScene(scene.name);
    }

    private void EnsureSources()
    {
        bgmSource ??= CreateSource("BGM Source", true);
        sfxSource ??= CreateSource("SFX Source", false);
        ambientSource ??= CreateSource("Ambient Source", true);
    }

    private AudioSource CreateSource(string sourceName, bool loop)
    {
        GameObject sourceObject = new GameObject(sourceName);
        sourceObject.transform.SetParent(transform, false);

        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
        return source;
    }

    private void LoadClipsIfNeeded()
    {
        bgmClip ??= LoadClip(bgmResourcePath);
        moneySfxClip ??= LoadClip(moneySfxResourcePath);
        ambientClip ??= LoadClip(ambientResourcePath);
    }

    private static AudioClip LoadClip(string resourcePath)
    {
        return string.IsNullOrWhiteSpace(resourcePath)
            ? null
            : Resources.Load<AudioClip>(resourcePath);
    }

    private void ApplyVolumes()
    {
        EnsureSources();
        bgmSource.volume = bgmVolume;
        sfxSource.volume = sfxVolume;
        ambientSource.volume = ambientVolume;
    }

    private void PlayBgm()
    {
        EnsureSources();

        if (bgmClip == null)
        {
            LogMissingClipOnce("BGM", bgmResourcePath, ref missingBgmLogged);
            return;
        }

        if (bgmSource.clip != bgmClip)
        {
            bgmSource.clip = bgmClip;
        }

        if (!bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }

    private void PlayMoneySfxInternal()
    {
        EnsureSources();
        LoadClipsIfNeeded();

        if (moneySfxClip == null)
        {
            LogMissingClipOnce("Money SFX", moneySfxResourcePath, ref missingMoneySfxLogged);
            return;
        }

        sfxSource.PlayOneShot(moneySfxClip, sfxVolume);
    }

    private void UpdateAmbientForScene(string sceneName)
    {
        EnsureSources();

        if (sceneName == gameplaySceneName)
        {
            PlayAmbient();
            return;
        }

        if (ambientSource.isPlaying)
        {
            ambientSource.Stop();
        }
    }

    private void PlayAmbient()
    {
        if (ambientClip == null)
        {
            LogMissingClipOnce("Ambient", ambientResourcePath, ref missingAmbientLogged);
            return;
        }

        if (ambientSource.clip != ambientClip)
        {
            ambientSource.clip = ambientClip;
        }

        if (!ambientSource.isPlaying)
        {
            ambientSource.Play();
        }
    }

    private void LogMissingClipOnce(string label, string resourcePath, ref bool alreadyLogged)
    {
        if (!logMissingClips || alreadyLogged)
        {
            return;
        }

        alreadyLogged = true;
        Debug.LogWarning($"[Audio] {label} clip is missing. Assign it on GameAudioManager or place it in Assets/Resources/{resourcePath}.");
    }
}
