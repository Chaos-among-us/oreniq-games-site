using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndlessDodgeAudioDirector : MonoBehaviour
{
    private const string AudioTracePrefix = "[QA-AUDIO]";
    private static readonly int[] MajorScaleSemitones = { 0, 2, 4, 5, 7, 9, 11 };
    private static readonly int[][] BiomeChordProgressions =
    {
        new[] { 0, 4, 5, 3, 0, 4, 3, 5, 0, 4, 5, 3, 5, 4, 3, 4 },
        new[] { 5, 3, 0, 4, 5, 3, 4, 0, 5, 3, 0, 4, 4, 5, 3, 0 },
        new[] { 0, 5, 3, 4, 0, 5, 4, 3, 0, 5, 3, 4, 3, 4, 5, 4 },
        new[] { 3, 0, 4, 5, 3, 0, 5, 4, 3, 0, 4, 5, 5, 4, 3, 0 }
    };
    private static readonly float[] BiomeSongRoots = { 220f, 246.94f, 196f, 261.63f };

    private readonly Dictionary<int, AudioClip> ambientLoops = new Dictionary<int, AudioClip>();
    private readonly Dictionary<string, AudioClip> sfxLibrary = new Dictionary<string, AudioClip>();
    private readonly List<AudioSource> sfxVoices = new List<AudioSource>();
    private readonly Queue<int> pendingBiomeWarmups = new Queue<int>();
    private readonly HashSet<int> queuedBiomeWarmups = new HashSet<int>();

    private GameManager gameManager;
    private AudioSource musicSourceA;
    private AudioSource musicSourceB;
    private int activeMusicSourceIndex;
    private int lastAppliedBiomeIndex = -1;
    private RuntimeCaveTheme currentTheme;
    private Coroutine musicFadeRoutine;
    private Coroutine biomeWarmupRoutine;
    private Coroutine audioRestoreRoutine;
    private Coroutine qaPromptAudioRecoveryRoutine;
    private int sfxVoiceIndex;
    private bool initialMusicPrepared;

    private const int SampleRate = 22050;
    private const float AmbientLoopLength = 32f;
    private const float BaseMusicVolume = 0.76f;
    private const float PeakMusicVolume = 0.86f;
    private const float MenuMusicVolume = 0.66f;

    public static EndlessDodgeAudioDirector Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureExistsOnBoot()
    {
        EnsureExists();
    }

    public static EndlessDodgeAudioDirector EnsureExists()
    {
        if (Instance != null)
            return Instance;

        EndlessDodgeAudioDirector existing = FindAnyObjectByType<EndlessDodgeAudioDirector>();

        if (existing != null)
            return existing;

        GameObject audioRoot = new GameObject("EndlessDodgeAudioDirector");
        return audioRoot.AddComponent<EndlessDodgeAudioDirector>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        AndroidVolumeControlHelper.BindHardwareVolumeKeysToMusic();
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        gameManager = GameManager.instance;
        EnsureAudioSources();

        if (sfxLibrary.Count == 0)
            BuildSfxLibrary();

        LogAudioState("Awake completed");
    }

    void Start()
    {
        AndroidVolumeControlHelper.BindHardwareVolumeKeysToMusic();
        RefreshSceneAudio(immediate: true);
        LogAudioState("Start refreshed scene audio");
    }

    void Update()
    {
        if (gameManager == null)
            gameManager = GameManager.instance;

        RefreshSceneAudio(immediate: false);
        UpdateMusicMix();
    }

    void OnApplicationPause(bool paused)
    {
        LogAudioState("OnApplicationPause paused=" + paused);
        HandleApplicationFocusChanged(!paused);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        LogAudioState("OnApplicationFocus hasFocus=" + hasFocus);
        HandleApplicationFocusChanged(hasFocus);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            if (audioRestoreRoutine != null)
                StopCoroutine(audioRestoreRoutine);

            if (qaPromptAudioRecoveryRoutine != null)
                StopCoroutine(qaPromptAudioRecoveryRoutine);

            LogAudioState("OnDestroy");
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            Instance = null;
        }
    }

    public static void RecoverAudioAfterQaPermissionFlow()
    {
        EndlessDodgeAudioDirector director = EnsureExists();

        if (director != null)
        {
            director.LogAudioState("RecoverAudioAfterQaPermissionFlow requested");
            director.BeginQaPromptAudioRecovery();
        }
    }

    public void PlayCoinPickup()
    {
        PlaySfx("coin", 0.45f, Random.Range(0.97f, 1.08f));
    }

    public void PlayObstacleHit()
    {
        PlaySfx("hit", 0.82f, 1f);
    }

    public void PlayShieldActivated()
    {
        PlaySfx("shield_on", 0.62f, 1f);
    }

    public void PlayShieldBlocked()
    {
        PlaySfx("shield_block", 0.78f, 1f);
    }

    public void PlayExtraLifeArmed()
    {
        PlaySfx("extra_life_arm", 0.68f, 1f);
    }

    public void PlayExtraLifeRevive()
    {
        PlaySfx("extra_life_revive", 0.76f, 1f);
    }

    public void PlayBombBlast()
    {
        PlaySfx("bomb", 0.9f, 1f);
    }

    public void PlayLevelAdvanceCue()
    {
        PlaySfx("level_up", 0.56f, 1f);
    }

    public void PlayUpgradeActivated(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.SpeedBoost:
                PlaySfx("speed", 0.62f, 1f);
                break;
            case UpgradeType.CoinMagnet:
                PlaySfx("magnet", 0.66f, 1f);
                break;
            case UpgradeType.DoubleCoins:
                PlaySfx("double_coins", 0.64f, 1f);
                break;
            case UpgradeType.SlowTime:
                PlaySfx("slow_time", 0.7f, 1f);
                break;
            case UpgradeType.SmallerPlayer:
                PlaySfx("smaller", 0.54f, 1f);
                break;
            case UpgradeType.ScoreBooster:
                PlaySfx("score", 0.6f, 1f);
                break;
            case UpgradeType.RareCoinBoost:
                PlaySfx("rare", 0.62f, 1f);
                break;
            default:
                PlaySfx("upgrade_generic", 0.56f, 1f);
                break;
        }
    }

    private void EnsureAudioSources()
    {
        musicSourceA = CreateOrFindAudioSource("MusicSourceA", loop: true, volume: BaseMusicVolume);
        musicSourceB = CreateOrFindAudioSource("MusicSourceB", loop: true, volume: 0f);

        if (sfxVoices.Count == 0)
        {
            for (int i = 0; i < 4; i++)
            {
                AudioSource voice = CreateOrFindAudioSource("SfxVoice" + i, loop: false, volume: 0.68f);
                voice.playOnAwake = false;
                sfxVoices.Add(voice);
            }
        }
    }

    private AudioSource CreateOrFindAudioSource(string objectName, bool loop, float volume)
    {
        Transform existing = transform.Find(objectName);
        GameObject audioObject;

        if (existing != null)
        {
            audioObject = existing.gameObject;
        }
        else
        {
            audioObject = new GameObject(objectName);
            audioObject.transform.SetParent(transform, false);
        }

        AudioSource source = audioObject.GetComponent<AudioSource>();

        if (source == null)
            source = audioObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
        source.ignoreListenerPause = true;
        source.priority = loop ? 24 : 48;
        source.volume = volume;
        return source;
    }

    private void ApplyTheme(RuntimeCaveTheme theme, bool immediate)
    {
        currentTheme = theme;
        lastAppliedBiomeIndex = theme.BiomeIndex;
        AudioClip loop = GetAmbientLoop(theme);

        if (immediate)
        {
            AudioSource activeSource = GetActiveMusicSource();
            activeSource.clip = loop;
            activeSource.volume = GetTargetMusicVolume();

            if (!activeSource.isPlaying)
                activeSource.Play();

            GetInactiveMusicSource().Stop();
            return;
        }

        if (musicFadeRoutine != null)
            StopCoroutine(musicFadeRoutine);

        musicFadeRoutine = StartCoroutine(CrossfadeMusic(loop));
    }

    private IEnumerator CrossfadeMusic(AudioClip nextLoop)
    {
        AudioSource fromSource = GetActiveMusicSource();
        AudioSource toSource = GetInactiveMusicSource();

        toSource.clip = nextLoop;
        toSource.volume = 0f;
        toSource.Play();

        const float fadeDuration = 2.1f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float targetVolume = GetTargetMusicVolume();
            fromSource.volume = Mathf.Lerp(targetVolume, 0f, t);
            toSource.volume = Mathf.Lerp(0f, targetVolume, t);
            yield return null;
        }

        fromSource.Stop();
        fromSource.volume = 0f;
        toSource.volume = GetTargetMusicVolume();
        activeMusicSourceIndex = activeMusicSourceIndex == 0 ? 1 : 0;
        musicFadeRoutine = null;
    }

    private AudioClip GetAmbientLoop(RuntimeCaveTheme theme)
    {
        if (ambientLoops.TryGetValue(theme.BiomeIndex, out AudioClip cachedClip) && cachedClip != null)
            return cachedClip;

        AudioClip clip = BuildAmbientLoop(theme);
        ambientLoops[theme.BiomeIndex] = clip;
        return clip;
    }

    private void QueueRemainingBiomeWarmups(int activeBiomeIndex)
    {
        for (int biomeIndex = 0; biomeIndex < BiomeSongRoots.Length; biomeIndex++)
        {
            if (biomeIndex == activeBiomeIndex || ambientLoops.ContainsKey(biomeIndex) || queuedBiomeWarmups.Contains(biomeIndex))
                continue;

            queuedBiomeWarmups.Add(biomeIndex);
            pendingBiomeWarmups.Enqueue(biomeIndex);
        }

        if (biomeWarmupRoutine == null && pendingBiomeWarmups.Count > 0)
            biomeWarmupRoutine = StartCoroutine(PrewarmQueuedBiomes());
    }

    private IEnumerator PrewarmQueuedBiomes()
    {
        while (pendingBiomeWarmups.Count > 0)
        {
            int biomeIndex = pendingBiomeWarmups.Dequeue();
            queuedBiomeWarmups.Remove(biomeIndex);
            WarmBiomeImmediate(biomeIndex);
            yield return null;
        }

        biomeWarmupRoutine = null;
    }

    private void WarmBiomeImmediate(int biomeIndex)
    {
        if (ambientLoops.ContainsKey(biomeIndex))
            return;

        RuntimeCaveTheme representativeTheme = CaveThemeLibrary.GetThemeForLevel((biomeIndex * CaveThemeLibrary.LevelsPerBiome) + 1);
        ambientLoops[biomeIndex] = BuildAmbientLoop(representativeTheme);
    }

    private void BuildSfxLibrary()
    {
        sfxLibrary["coin"] = BuildCoinClip();
        sfxLibrary["hit"] = BuildHitClip();
        sfxLibrary["shield_on"] = BuildShieldActivateClip();
        sfxLibrary["shield_block"] = BuildShieldBlockClip();
        sfxLibrary["extra_life_arm"] = BuildExtraLifeArmClip();
        sfxLibrary["extra_life_revive"] = BuildExtraLifeReviveClip();
        sfxLibrary["bomb"] = BuildBombClip();
        sfxLibrary["speed"] = BuildSpeedClip();
        sfxLibrary["magnet"] = BuildMagnetClip();
        sfxLibrary["double_coins"] = BuildDoubleCoinsClip();
        sfxLibrary["slow_time"] = BuildSlowTimeClip();
        sfxLibrary["smaller"] = BuildSmallerClip();
        sfxLibrary["score"] = BuildScoreClip();
        sfxLibrary["rare"] = BuildRareClip();
        sfxLibrary["level_up"] = BuildLevelUpClip();
        sfxLibrary["upgrade_generic"] = BuildGenericUpgradeClip();
    }

    private void PlaySfx(string key, float volume, float pitch)
    {
        if (!sfxLibrary.TryGetValue(key, out AudioClip clip) || clip == null || sfxVoices.Count == 0)
            return;

        EnsureMobileAudioOutputStarted();
        AudioSource voice = sfxVoices[sfxVoiceIndex];
        sfxVoiceIndex = (sfxVoiceIndex + 1) % sfxVoices.Count;

        voice.Stop();
        voice.clip = clip;
        voice.pitch = pitch;
        voice.volume = volume;
        voice.Play();
    }

    private AudioSource GetActiveMusicSource()
    {
        return activeMusicSourceIndex == 0 ? musicSourceA : musicSourceB;
    }

    private AudioSource GetInactiveMusicSource()
    {
        return activeMusicSourceIndex == 0 ? musicSourceB : musicSourceA;
    }

    private int GetCurrentLevel()
    {
        if (gameManager == null)
            return 1;

        return Mathf.Max(1, gameManager.GetDifficultyLevel());
    }

    private AudioClip BuildAmbientLoop(RuntimeCaveTheme theme)
    {
        int sampleCount = Mathf.RoundToInt(SampleRate * AmbientLoopLength);
        float[] data = new float[sampleCount];
        float root = GetSongRootFrequency(theme.BiomeIndex);
        int[] progression = GetChordProgression(theme.BiomeIndex);

        AddBassGroove(data, root, progression, 0.24f);
        AddPadProgression(data, root, progression, 0.17f);
        AddTensionOstinato(data, root, progression, theme.BiomeIndex, 0.075f);
        AddAccentStabs(data, root, progression, theme.BiomeIndex, 0.094f);
        AddResponseLine(data, root, progression, theme.BiomeIndex, 0.055f);
        AddKickPattern(data, progression.Length, root * 0.5f, 0.22f);
        AddSnarePattern(data, progression.Length, 0.09f);
        AddHiHatPattern(data, progression.Length, 0.026f);
        ApplyLoopEdgeFade(data, 0.14f);
        Normalize(data, 0.96f);

        AudioClip clip = AudioClip.Create(
            "AmbientLoop_Level" + theme.Level,
            sampleCount,
            1,
            SampleRate,
            false);
        clip.SetData(data, 0);
        return clip;
    }

    private int[] GetChordProgression(int biomeIndex)
    {
        return BiomeChordProgressions[Mathf.Abs(biomeIndex) % BiomeChordProgressions.Length];
    }

    private float GetSongRootFrequency(int biomeIndex)
    {
        return QuantizeFrequency(BiomeSongRoots[Mathf.Abs(biomeIndex) % BiomeSongRoots.Length], AmbientLoopLength);
    }

    private void AddPadProgression(float[] data, float rootFrequency, int[] progression, float amplitude)
    {
        float barDuration = AmbientLoopLength / progression.Length;

        for (int bar = 0; bar < progression.Length; bar++)
        {
            float startTime = bar * barDuration;
            float noteDuration = barDuration * 0.94f;
            int degree = progression[bar];
            float sectionLift = GetSectionLift(bar, progression.Length);

            AddSynthNote(data, GetScaleFrequency(rootFrequency, degree, 0), amplitude * (0.78f + sectionLift * 0.2f), startTime, noteDuration, 0.22f, 0.7f, 0.0008f, 0.45f, 0.06f, 0f);
            AddSynthNote(data, GetScaleFrequency(rootFrequency, degree + 2, 0), amplitude * (0.54f + sectionLift * 0.16f), startTime, noteDuration, 0.26f, 0.74f, 0.0007f, 0.4f, 0.05f, 0f);
            AddSynthNote(data, GetScaleFrequency(rootFrequency, degree + 4, 0), amplitude * (0.44f + sectionLift * 0.14f), startTime, noteDuration, 0.3f, 0.78f, 0.0006f, 0.35f, 0.04f, 0f);
            AddSynthNote(data, GetScaleFrequency(rootFrequency, degree + 7, 0), amplitude * (0.12f + sectionLift * 0.08f), startTime, noteDuration, 0.2f, 0.64f, 0.0006f, 0.4f, 0.03f, 0f);
        }
    }

    private void AddBassGroove(float[] data, float rootFrequency, int[] progression, float amplitude)
    {
        float barDuration = AmbientLoopLength / progression.Length;
        float beatDuration = barDuration / 4f;

        for (int bar = 0; bar < progression.Length; bar++)
        {
            int degree = progression[bar];
            float barStart = bar * barDuration;
            float sectionEnergy = GetSectionEnergy(bar, progression.Length);
            int passingDegree = bar >= progression.Length - 4 ? degree + 6 : degree + 4;

            AddSynthNote(data, GetScaleFrequency(rootFrequency * 0.5f, degree, 0), amplitude * sectionEnergy, barStart, beatDuration * 0.92f, 0.005f, 0.14f, 0.0005f, 0.2f, 0.03f, 0f);
            AddSynthNote(data, GetScaleFrequency(rootFrequency * 0.5f, degree, 0), amplitude * sectionEnergy * 0.84f, barStart + beatDuration * 2f, beatDuration * 0.78f, 0.005f, 0.12f, 0.0005f, 0.2f, 0.03f, 0f);
            AddSynthNote(data, GetScaleFrequency(rootFrequency * 0.5f, passingDegree, 0), amplitude * sectionEnergy * 0.5f, barStart + beatDuration * 3.2f, beatDuration * 0.5f, 0.005f, 0.1f, 0.0005f, 0.2f, 0.025f, 0f);
        }
    }

    private void AddTensionOstinato(float[] data, float rootFrequency, int[] progression, int biomeIndex, float amplitude)
    {
        float barDuration = AmbientLoopLength / progression.Length;
        float stepDuration = barDuration / 8f;
        int[][] patterns =
        {
            new[] { 0, 2, 4, 2, 0, 2, 4, 2 },
            new[] { 0, 2, 4, 6, 4, 2, 4, 2 },
            new[] { 0, 4, 2, 4, 0, 4, 2, 6 },
            new[] { 0, 2, 4, 2, 4, 2, 0, 2 }
        };

        int[] pattern = patterns[Mathf.Abs(biomeIndex) % patterns.Length];

        for (int bar = 0; bar < progression.Length; bar++)
        {
            int degree = progression[bar];
            float barStart = bar * barDuration;
            float sectionEnergy = GetSectionEnergy(bar, progression.Length);

            if (bar < 2)
                continue;

            for (int step = 0; step < pattern.Length; step++)
            {
                float stepStart = barStart + step * stepDuration;
                float noteDuration = stepDuration * 0.5f;
                float velocity = step % 4 == 0 ? 1f : (step % 2 == 0 ? 0.76f : 0.58f);
                int noteDegree = degree + pattern[step];
                AddSynthNote(data, GetScaleFrequency(rootFrequency, noteDegree, 0), amplitude * sectionEnergy * velocity, stepStart, noteDuration, 0.008f, 0.08f, 0.0008f, 0.3f, 0.06f, 0f);
            }
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        gameManager = GameManager.instance;
        AndroidVolumeControlHelper.BindHardwareVolumeKeysToMusic();
        RefreshSceneAudio(immediate: true);
        LogAudioState("HandleSceneLoaded scene=" + scene.name + " mode=" + mode);
    }

    private void HandleApplicationFocusChanged(bool hasFocus)
    {
        LogAudioState("HandleApplicationFocusChanged hasFocus=" + hasFocus);

        if (!hasFocus)
        {
            if (audioRestoreRoutine != null)
            {
                StopCoroutine(audioRestoreRoutine);
                audioRestoreRoutine = null;
            }

            return;
        }

        if (audioRestoreRoutine != null)
            StopCoroutine(audioRestoreRoutine);

        AndroidVolumeControlHelper.BindHardwareVolumeKeysToMusic();
        audioRestoreRoutine = StartCoroutine(RestoreAudioAfterFocusCoroutine());
    }

    private void BeginQaPromptAudioRecovery()
    {
        if (audioRestoreRoutine != null)
        {
            StopCoroutine(audioRestoreRoutine);
            audioRestoreRoutine = null;
        }

        if (qaPromptAudioRecoveryRoutine != null)
            StopCoroutine(qaPromptAudioRecoveryRoutine);

        LogAudioState("BeginQaPromptAudioRecovery");
        qaPromptAudioRecoveryRoutine = StartCoroutine(RestoreAudioAfterQaPermissionCoroutine());
    }

    private void RefreshSceneAudio(bool immediate)
    {
        RuntimeCaveTheme theme = ResolveThemeForCurrentScene();

        if (!initialMusicPrepared)
        {
            WarmBiomeImmediate(theme.BiomeIndex);
            QueueRemainingBiomeWarmups(theme.BiomeIndex);
            ApplyTheme(theme, immediate: true);
            initialMusicPrepared = true;
            return;
        }

        if (theme.BiomeIndex != lastAppliedBiomeIndex)
        {
            ApplyTheme(theme, immediate);
        }
        else if (!HasActiveMusicClip())
        {
            ApplyTheme(theme, immediate: true);
        }
        else
        {
            ResumeMusicIfNeeded();
        }
    }

    private RuntimeCaveTheme ResolveThemeForCurrentScene()
    {
        if (IsGameplaySceneActive())
            return CaveThemeLibrary.GetThemeForLevel(GetCurrentLevel());

        return CaveThemeLibrary.GetMenuTheme();
    }

    private bool IsGameplaySceneActive()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        return activeScene.IsValid() && string.Equals(activeScene.name, "Game");
    }

    private bool HasActiveMusicClip()
    {
        AudioSource activeSource = GetActiveMusicSource();
        return activeSource != null && activeSource.clip != null;
    }

    private void ResumeMusicIfNeeded()
    {
        EnsureMobileAudioOutputStarted();
        AudioSource activeSource = GetActiveMusicSource();

        if (activeSource == null || activeSource.clip == null)
            return;

        if (!activeSource.isPlaying)
            activeSource.Play();

        AudioSource inactiveSource = GetInactiveMusicSource();

        if (inactiveSource != null && inactiveSource.clip == null && inactiveSource.isPlaying)
            inactiveSource.Stop();
    }

    private bool IsActiveMusicPlaying()
    {
        AudioSource activeSource = GetActiveMusicSource();
        return activeSource != null && activeSource.clip != null && activeSource.isPlaying;
    }

    private void StopMusicFadeRoutineIfNeeded()
    {
        if (musicFadeRoutine == null)
            return;

        StopCoroutine(musicFadeRoutine);
        musicFadeRoutine = null;
    }

    private void RestartMusicOnSource(AudioSource source, AudioClip loop, bool promoteToActive)
    {
        if (source == null || loop == null)
            return;

        StopMusicFadeRoutineIfNeeded();
        source.Stop();
        source.clip = loop;
        source.loop = true;
        source.pitch = 1f;
        source.volume = GetTargetMusicVolume();
        source.timeSamples = 0;
        source.Play();

        if (promoteToActive)
            activeMusicSourceIndex = source == musicSourceA ? 0 : 1;
    }

    private void ForceRestartMusicLoop(string reason)
    {
        EnsureMobileAudioOutputStarted();
        RuntimeCaveTheme theme = ResolveThemeForCurrentScene();
        currentTheme = theme;
        lastAppliedBiomeIndex = theme.BiomeIndex;
        AudioClip loop = GetAmbientLoop(theme);
        AudioSource activeSource = GetActiveMusicSource();
        AudioSource inactiveSource = GetInactiveMusicSource();

        if (inactiveSource != null)
        {
            inactiveSource.Stop();
            inactiveSource.volume = 0f;
        }

        RestartMusicOnSource(activeSource, loop, promoteToActive: false);

        if (IsActiveMusicPlaying())
        {
            LogAudioState("ForceRestartMusicLoop current-source reason=" + reason);
            return;
        }

        RestartMusicOnSource(inactiveSource, loop, promoteToActive: true);
        LogAudioState("ForceRestartMusicLoop fallback-source reason=" + reason);
    }

    private IEnumerator RestoreAudioAfterFocusCoroutine()
    {
        yield return null;

        for (int attempt = 0; attempt < 2; attempt++)
        {
            LogAudioState("RestoreAudioAfterFocus attempt=" + attempt + " before recovery");
            ApplyImmediateAudioRecovery(immediate: true, forceMusicRestart: attempt > 0);

            double previousDspTime = AudioSettings.dspTime;
            yield return new WaitForSecondsRealtime(0.12f);

            if (HasRecoveredAudio(previousDspTime))
            {
                LogAudioState("RestoreAudioAfterFocus success attempt=" + attempt);
                audioRestoreRoutine = null;
                yield break;
            }

            LogAudioState("RestoreAudioAfterFocus stalled attempt=" + attempt + ", resetting audio system");
            ForceAudioSystemReset();
            double resetDspTime = AudioSettings.dspTime;
            yield return new WaitForSecondsRealtime(0.15f);

            if (HasRecoveredAudio(resetDspTime))
            {
                LogAudioState("RestoreAudioAfterFocus recovered after reset attempt=" + attempt);
                audioRestoreRoutine = null;
                yield break;
            }
        }

        ApplyImmediateAudioRecovery(immediate: true);
        LogAudioState("RestoreAudioAfterFocus final recovery");
        audioRestoreRoutine = null;
    }

    private IEnumerator RestoreAudioAfterQaPermissionCoroutine()
    {
        float[] retryDelays = { 0.05f, 0.18f, 0.4f, 0.8f };

        for (int attempt = 0; attempt < retryDelays.Length; attempt++)
        {
            if (retryDelays[attempt] > 0f)
                yield return new WaitForSecondsRealtime(retryDelays[attempt]);

            LogAudioState("RestoreAudioAfterQaPermission attempt=" + attempt + " delay=" + retryDelays[attempt]);
            ApplyImmediateAudioRecovery(immediate: true, forceMusicRestart: true);

            double previousDspTime = AudioSettings.dspTime;
            yield return new WaitForSecondsRealtime(0.12f);

            if (HasRecoveredAudio(previousDspTime))
            {
                LogAudioState("RestoreAudioAfterQaPermission success attempt=" + attempt);
                qaPromptAudioRecoveryRoutine = null;
                yield break;
            }

            LogAudioState("RestoreAudioAfterQaPermission stalled attempt=" + attempt + ", resetting audio system");
            ForceAudioSystemReset();
            double resetDspTime = AudioSettings.dspTime;
            yield return new WaitForSecondsRealtime(0.15f);

            if (HasRecoveredAudio(resetDspTime))
            {
                LogAudioState("RestoreAudioAfterQaPermission recovered after reset attempt=" + attempt);
                qaPromptAudioRecoveryRoutine = null;
                yield break;
            }
        }

        ApplyImmediateAudioRecovery(immediate: true);
        LogAudioState("RestoreAudioAfterQaPermission final recovery");
        qaPromptAudioRecoveryRoutine = null;
    }

    private void ApplyImmediateAudioRecovery(bool immediate, bool forceMusicRestart = false)
    {
        AndroidVolumeControlHelper.BindHardwareVolumeKeysToMusic();
        EnsureMobileAudioOutputStarted();
        AudioListener.pause = false;
        AudioListener.volume = 1f;
        EnsureAudioSources();
        RefreshSceneAudio(immediate);

        if (forceMusicRestart && !IsActiveMusicPlaying())
            ForceRestartMusicLoop("ApplyImmediateAudioRecovery");
        else
            ResumeMusicIfNeeded();

        LogAudioState("ApplyImmediateAudioRecovery immediate=" + immediate + " forceMusicRestart=" + forceMusicRestart);
    }

    private bool IsAudioTimelineAdvancing(double previousDspTime)
    {
        return AudioSettings.dspTime > previousDspTime + 0.01d;
    }

    private bool HasRecoveredAudio(double previousDspTime)
    {
        return IsAudioTimelineAdvancing(previousDspTime) && IsActiveMusicPlaying();
    }

    private void ForceAudioSystemReset()
    {
        LogAudioState("ForceAudioSystemReset begin");

        if (musicFadeRoutine != null)
        {
            StopCoroutine(musicFadeRoutine);
            musicFadeRoutine = null;
        }

        if (biomeWarmupRoutine != null)
        {
            StopCoroutine(biomeWarmupRoutine);
            biomeWarmupRoutine = null;
        }

        pendingBiomeWarmups.Clear();
        queuedBiomeWarmups.Clear();
        ambientLoops.Clear();
        sfxLibrary.Clear();
        initialMusicPrepared = false;

        if (musicSourceA != null)
        {
            musicSourceA.Stop();
            musicSourceA.clip = null;
        }

        if (musicSourceB != null)
        {
            musicSourceB.Stop();
            musicSourceB.clip = null;
        }

        for (int i = 0; i < sfxVoices.Count; i++)
        {
            AudioSource voice = sfxVoices[i];

            if (voice == null)
                continue;

            voice.Stop();
            voice.clip = null;
        }

        AudioSettings.Reset(AudioSettings.GetConfiguration());
        BuildSfxLibrary();
        ApplyImmediateAudioRecovery(immediate: true);
        LogAudioState("ForceAudioSystemReset complete");
    }

    private void EnsureMobileAudioOutputStarted()
    {
#if UNITY_ANDROID || UNITY_IOS
        AudioSettings.Mobile.StartAudioOutput();
#endif
    }

    private void LogAudioState(string reason)
    {
        Debug.Log(
            AudioTracePrefix +
            " " +
            reason +
            " | scene=" +
            GetActiveSceneName() +
            " | dsp=" +
            AudioSettings.dspTime.ToString("F3") +
            " | outputStarted=" +
            IsMobileAudioOutputStarted() +
            " | listenerPause=" +
            AudioListener.pause +
            " | listenerVolume=" +
            AudioListener.volume.ToString("F2") +
            " | active=" +
            DescribeAudioSource(GetActiveMusicSource()) +
            " | inactive=" +
            DescribeAudioSource(GetInactiveMusicSource()));
    }

    private static string GetActiveSceneName()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        return activeScene.IsValid() ? activeScene.name : "<invalid>";
    }

    private static string DescribeAudioSource(AudioSource source)
    {
        if (source == null)
            return "<null>";

        string clipName = source.clip != null ? source.clip.name : "<none>";
        return
            clipName +
            ",playing=" +
            source.isPlaying +
            ",volume=" +
            source.volume.ToString("F2");
    }

    private static bool IsMobileAudioOutputStarted()
    {
#if UNITY_ANDROID || UNITY_IOS
        return AudioSettings.Mobile.audioOutputStarted;
#else
        return true;
#endif
    }

    private void AddAccentStabs(float[] data, float rootFrequency, int[] progression, int biomeIndex, float amplitude)
    {
        float sectionDuration = AmbientLoopLength / 4f;
        float[] teaserTimes = { 4.4f, 5.3f, 6.2f };
        int[] teaserPhrase = { 2, 4, 5 };
        float[] phraseTimes = { 0.78f, 1.54f, 2.26f, 3.0f, 3.88f, 4.7f, 5.5f };
        int[] phraseA = { 4, 4, 5, 4, 2, 4, 7 };
        int[] phraseB = { 7, 5, 4, 2, 4, 5, 9 };
        int[] phraseC = { 4, 5, 7, 9, 7, 5, 4 };
        int[] turnaroundPhrase = { 9, 7, 5, 4, 2, 4, 0 };

        AddHookPhrase(data, rootFrequency, 0f, teaserTimes, teaserPhrase, amplitude * 0.42f);
        AddHookPhrase(data, rootFrequency, sectionDuration, phraseTimes, phraseA, amplitude * 0.94f);
        AddHookPhrase(data, rootFrequency, sectionDuration * 2f, phraseTimes, phraseB, amplitude);
        AddHookPhrase(data, rootFrequency, sectionDuration * 3f, phraseTimes, phraseC, amplitude * 1.03f);
        AddHookPhrase(data, rootFrequency, sectionDuration * 3f + 0.2f, new[] { 5.95f, 6.35f, 6.75f, 7.15f, 7.52f, 7.88f, 8.2f }, turnaroundPhrase, amplitude * 0.52f);
    }

    private void AddHookPhrase(float[] data, float rootFrequency, float sectionStart, float[] noteTimes, int[] scaleDegrees, float amplitude)
    {
        for (int i = 0; i < Mathf.Min(noteTimes.Length, scaleDegrees.Length); i++)
        {
            AddSynthNote(
                data,
                GetScaleFrequency(rootFrequency, scaleDegrees[i], 0),
                amplitude * (i == scaleDegrees.Length - 1 ? 0.9f : 1f),
                sectionStart + noteTimes[i],
                0.34f,
                0.02f,
                0.18f,
                0.0007f,
                0.35f,
                0.05f,
                0f);
        }
    }

    private void AddResponseLine(float[] data, float rootFrequency, int[] progression, int biomeIndex, float amplitude)
    {
        float sectionDuration = AmbientLoopLength / 4f;
        float[] responseTimes = { 1.18f, 2.02f, 2.86f, 3.78f, 4.64f, 5.48f };
        int[][] phrases =
        {
            new[] { 9, 7, 5, 7, 9, 7 },
            new[] { 7, 5, 4, 5, 7, 9 },
            new[] { 9, 11, 9, 7, 5, 4 },
            new[] { 7, 9, 11, 9, 7, 5 }
        };

        int[] phrase = phrases[Mathf.Abs(biomeIndex) % phrases.Length];
        AddHookPhrase(data, rootFrequency, sectionDuration * 2f, responseTimes, phrase, amplitude * 0.85f);
        AddHookPhrase(data, rootFrequency, sectionDuration * 3f, responseTimes, phrase, amplitude);
    }

    private void AddKickPattern(float[] data, int barCount, float frequency, float amplitude)
    {
        float barDuration = AmbientLoopLength / barCount;
        float beatDuration = barDuration / 4f;

        for (int bar = 0; bar < barCount; bar++)
        {
            float barStart = bar * barDuration;
            int section = GetSectionIndex(bar, barCount);

            AddKickHit(data, barStart, frequency, amplitude);

            if (section == 0)
            {
                AddKickHit(data, barStart + beatDuration * 2f, frequency, amplitude * 0.72f);
            }
            else if (section == 1)
            {
                AddKickHit(data, barStart + beatDuration * 2f, frequency, amplitude * 0.88f);

                if (bar % 2 == 1)
                    AddKickHit(data, barStart + beatDuration * 3.45f, frequency, amplitude * 0.44f);
            }
            else if (section == 2)
            {
                AddKickHit(data, barStart + beatDuration * 1.5f, frequency, amplitude * 0.48f);
                AddKickHit(data, barStart + beatDuration * 2.95f, frequency, amplitude * 0.84f);
            }
            else
            {
                AddKickHit(data, barStart + beatDuration * 2f, frequency, amplitude * 0.9f);
                AddKickHit(data, barStart + beatDuration * 3.25f, frequency, amplitude * 0.42f);
                AddKickHit(data, barStart + beatDuration * 3.7f, frequency, amplitude * 0.34f);
            }
        }
    }

    private void AddSnarePattern(float[] data, int barCount, float amplitude)
    {
        float barDuration = AmbientLoopLength / barCount;
        float beatDuration = barDuration / 4f;

        for (int bar = 0; bar < barCount; bar++)
        {
            float barStart = bar * barDuration;
            float sectionScale = GetSectionIndex(bar, barCount) == 0 ? 0.72f : 1f;
            AddNoiseHit(data, barStart + beatDuration, beatDuration * 0.26f, amplitude * sectionScale, 1900f, 0.08f);
            AddNoiseHit(data, barStart + beatDuration * 3f, beatDuration * 0.28f, amplitude * 1.06f * sectionScale, 2050f, 0.09f);

            if (bar == barCount - 1)
            {
                AddNoiseHit(data, barStart + beatDuration * 3.35f, beatDuration * 0.16f, amplitude * 0.58f, 2400f, 0.1f);
                AddNoiseHit(data, barStart + beatDuration * 3.7f, beatDuration * 0.14f, amplitude * 0.52f, 2650f, 0.1f);
            }
        }
    }

    private void AddHiHatPattern(float[] data, int barCount, float amplitude)
    {
        float barDuration = AmbientLoopLength / barCount;
        float stepDuration = barDuration / 8f;

        for (int bar = 0; bar < barCount; bar++)
        {
            float barStart = bar * barDuration;
            int section = GetSectionIndex(bar, barCount);

            for (int step = 0; step < 8; step++)
            {
                if (section == 0 && step % 2 == 0)
                    continue;

                float velocity = step % 2 == 0 ? 0.55f : 0.8f;

                if (section == 2)
                    velocity *= 0.88f;
                else if (section == 3)
                    velocity *= 1.08f;

                AddNoiseHit(data, barStart + step * stepDuration, stepDuration * 0.12f, amplitude * velocity, 4300f, 0.025f);
            }
        }
    }

    private float GetSectionLift(int barIndex, int barCount)
    {
        switch (GetSectionIndex(barIndex, barCount))
        {
            case 0:
                return 0.1f;
            case 1:
                return 0.42f;
            case 2:
                return 0.3f;
            default:
                return 0.56f;
        }
    }

    private float GetSectionEnergy(int barIndex, int barCount)
    {
        switch (GetSectionIndex(barIndex, barCount))
        {
            case 0:
                return 0.78f;
            case 1:
                return 1f;
            case 2:
                return 0.9f;
            default:
                return 1.08f;
        }
    }

    private int GetSectionIndex(int barIndex, int barCount)
    {
        int sectionLength = Mathf.Max(1, barCount / 4);
        return Mathf.Clamp(barIndex / sectionLength, 0, 3);
    }

    private void AddKickHit(float[] data, float startTime, float baseFrequency, float amplitude)
    {
        float duration = 0.22f;
        int startSample = Mathf.Clamp(Mathf.RoundToInt(startTime * SampleRate), 0, data.Length - 1);
        int endSample = Mathf.Clamp(Mathf.RoundToInt((startTime + duration) * SampleRate), startSample + 1, data.Length);

        for (int i = startSample; i < endSample; i++)
        {
            float localTime = (i - startSample) / (float)SampleRate;
            float normalized = Mathf.Clamp01(localTime / duration);
            float envelope = Mathf.Exp(-localTime * 14f);
            float frequency = Mathf.Lerp(baseFrequency * 2.3f, baseFrequency, normalized * normalized);
            float tone = Mathf.Sin(localTime * frequency * Mathf.PI * 2f);
            float click = Mathf.Sin(localTime * frequency * 4.5f * Mathf.PI * 2f) * Mathf.Exp(-localTime * 42f) * 0.24f;
            data[i] += (tone + click) * amplitude * envelope;
        }
    }

    private void AddNoiseHit(float[] data, float startTime, float duration, float amplitude, float carrierFrequency, float noiseBlend)
    {
        int startSample = Mathf.Clamp(Mathf.RoundToInt(startTime * SampleRate), 0, data.Length - 1);
        int endSample = Mathf.Clamp(Mathf.RoundToInt((startTime + duration) * SampleRate), startSample + 1, data.Length);
        float seed = startTime * 17.13f + carrierFrequency * 0.0017f;

        for (int i = startSample; i < endSample; i++)
        {
            float localTime = (i - startSample) / (float)SampleRate;
            float normalized = Mathf.Clamp01(localTime / Mathf.Max(0.001f, duration));
            float envelope = Mathf.Exp(-normalized * 9f);
            float tone = Mathf.Sin(localTime * carrierFrequency * Mathf.PI * 2f) * 0.28f;
            float overtone = Mathf.Sin(localTime * carrierFrequency * 1.9f * Mathf.PI * 2f) * 0.17f;
            float noise = (Mathf.PerlinNoise(seed, localTime * 240f) * 2f - 1f) * noiseBlend;
            data[i] += (tone + overtone + noise) * amplitude * envelope;
        }
    }

    private void AddSynthNote(
        float[] data,
        float frequency,
        float amplitude,
        float startTime,
        float duration,
        float attackTime,
        float releaseTime,
        float vibratoDepth,
        float vibratoSpeed,
        float harmonicBlend,
        float noiseBlend)
    {
        int startSample = Mathf.Clamp(Mathf.RoundToInt(startTime * SampleRate), 0, data.Length - 1);
        int endSample = Mathf.Clamp(Mathf.RoundToInt((startTime + duration) * SampleRate), startSample + 1, data.Length);
        float sustainEnd = Mathf.Max(attackTime, duration - releaseTime);
        float seed = startTime * 7.31f + frequency * 0.013f;

        for (int i = startSample; i < endSample; i++)
        {
            float localTime = (i - startSample) / (float)SampleRate;
            float envelope;

            if (localTime < attackTime)
            {
                envelope = Mathf.Clamp01(localTime / Mathf.Max(0.001f, attackTime));
            }
            else if (localTime > sustainEnd)
            {
                float releaseNormalized = (localTime - sustainEnd) / Mathf.Max(0.001f, duration - sustainEnd);
                envelope = Mathf.Clamp01(1f - releaseNormalized);
            }
            else
            {
                envelope = 1f;
            }

            envelope *= 0.86f + 0.14f * Mathf.Cos(Mathf.Clamp01(localTime / Mathf.Max(0.001f, duration)) * Mathf.PI);

            float vibrato = 1f + Mathf.Sin(localTime * vibratoSpeed * Mathf.PI * 2f) * vibratoDepth;
            float phaseFrequency = frequency * vibrato;
            float fundamental = Mathf.Sin(localTime * phaseFrequency * Mathf.PI * 2f);
            float second = Mathf.Sin(localTime * phaseFrequency * 2.01f * Mathf.PI * 2f) * harmonicBlend;
            float third = Mathf.Sin(localTime * phaseFrequency * 3.02f * Mathf.PI * 2f) * harmonicBlend * 0.46f;
            float noise = (Mathf.PerlinNoise(seed, localTime * 38f) * 2f - 1f) * noiseBlend;

            data[i] += (fundamental * 0.72f + second * 0.22f + third * 0.12f + noise) * amplitude * envelope;
        }
    }

    private float GetScaleFrequency(float rootFrequency, int scaleDegree, int octaveOffset)
    {
        int scaleLength = MajorScaleSemitones.Length;
        int wrappedDegree = Mathf.FloorToInt(Mathf.Repeat(scaleDegree, scaleLength));
        int octaveCarry = Mathf.FloorToInt(scaleDegree / (float)scaleLength);
        int semitoneOffset = MajorScaleSemitones[wrappedDegree] + (octaveOffset + octaveCarry) * 12;
        float frequency = rootFrequency * Mathf.Pow(2f, semitoneOffset / 12f);
        return QuantizeFrequency(frequency, AmbientLoopLength);
    }

    private void AddDrone(float[] data, float frequency, float amplitude, float wobbleSpeed)
    {
        for (int i = 0; i < data.Length; i++)
        {
            float time = i / (float)SampleRate;
            float wobble = 1f + Mathf.Sin(time * wobbleSpeed * Mathf.PI * 2f) * 0.08f;
            data[i] += Mathf.Sin(time * frequency * wobble * Mathf.PI * 2f) * amplitude;
        }
    }

    private void AddPulsePad(float[] data, float frequency, float amplitude, int pulseCount)
    {
        for (int i = 0; i < data.Length; i++)
        {
            float time = i / (float)SampleRate;
            float pulse = 0.45f + 0.55f * Mathf.Pow(Mathf.Max(0f, Mathf.Sin((time / AmbientLoopLength) * pulseCount * Mathf.PI * 2f)), 2f);
            data[i] += Mathf.Sin(time * frequency * Mathf.PI * 2f) * amplitude * pulse;
        }
    }

    private void AddRhythmBed(float[] data, float pulseFrequency, float accentFrequency, float bpm, float amplitude)
    {
        float beatDuration = 60f / Mathf.Max(40f, bpm);
        int beatCount = Mathf.Max(1, Mathf.RoundToInt(AmbientLoopLength / beatDuration));

        for (int beatIndex = 0; beatIndex < beatCount; beatIndex++)
        {
            float beatTime = beatIndex * beatDuration;
            float beatStrength = beatIndex % 4 == 0 ? 1f : (beatIndex % 2 == 0 ? 0.78f : 0.62f);

            AddPercussivePulse(
                data,
                pulseFrequency * (beatIndex % 4 == 3 ? 1.04f : 1f),
                amplitude * beatStrength,
                beatTime,
                0.18f,
                0.055f);

            float offBeatTime = beatTime + beatDuration * 0.5f;

            if (offBeatTime < AmbientLoopLength - 0.12f)
            {
                AddPercussivePulse(
                    data,
                    accentFrequency * 0.52f,
                    amplitude * 0.42f,
                    offBeatTime,
                    0.1f,
                    0.075f);
            }
        }
    }

    private void AddPercussivePulse(
        float[] data,
        float frequency,
        float amplitude,
        float startTime,
        float duration,
        float noiseAmount)
    {
        int startSample = Mathf.Clamp(Mathf.RoundToInt(startTime * SampleRate), 0, data.Length - 1);
        int endSample = Mathf.Clamp(Mathf.RoundToInt((startTime + duration) * SampleRate), startSample + 1, data.Length);
        float seed = startTime * 17.13f + frequency * 0.013f;

        for (int i = startSample; i < endSample; i++)
        {
            float localTime = (i - startSample) / (float)SampleRate;
            float attack = Mathf.Clamp01(localTime / 0.018f);
            float decay = Mathf.Exp(-localTime * 14f);
            float envelope = attack * decay;
            float body = Mathf.Sin(localTime * frequency * Mathf.PI * 2f);
            float overtone = Mathf.Sin(localTime * frequency * 1.94f * Mathf.PI * 2f) * 0.24f;
            float grit = (Mathf.PerlinNoise(seed, localTime * 38f) * 2f - 1f) * noiseAmount;
            data[i] += (body * 0.78f + overtone * 0.22f + grit) * amplitude * envelope;
        }
    }

    private void AddMelody(float[] data, float root, float accent, float amplitude)
    {
        float[] noteMultipliers = { 1f, 1.5f, 1.25f, 1.75f, 1.5f, 2f };
        float stepDuration = AmbientLoopLength / noteMultipliers.Length;

        for (int noteIndex = 0; noteIndex < noteMultipliers.Length; noteIndex++)
        {
            float noteStart = noteIndex * stepDuration + 0.35f;
            float noteEnd = Mathf.Min(AmbientLoopLength, noteStart + stepDuration * 0.45f);
            float noteFrequency = QuantizeFrequency(root * noteMultipliers[noteIndex], AmbientLoopLength);

            if (noteIndex % 2 == 1)
                noteFrequency = QuantizeFrequency(accent * (0.75f + noteIndex * 0.05f), AmbientLoopLength);

            AddWindowedTone(data, noteFrequency, amplitude, noteStart, noteEnd);
        }
    }

    private void AddShimmer(float[] data, int level, float amplitude)
    {
        float shimmerA = QuantizeFrequency(523f + (level * 6f), AmbientLoopLength);
        float shimmerB = QuantizeFrequency(659f + (level * 4f), AmbientLoopLength);

        for (int i = 0; i < data.Length; i++)
        {
            float time = i / (float)SampleRate;
            float shimmerEnvelope = 0.45f + 0.55f * Mathf.Sin((time / AmbientLoopLength) * Mathf.PI * 2f * 3f);
            float shimmer = Mathf.Sin(time * shimmerA * Mathf.PI * 2f) + Mathf.Sin(time * shimmerB * Mathf.PI * 2f);
            data[i] += shimmer * amplitude * shimmerEnvelope * 0.5f;
        }
    }

    private void AddBassPulse(float[] data, float frequency, float amplitude)
    {
        const int pulseCount = 8;
        float pulseLength = AmbientLoopLength / pulseCount;

        for (int pulseIndex = 0; pulseIndex < pulseCount; pulseIndex++)
        {
            float startTime = pulseIndex * pulseLength;
            float endTime = Mathf.Min(AmbientLoopLength, startTime + pulseLength * 0.42f);
            AddWindowedTone(data, frequency * (pulseIndex % 2 == 0 ? 1f : 1.02f), amplitude, startTime, endTime);
        }
    }

    private void AddChamberResonance(float[] data, float firstFrequency, float secondFrequency, float amplitude)
    {
        for (int i = 0; i < data.Length; i++)
        {
            float time = i / (float)SampleRate;
            float envelope = 0.72f + 0.28f * Mathf.Sin((time / AmbientLoopLength) * Mathf.PI * 2f);
            float wobble = 1f + Mathf.Sin(time * Mathf.PI * 2f * 0.09f) * 0.04f;
            float toneA = Mathf.Sin(time * firstFrequency * wobble * Mathf.PI * 2f);
            float toneB = Mathf.Sin(time * secondFrequency * Mathf.PI * 2f);
            data[i] += (toneA * 0.7f + toneB * 0.3f) * amplitude * envelope;
        }
    }

    private void AddStonePercussion(float[] data, int level, float amplitude)
    {
        float[] normalizedHits =
        {
            0.78f,
            1.62f,
            2.46f,
            3.35f,
            4.28f,
            5.14f,
            6.08f,
            6.92f
        };

        for (int i = 0; i < normalizedHits.Length; i++)
        {
            float hitTime = Mathf.Min(AmbientLoopLength - 0.18f, normalizedHits[i]);
            float pitch = 540f + level * 7f + (i % 3) * 46f;
            AddPercussivePulse(data, pitch, amplitude * (0.75f + (i % 2) * 0.15f), hitTime, 0.055f, 0.11f);
        }
    }

    private void AddCaveRumble(float[] data, int level, float amplitude)
    {
        float seedA = 11f + level * 0.17f;
        float seedB = 37f + level * 0.11f;

        for (int i = 0; i < data.Length; i++)
        {
            float time = i / (float)SampleRate;
            float slow = Mathf.PerlinNoise(seedA, time * 0.08f) * 2f - 1f;
            float mid = Mathf.PerlinNoise(seedB, time * 0.42f) * 2f - 1f;
            float envelope = 0.7f + 0.3f * Mathf.Sin((time / AmbientLoopLength) * Mathf.PI * 2f);
            data[i] += (slow * 0.82f + mid * 0.18f) * amplitude * envelope;
        }
    }

    private void AddAirNoise(float[] data, int level, float amplitude)
    {
        float seed = 71f + level * 0.29f;

        for (int i = 0; i < data.Length; i++)
        {
            float time = i / (float)SampleRate;
            float wind = Mathf.PerlinNoise(seed, time * 1.8f) * 2f - 1f;
            float flutter = Mathf.PerlinNoise(seed + 13f, time * 6.5f) * 2f - 1f;
            float envelope = 0.35f + 0.65f * Mathf.Sin((time / AmbientLoopLength) * Mathf.PI * 2f * 2f);
            data[i] += (wind * 0.75f + flutter * 0.25f) * amplitude * envelope;
        }
    }

    private void AddDripEchoes(float[] data, float root, float accent, float amplitude)
    {
        float[] pitches = { root * 1.84f, accent * 1.04f, root * 1.42f, accent * 0.9f };
        float[] starts =
        {
            AmbientLoopLength * 0.12f,
            AmbientLoopLength * 0.34f,
            AmbientLoopLength * 0.56f,
            AmbientLoopLength * 0.79f
        };

        for (int i = 0; i < pitches.Length; i++)
        {
            float start = starts[i];
            AddWindowedTone(data, pitches[i], amplitude, start, start + 0.18f);
            AddWindowedTone(data, pitches[i] * 0.5f, amplitude * 0.3f, start + 0.22f, start + 0.46f);
        }
    }

    private void AddEchoMotif(float[] data, float root, float accent, float amplitude)
    {
        float[] motif = { root * 2f, accent * 1.12f, root * 1.5f, accent * 0.92f };
        float stepDuration = AmbientLoopLength / 8f;

        for (int i = 0; i < motif.Length; i++)
        {
            float baseTime = i * stepDuration + 0.7f;
            AddWindowedTone(data, motif[i], amplitude, baseTime, baseTime + stepDuration * 0.28f);
            AddWindowedTone(data, motif[i] * 0.5f, amplitude * 0.45f, baseTime + 0.22f, baseTime + 0.22f + stepDuration * 0.24f);
        }
    }

    private void ApplyLoopEdgeFade(float[] data, float fadeDurationSeconds)
    {
        int fadeSamples = Mathf.Clamp(Mathf.RoundToInt(fadeDurationSeconds * SampleRate), 32, data.Length / 4);

        for (int i = 0; i < fadeSamples; i++)
        {
            float t = i / Mathf.Max(1f, fadeSamples - 1f);
            float fadeIn = Mathf.SmoothStep(0f, 1f, t);
            float fadeOut = Mathf.SmoothStep(1f, 0f, t);
            data[i] *= fadeIn;
            data[data.Length - fadeSamples + i] *= fadeOut;
        }

        data[0] = 0f;
        data[data.Length - 1] = 0f;
    }

    private void AddWindowedTone(float[] data, float frequency, float amplitude, float startTime, float endTime)
    {
        int startSample = Mathf.Clamp(Mathf.RoundToInt(startTime * SampleRate), 0, data.Length - 1);
        int endSample = Mathf.Clamp(Mathf.RoundToInt(endTime * SampleRate), startSample + 1, data.Length);
        float duration = Mathf.Max(0.001f, endTime - startTime);

        for (int i = startSample; i < endSample; i++)
        {
            float localTime = (i - startSample) / (float)SampleRate;
            float envelope = Mathf.Sin(Mathf.Clamp01(localTime / duration) * Mathf.PI);
            data[i] += Mathf.Sin(localTime * frequency * Mathf.PI * 2f) * amplitude * envelope;
        }
    }

    private AudioClip BuildCoinClip()
    {
        return BuildSweepClip("CoinPickup", 0.16f, 980f, 1480f, 0.75f, true);
    }

    private AudioClip BuildHitClip()
    {
        return BuildDualToneClip("ObstacleHit", 0.24f, 196f, 92f, 0.78f, 0.42f);
    }

    private AudioClip BuildShieldActivateClip()
    {
        return BuildArpeggioClip("ShieldOn", new[] { 540f, 810f, 1080f }, 0.24f, 0.6f);
    }

    private AudioClip BuildShieldBlockClip()
    {
        return BuildSweepClip("ShieldBlock", 0.22f, 1240f, 660f, 0.7f, false);
    }

    private AudioClip BuildExtraLifeArmClip()
    {
        return BuildArpeggioClip("ExtraLifeArm", new[] { 440f, 554f }, 0.32f, 0.58f);
    }

    private AudioClip BuildExtraLifeReviveClip()
    {
        return BuildArpeggioClip("ExtraLifeRevive", new[] { 392f, 523f, 659f, 784f }, 0.48f, 0.74f);
    }

    private AudioClip BuildBombClip()
    {
        return BuildDualToneClip("Bomb", 0.42f, 110f, 58f, 0.95f, 0.56f);
    }

    private AudioClip BuildSpeedClip()
    {
        return BuildSweepClip("SpeedBoost", 0.2f, 420f, 980f, 0.62f, true);
    }

    private AudioClip BuildMagnetClip()
    {
        return BuildArpeggioClip("CoinMagnet", new[] { 300f, 450f, 600f }, 0.28f, 0.62f);
    }

    private AudioClip BuildDoubleCoinsClip()
    {
        return BuildArpeggioClip("DoubleCoins", new[] { 880f, 1176f, 1568f }, 0.2f, 0.64f);
    }

    private AudioClip BuildSlowTimeClip()
    {
        return BuildSweepClip("SlowTime", 0.4f, 600f, 180f, 0.72f, false);
    }

    private AudioClip BuildSmallerClip()
    {
        return BuildSweepClip("SmallerPlayer", 0.18f, 760f, 420f, 0.5f, false);
    }

    private AudioClip BuildScoreClip()
    {
        return BuildArpeggioClip("ScoreBooster", new[] { 520f, 660f, 880f }, 0.22f, 0.62f);
    }

    private AudioClip BuildRareClip()
    {
        return BuildArpeggioClip("RareCoinBoost", new[] { 1040f, 1310f, 1560f }, 0.22f, 0.6f);
    }

    private AudioClip BuildLevelUpClip()
    {
        return BuildArpeggioClip("LevelUp", new[] { 392f, 523f, 659f }, 0.34f, 0.68f);
    }

    private AudioClip BuildGenericUpgradeClip()
    {
        return BuildArpeggioClip("UpgradeGeneric", new[] { 480f, 720f }, 0.22f, 0.52f);
    }

    private AudioClip BuildSweepClip(string clipName, float length, float startFrequency, float endFrequency, float amplitude, bool brightenTail)
    {
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(length * SampleRate));
        float[] data = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float time = i / (float)SampleRate;
            float t = i / (float)(sampleCount - 1);
            float frequency = Mathf.Lerp(startFrequency, endFrequency, t);
            float envelope = Mathf.Exp(-5.2f * t);
            float sample = Mathf.Sin(time * frequency * Mathf.PI * 2f) * amplitude * envelope;

            if (brightenTail)
                sample += Mathf.Sin(time * frequency * 2f * Mathf.PI * 2f) * amplitude * 0.18f * envelope;

            data[i] = sample;
        }

        Normalize(data, 0.9f);
        return CreateClip(clipName, data);
    }

    private AudioClip BuildDualToneClip(string clipName, float length, float firstFrequency, float secondFrequency, float firstAmplitude, float secondAmplitude)
    {
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(length * SampleRate));
        float[] data = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float time = i / (float)SampleRate;
            float t = i / (float)(sampleCount - 1);
            float envelope = Mathf.Exp(-4.4f * t);
            float first = Mathf.Sin(time * firstFrequency * Mathf.PI * 2f) * firstAmplitude;
            float second = Mathf.Sin(time * secondFrequency * Mathf.PI * 2f) * secondAmplitude;
            float grit = Mathf.Sin(time * (firstFrequency * 2.4f) * Mathf.PI * 2f) * 0.16f;
            data[i] = (first + second + grit) * envelope;
        }

        Normalize(data, 0.92f);
        return CreateClip(clipName, data);
    }

    private AudioClip BuildArpeggioClip(string clipName, float[] frequencies, float length, float amplitude)
    {
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(length * SampleRate));
        float[] data = new float[sampleCount];
        float noteDuration = length / Mathf.Max(1, frequencies.Length);

        for (int noteIndex = 0; noteIndex < frequencies.Length; noteIndex++)
        {
            float startTime = noteIndex * noteDuration;
            float endTime = Mathf.Min(length, startTime + noteDuration * 0.92f);
            int startSample = Mathf.RoundToInt(startTime * SampleRate);
            int endSample = Mathf.Min(sampleCount, Mathf.RoundToInt(endTime * SampleRate));

            for (int i = startSample; i < endSample; i++)
            {
                float localTime = (i - startSample) / (float)SampleRate;
                float normalized = Mathf.Clamp01(localTime / Mathf.Max(0.001f, noteDuration));
                float envelope = Mathf.Sin(normalized * Mathf.PI) * Mathf.Exp(-1.2f * normalized);
                float tone = Mathf.Sin(localTime * frequencies[noteIndex] * Mathf.PI * 2f);
                float overtone = Mathf.Sin(localTime * frequencies[noteIndex] * 2f * Mathf.PI * 2f) * 0.18f;
                data[i] += (tone + overtone) * amplitude * envelope;
            }
        }

        Normalize(data, 0.9f);
        return CreateClip(clipName, data);
    }

    private AudioClip CreateClip(string clipName, float[] data)
    {
        AudioClip clip = AudioClip.Create(clipName, data.Length, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private void Normalize(float[] data, float peak)
    {
        float max = 0.0001f;

        for (int i = 0; i < data.Length; i++)
            max = Mathf.Max(max, Mathf.Abs(data[i]));

        float multiplier = peak / max;

        for (int i = 0; i < data.Length; i++)
            data[i] *= multiplier;
    }

    private float QuantizeFrequency(float approximateFrequency, float clipLength)
    {
        int cycles = Mathf.Max(1, Mathf.RoundToInt(approximateFrequency * clipLength));
        return cycles / clipLength;
    }

    private void UpdateMusicMix()
    {
        AudioSource activeSource = GetActiveMusicSource();

        if (activeSource == null || activeSource.clip == null)
            return;

        if (!activeSource.isPlaying)
            activeSource.Play();

        float targetVolume = GetTargetMusicVolume();
        activeSource.volume = Mathf.MoveTowards(activeSource.volume, targetVolume, Time.unscaledDeltaTime * 0.35f);
    }

    private float GetTargetMusicVolume()
    {
        if (!IsGameplaySceneActive())
            return MenuMusicVolume;

        float build = gameManager != null ? gameManager.GetLevelProgress01() : 0f;
        return Mathf.Lerp(BaseMusicVolume, PeakMusicVolume, Mathf.SmoothStep(0f, 1f, build));
    }
}
