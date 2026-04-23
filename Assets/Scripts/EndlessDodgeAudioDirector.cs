using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessDodgeAudioDirector : MonoBehaviour
{
    private readonly Dictionary<int, AudioClip> ambientLoops = new Dictionary<int, AudioClip>();
    private readonly Dictionary<string, AudioClip> sfxLibrary = new Dictionary<string, AudioClip>();
    private readonly List<AudioSource> sfxVoices = new List<AudioSource>();

    private GameManager gameManager;
    private AudioSource musicSourceA;
    private AudioSource musicSourceB;
    private int activeMusicSourceIndex;
    private int lastAppliedBiomeIndex = -1;
    private RuntimeCaveTheme currentTheme;
    private Coroutine musicFadeRoutine;
    private int sfxVoiceIndex;

    private const int SampleRate = 22050;
    private const float AmbientLoopLength = 8f;
    private const float BaseMusicVolume = 0.94f;
    private const float PeakMusicVolume = 1f;

    void Awake()
    {
        gameManager = GameManager.instance;
        EnsureAudioSources();
        BuildSfxLibrary();
    }

    void Start()
    {
        ApplyTheme(CaveThemeLibrary.GetThemeForLevel(GetCurrentLevel()), immediate: true);
    }

    void Update()
    {
        if (gameManager == null)
            gameManager = GameManager.instance;

        RuntimeCaveTheme theme = CaveThemeLibrary.GetThemeForLevel(GetCurrentLevel());

        if (theme.BiomeIndex != lastAppliedBiomeIndex)
            ApplyTheme(theme, immediate: false);

        UpdateMusicMix();
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
        float root = QuantizeFrequency(theme.RootFrequency, AmbientLoopLength);
        float accent = QuantizeFrequency(theme.AccentFrequency, AmbientLoopLength);

        AddDrone(data, root * 0.5f, 0.08f, 0.04f);
        AddDrone(data, root * 0.98f, 0.035f, 0.03f);
        AddChamberResonance(data, root * 1.5f, accent * 0.98f, 0.042f);
        AddCaveRumble(data, theme.Level, 0.19f);
        AddAirNoise(data, theme.Level, 0.11f);
        AddRhythmBed(data, root * 0.5f, accent, 112f + theme.BiomeIndex * 8f, 0.18f);
        AddStonePercussion(data, theme.Level, 0.085f);
        AddDripEchoes(data, root, accent, 0.07f);
        ApplyLoopEdgeFade(data, 0.22f);
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
        float build = gameManager != null ? gameManager.GetLevelProgress01() : 0f;
        return Mathf.Lerp(BaseMusicVolume, PeakMusicVolume, Mathf.SmoothStep(0f, 1f, build));
    }
}
