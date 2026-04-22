using System.Collections.Generic;
using UnityEngine;

public class DynamicBackgroundController : MonoBehaviour
{
    private enum BackgroundShape
    {
        Cloud,
        Mist,
        Shard,
        Diamond,
        Streak
    }

    private sealed class SpawnConfig
    {
        public BackgroundShape shape;
        public bool preferEdges;
        public int sortingOrder;
        public float intervalMin;
        public float intervalMax;
        public float widthScaleMin;
        public float widthScaleMax;
        public float heightScaleMin;
        public float heightScaleMax;
        public float speedMin;
        public float speedMax;
        public float swayMin;
        public float swayMax;
        public float swayFrequencyMin;
        public float swayFrequencyMax;
        public float rotationMin;
        public float rotationMax;
        public float angularVelocityMin;
        public float angularVelocityMax;
        public float alphaMin;
        public float alphaMax;
        public Color colorA;
        public Color colorB;
    }

    private sealed class ThemeProfile
    {
        public string name;
        public Color topColor;
        public Color bottomColor;
        public SpawnConfig[] spawns;
    }

    private sealed class SpawnChannel
    {
        public SpawnConfig config;
        public float timer;
    }

    private sealed class BackgroundDrifter
    {
        public Transform transform;
        public SpriteRenderer renderer;
        public float baseX;
        public float speed;
        public float swayAmplitude;
        public float swayFrequency;
        public float phase;
        public float angularVelocity;
        public float despawnPadding;
    }

    private static readonly Dictionary<BackgroundShape, Sprite> spriteCache = new Dictionary<BackgroundShape, Sprite>();

    private readonly List<SpawnChannel> spawnChannels = new List<SpawnChannel>();
    private readonly List<BackgroundDrifter> activeDrifters = new List<BackgroundDrifter>();

    private Camera targetCamera;
    private SpriteRenderer gradientRenderer;
    private ThemeProfile[] themes;
    private ThemeProfile activeTheme;
    private int activeLevel = -1;
    private float halfWidth;
    private float halfHeight;
    private int cachedScreenWidth;
    private int cachedScreenHeight;

    public static DynamicBackgroundController EnsureExists()
    {
        DynamicBackgroundController existing = FindAnyObjectByType<DynamicBackgroundController>();

        if (existing != null)
            return existing;

        GameObject backgroundObject = new GameObject("DynamicBackground");
        return backgroundObject.AddComponent<DynamicBackgroundController>();
    }

    void Awake()
    {
        targetCamera = Camera.main;
        themes = BuildThemes();
        CreateGradientRenderer();
        RefreshCameraBounds();
    }

    void Start()
    {
        ApplyThemeForCurrentLevel(true);
    }

    void LateUpdate()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            return;

        if (Screen.width != cachedScreenWidth || Screen.height != cachedScreenHeight)
            RefreshCameraBounds();

        ApplyThemeForCurrentLevel(false);
        UpdateGradientRenderer();
        UpdateSpawnChannels();
        UpdateDrifters();
    }

    void OnDestroy()
    {
        for (int i = 0; i < activeDrifters.Count; i++)
        {
            if (activeDrifters[i] != null && activeDrifters[i].transform != null)
                Destroy(activeDrifters[i].transform.gameObject);
        }

        activeDrifters.Clear();
    }

    void CreateGradientRenderer()
    {
        GameObject gradientObject = new GameObject("Backdrop", typeof(SpriteRenderer));
        gradientObject.transform.SetParent(transform, false);
        gradientRenderer = gradientObject.GetComponent<SpriteRenderer>();
        gradientRenderer.sortingOrder = -320;
    }

    void ApplyThemeForCurrentLevel(bool force)
    {
        int level = 1;

        if (GameManager.instance != null)
            level = Mathf.Max(1, GameManager.instance.GetDifficultyLevel());

        if (!force && level == activeLevel)
            return;

        activeLevel = level;
        activeTheme = themes[(activeLevel - 1) % themes.Length];
        RebuildSpawnChannels();
        ApplyGradient(activeTheme.topColor, activeTheme.bottomColor);
    }

    void RebuildSpawnChannels()
    {
        spawnChannels.Clear();

        if (activeTheme == null || activeTheme.spawns == null)
            return;

        for (int i = 0; i < activeTheme.spawns.Length; i++)
        {
            SpawnChannel channel = new SpawnChannel();
            channel.config = activeTheme.spawns[i];
            channel.timer = Random.Range(channel.config.intervalMin, channel.config.intervalMax);
            spawnChannels.Add(channel);
        }
    }

    void ApplyGradient(Color topColor, Color bottomColor)
    {
        Sprite gradientSprite = CreateGradientSprite(topColor, bottomColor);

        if (gradientRenderer != null)
        {
            gradientRenderer.sprite = gradientSprite;
            gradientRenderer.color = Color.white;
        }
    }

    void RefreshCameraBounds()
    {
        if (targetCamera == null)
            return;

        halfHeight = targetCamera.orthographicSize;
        halfWidth = halfHeight * targetCamera.aspect;
        cachedScreenWidth = Screen.width;
        cachedScreenHeight = Screen.height;
        UpdateGradientRenderer();
    }

    void UpdateGradientRenderer()
    {
        if (targetCamera == null || gradientRenderer == null)
            return;

        Transform gradientTransform = gradientRenderer.transform;
        gradientTransform.position = new Vector3(targetCamera.transform.position.x, targetCamera.transform.position.y, 0f);
        gradientTransform.localScale = new Vector3((halfWidth * 2.4f) / 2f, (halfHeight * 2.4f) / 8f, 1f);
    }

    void UpdateSpawnChannels()
    {
        if (activeTheme == null || spawnChannels.Count == 0)
            return;

        float deltaTime = Time.deltaTime;

        if (deltaTime <= 0f)
            return;

        float densityMultiplier = 1f + Mathf.Min(0.55f, (activeLevel - 1) * 0.06f);

        for (int i = 0; i < spawnChannels.Count; i++)
        {
            SpawnChannel channel = spawnChannels[i];
            channel.timer -= deltaTime * densityMultiplier;

            if (channel.timer > 0f)
                continue;

            SpawnDrifter(channel.config);
            channel.timer = Random.Range(channel.config.intervalMin, channel.config.intervalMax);
        }
    }

    void SpawnDrifter(SpawnConfig config)
    {
        if (targetCamera == null)
            return;

        GameObject drifterObject = new GameObject(config.shape + "Drifter", typeof(SpriteRenderer));
        drifterObject.transform.SetParent(transform, false);

        SpriteRenderer renderer = drifterObject.GetComponent<SpriteRenderer>();
        renderer.sprite = GetShapeSprite(config.shape);
        renderer.sortingOrder = config.sortingOrder;
        renderer.color = BuildTint(config);

        Vector3 spawnPosition = GetSpawnPosition(config);
        float widthScale = Random.Range(config.widthScaleMin, config.widthScaleMax);
        float heightScale = Random.Range(config.heightScaleMin, config.heightScaleMax);
        float rotationZ = Random.Range(config.rotationMin, config.rotationMax);

        drifterObject.transform.position = spawnPosition;
        drifterObject.transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
        drifterObject.transform.localScale = new Vector3(widthScale, heightScale, 1f);

        BackgroundDrifter drifter = new BackgroundDrifter();
        drifter.transform = drifterObject.transform;
        drifter.renderer = renderer;
        drifter.baseX = spawnPosition.x;
        drifter.speed = Random.Range(config.speedMin, config.speedMax) * GetLevelSpeedMultiplier();
        drifter.swayAmplitude = Random.Range(config.swayMin, config.swayMax);
        drifter.swayFrequency = Random.Range(config.swayFrequencyMin, config.swayFrequencyMax);
        drifter.phase = Random.Range(0f, Mathf.PI * 2f);
        drifter.angularVelocity = Random.Range(config.angularVelocityMin, config.angularVelocityMax);
        drifter.despawnPadding = Mathf.Max(2.5f, heightScale + 1.5f);
        activeDrifters.Add(drifter);
    }

    Vector3 GetSpawnPosition(SpawnConfig config)
    {
        float left = targetCamera.transform.position.x - halfWidth;
        float right = targetCamera.transform.position.x + halfWidth;
        float top = targetCamera.transform.position.y + halfHeight + 2.2f;
        float x;

        if (config.preferEdges)
        {
            float edgeBandWidth = halfWidth * 0.45f;

            if (Random.value < 0.5f)
                x = Random.Range(left - 0.7f, left + edgeBandWidth);
            else
                x = Random.Range(right - edgeBandWidth, right + 0.7f);
        }
        else
        {
            x = Random.Range(left - 0.5f, right + 0.5f);
        }

        return new Vector3(x, top, 0f);
    }

    void UpdateDrifters()
    {
        if (activeDrifters.Count == 0 || targetCamera == null)
            return;

        float worldMultiplier = 1f;

        if (GameManager.instance != null)
            worldMultiplier = GameManager.instance.GetWorldSpeedMultiplier();

        float deltaTime = Time.deltaTime;
        float bottomLimit = targetCamera.transform.position.y - halfHeight;

        for (int i = activeDrifters.Count - 1; i >= 0; i--)
        {
            BackgroundDrifter drifter = activeDrifters[i];

            if (drifter == null || drifter.transform == null)
            {
                activeDrifters.RemoveAt(i);
                continue;
            }

            Vector3 position = drifter.transform.position;
            position.y -= drifter.speed * worldMultiplier * deltaTime;
            position.x = drifter.baseX + Mathf.Sin((Time.time * drifter.swayFrequency) + drifter.phase) * drifter.swayAmplitude;
            drifter.transform.position = position;
            drifter.transform.Rotate(0f, 0f, drifter.angularVelocity * deltaTime);

            if (position.y < bottomLimit - drifter.despawnPadding)
            {
                Destroy(drifter.transform.gameObject);
                activeDrifters.RemoveAt(i);
            }
        }
    }

    float GetLevelSpeedMultiplier()
    {
        return 1f + Mathf.Min(0.4f, (activeLevel - 1) * 0.04f);
    }

    Color BuildTint(SpawnConfig config)
    {
        Color tint = Color.Lerp(config.colorA, config.colorB, Random.value);
        tint.a = Random.Range(config.alphaMin, config.alphaMax);
        return tint;
    }

    ThemeProfile[] BuildThemes()
    {
        return new[]
        {
            CreateTheme(
                "Cloud Sea",
                new Color(0.42f, 0.7f, 0.96f, 1f),
                new Color(0.9f, 0.96f, 1f, 1f),
                new[]
                {
                    CreateSpawn(BackgroundShape.Cloud, false, -270, 0.38f, 0.72f, 1.8f, 3.4f, 0.9f, 1.7f, 0.35f, 0.62f, 0.15f, 0.55f, 0.2f, 0.6f, -8f, 8f, -4f, 4f, 0.16f, 0.28f, new Color(1f, 1f, 1f, 1f), new Color(0.84f, 0.93f, 1f, 1f)),
                    CreateSpawn(BackgroundShape.Mist, false, -290, 0.9f, 1.4f, 2.8f, 4.8f, 1.5f, 2.6f, 0.18f, 0.34f, 0.1f, 0.35f, 0.12f, 0.35f, -5f, 5f, -2f, 2f, 0.07f, 0.14f, new Color(1f, 1f, 1f, 1f), new Color(0.74f, 0.89f, 1f, 1f))
                }),
            CreateTheme(
                "Ember Cave",
                new Color(0.08f, 0.09f, 0.14f, 1f),
                new Color(0.24f, 0.14f, 0.1f, 1f),
                new[]
                {
                    CreateSpawn(BackgroundShape.Shard, true, -250, 0.22f, 0.42f, 1.7f, 3.8f, 2.8f, 5.6f, 0.7f, 1.15f, 0.08f, 0.32f, 0.12f, 0.35f, -22f, 22f, -5f, 5f, 0.28f, 0.46f, new Color(0.13f, 0.11f, 0.12f, 1f), new Color(0.25f, 0.17f, 0.12f, 1f)),
                    CreateSpawn(BackgroundShape.Diamond, false, -240, 0.48f, 0.82f, 0.18f, 0.42f, 0.22f, 0.58f, 0.55f, 0.95f, 0.04f, 0.18f, 0.25f, 0.8f, -45f, 45f, -12f, 12f, 0.14f, 0.24f, new Color(1f, 0.68f, 0.29f, 1f), new Color(0.92f, 0.37f, 0.18f, 1f))
                }),
            CreateTheme(
                "Crystal Tunnel",
                new Color(0.05f, 0.08f, 0.19f, 1f),
                new Color(0.12f, 0.2f, 0.3f, 1f),
                new[]
                {
                    CreateSpawn(BackgroundShape.Shard, true, -250, 0.26f, 0.46f, 1.2f, 2.4f, 2.6f, 4.8f, 0.75f, 1.25f, 0.05f, 0.2f, 0.18f, 0.55f, -30f, 30f, -7f, 7f, 0.24f, 0.38f, new Color(0.47f, 0.92f, 1f, 1f), new Color(0.6f, 0.58f, 1f, 1f)),
                    CreateSpawn(BackgroundShape.Mist, false, -285, 0.88f, 1.45f, 2.4f, 4f, 1.4f, 2.4f, 0.18f, 0.36f, 0.08f, 0.26f, 0.16f, 0.4f, -10f, 10f, -2f, 2f, 0.06f, 0.12f, new Color(0.42f, 0.92f, 1f, 1f), new Color(0.43f, 0.5f, 0.98f, 1f))
                }),
            CreateTheme(
                "Storm Front",
                new Color(0.05f, 0.09f, 0.16f, 1f),
                new Color(0.22f, 0.25f, 0.31f, 1f),
                new[]
                {
                    CreateSpawn(BackgroundShape.Streak, false, -245, 0.08f, 0.14f, 0.25f, 0.55f, 1.9f, 3.2f, 1.3f, 2.05f, 0f, 0.12f, 0.2f, 0.55f, -14f, 14f, -12f, 12f, 0.12f, 0.24f, new Color(0.84f, 0.93f, 1f, 1f), new Color(0.57f, 0.74f, 0.95f, 1f)),
                    CreateSpawn(BackgroundShape.Cloud, false, -275, 0.52f, 0.92f, 1.7f, 3.1f, 0.9f, 1.8f, 0.34f, 0.72f, 0.08f, 0.26f, 0.14f, 0.38f, -8f, 8f, -3f, 3f, 0.11f, 0.2f, new Color(0.23f, 0.29f, 0.37f, 1f), new Color(0.36f, 0.44f, 0.56f, 1f))
                }),
            CreateTheme(
                "Star Drift",
                new Color(0.02f, 0.03f, 0.09f, 1f),
                new Color(0.05f, 0.09f, 0.18f, 1f),
                new[]
                {
                    CreateSpawn(BackgroundShape.Diamond, false, -230, 0.07f, 0.16f, 0.1f, 0.28f, 0.1f, 0.28f, 0.35f, 0.85f, 0f, 0.14f, 0.2f, 0.9f, -20f, 20f, -14f, 14f, 0.22f, 0.42f, new Color(1f, 0.95f, 0.78f, 1f), new Color(0.64f, 0.82f, 1f, 1f)),
                    CreateSpawn(BackgroundShape.Mist, false, -300, 1.05f, 1.75f, 2.8f, 4.8f, 1.5f, 2.8f, 0.12f, 0.24f, 0.08f, 0.22f, 0.1f, 0.32f, -8f, 8f, -2f, 2f, 0.05f, 0.1f, new Color(0.55f, 0.37f, 0.92f, 1f), new Color(0.2f, 0.84f, 0.96f, 1f)),
                    CreateSpawn(BackgroundShape.Streak, false, -220, 0.6f, 1.1f, 0.18f, 0.35f, 1.2f, 2.1f, 1f, 1.5f, 0f, 0.08f, 0.2f, 0.6f, -28f, -10f, -18f, -6f, 0.08f, 0.16f, new Color(0.92f, 0.96f, 1f, 1f), new Color(0.5f, 0.83f, 1f, 1f))
                })
        };
    }

    ThemeProfile CreateTheme(string name, Color topColor, Color bottomColor, SpawnConfig[] spawns)
    {
        ThemeProfile theme = new ThemeProfile();
        theme.name = name;
        theme.topColor = topColor;
        theme.bottomColor = bottomColor;
        theme.spawns = spawns;
        return theme;
    }

    SpawnConfig CreateSpawn(
        BackgroundShape shape,
        bool preferEdges,
        int sortingOrder,
        float intervalMin,
        float intervalMax,
        float widthScaleMin,
        float widthScaleMax,
        float heightScaleMin,
        float heightScaleMax,
        float speedMin,
        float speedMax,
        float swayMin,
        float swayMax,
        float swayFrequencyMin,
        float swayFrequencyMax,
        float rotationMin,
        float rotationMax,
        float angularVelocityMin,
        float angularVelocityMax,
        float alphaMin,
        float alphaMax,
        Color colorA,
        Color colorB)
    {
        SpawnConfig config = new SpawnConfig();
        config.shape = shape;
        config.preferEdges = preferEdges;
        config.sortingOrder = sortingOrder;
        config.intervalMin = intervalMin;
        config.intervalMax = intervalMax;
        config.widthScaleMin = widthScaleMin;
        config.widthScaleMax = widthScaleMax;
        config.heightScaleMin = heightScaleMin;
        config.heightScaleMax = heightScaleMax;
        config.speedMin = speedMin;
        config.speedMax = speedMax;
        config.swayMin = swayMin;
        config.swayMax = swayMax;
        config.swayFrequencyMin = swayFrequencyMin;
        config.swayFrequencyMax = swayFrequencyMax;
        config.rotationMin = rotationMin;
        config.rotationMax = rotationMax;
        config.angularVelocityMin = angularVelocityMin;
        config.angularVelocityMax = angularVelocityMax;
        config.alphaMin = alphaMin;
        config.alphaMax = alphaMax;
        config.colorA = colorA;
        config.colorB = colorB;
        return config;
    }

    Sprite GetShapeSprite(BackgroundShape shape)
    {
        if (spriteCache.TryGetValue(shape, out Sprite cachedSprite) && cachedSprite != null)
            return cachedSprite;

        Sprite createdSprite;

        switch (shape)
        {
            case BackgroundShape.Cloud:
                createdSprite = BuildCloudSprite();
                break;
            case BackgroundShape.Mist:
                createdSprite = BuildMistSprite();
                break;
            case BackgroundShape.Shard:
                createdSprite = BuildShardSprite();
                break;
            case BackgroundShape.Diamond:
                createdSprite = BuildDiamondSprite();
                break;
            default:
                createdSprite = BuildStreakSprite();
                break;
        }

        spriteCache[shape] = createdSprite;
        return createdSprite;
    }

    Sprite CreateGradientSprite(Color topColor, Color bottomColor)
    {
        const int width = 64;
        const int height = 256;
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < height; y++)
        {
            float t = y / (float)(height - 1);
            Color rowColor = Color.Lerp(bottomColor, topColor, t);

            for (int x = 0; x < width; x++)
                texture.SetPixel(x, y, rowColor);
        }

        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 32f);
        sprite.name = "RuntimeBackgroundGradient";
        return sprite;
    }

    Sprite BuildCloudSprite()
    {
        const int size = 96;
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 uv = new Vector2((x / (float)(size - 1) * 2f) - 1f, (y / (float)(size - 1) * 2f) - 1f);
                float alpha = 0f;
                alpha = Mathf.Max(alpha, SoftCircle(uv, new Vector2(-0.4f, 0.02f), 0.45f));
                alpha = Mathf.Max(alpha, SoftCircle(uv, new Vector2(0f, 0.12f), 0.55f));
                alpha = Mathf.Max(alpha, SoftCircle(uv, new Vector2(0.42f, 0f), 0.42f));
                alpha = Mathf.Max(alpha, SoftCircle(uv, new Vector2(0.02f, -0.28f), 0.5f));
                alpha *= Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-0.92f, -0.25f, uv.y));
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 32f);
        sprite.name = "RuntimeCloudSprite";
        return sprite;
    }

    Sprite BuildMistSprite()
    {
        const int size = 96;
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 uv = new Vector2((x / (float)(size - 1) * 2f) - 1f, (y / (float)(size - 1) * 2f) - 1f);
                float radial = Mathf.Clamp01(1f - uv.magnitude);
                float alpha = radial * radial * 0.85f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 32f);
        sprite.name = "RuntimeMistSprite";
        return sprite;
    }

    Sprite BuildDiamondSprite()
    {
        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = Mathf.Abs((x / (float)(size - 1) * 2f) - 1f);
                float ny = Mathf.Abs((y / (float)(size - 1) * 2f) - 1f);
                float distance = nx + ny;
                float alpha = Mathf.Clamp01(1f - distance);
                alpha = alpha * alpha;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 32f);
        sprite.name = "RuntimeDiamondSprite";
        return sprite;
    }

    Sprite BuildStreakSprite()
    {
        const int width = 20;
        const int height = 128;
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float nx = Mathf.Abs((x / (float)(width - 1) * 2f) - 1f);
                float ny = y / (float)(height - 1);
                float core = Mathf.Clamp01(1f - (nx * 3.4f));
                float taper = Mathf.Sin(ny * Mathf.PI);
                float alpha = core * taper;
                alpha = alpha * alpha;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 32f);
        sprite.name = "RuntimeStreakSprite";
        return sprite;
    }

    Sprite BuildShardSprite()
    {
        const int size = 96;
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 uv = new Vector2((x / (float)(size - 1) * 2f) - 1f, (y / (float)(size - 1) * 2f) - 1f);
                float normalizedY = (uv.y + 1f) * 0.5f;
                float centerX = Mathf.Lerp(-0.06f, 0.08f, normalizedY);
                float widthAtY = Mathf.Lerp(0.38f, 0.06f, normalizedY);
                float bottomPoint = Mathf.Clamp01(1f - Mathf.Abs(uv.x) / 0.32f) * Mathf.SmoothStep(-1f, -0.7f, uv.y);
                float body = Mathf.Clamp01(1f - Mathf.Abs(uv.x - centerX) / widthAtY);
                body *= Mathf.SmoothStep(-0.92f, -0.35f, uv.y);
                float topPoint = Mathf.Clamp01(1f - Mathf.Abs(uv.x - centerX) / Mathf.Lerp(0.24f, 0.02f, normalizedY));
                topPoint *= Mathf.SmoothStep(0.2f, 1f, uv.y);
                float alpha = Mathf.Max(body, Mathf.Max(bottomPoint * 0.7f, topPoint));
                alpha = Mathf.Clamp01(alpha);
                alpha *= alpha;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 32f);
        sprite.name = "RuntimeShardSprite";
        return sprite;
    }

    float SoftCircle(Vector2 uv, Vector2 center, float radius)
    {
        float distance = Vector2.Distance(uv, center);
        if (distance >= radius)
            return 0f;

        float normalized = 1f - (distance / radius);
        return normalized * normalized;
    }
}
