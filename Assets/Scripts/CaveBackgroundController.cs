using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveBackgroundController : MonoBehaviour
{
    private sealed class ScrollingLayer
    {
        public readonly string Name;
        public readonly float ScrollSpeed;
        public readonly float BaseOpacity;
        public readonly int LayerIndex;
        public readonly Transform Root;
        public readonly SpriteRenderer[] PrimaryTiles;
        public readonly SpriteRenderer[] SecondaryTiles;

        public int PrimaryThemeLevel = -1;
        public int SecondaryThemeLevel = -1;

        public ScrollingLayer(
            string name,
            float scrollSpeed,
            float baseOpacity,
            int layerIndex,
            Transform root,
            SpriteRenderer[] primaryTiles,
            SpriteRenderer[] secondaryTiles)
        {
            Name = name;
            ScrollSpeed = scrollSpeed;
            BaseOpacity = baseOpacity;
            LayerIndex = layerIndex;
            Root = root;
            PrimaryTiles = primaryTiles;
            SecondaryTiles = secondaryTiles;
        }
    }

    private static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    private readonly List<ScrollingLayer> layers = new List<ScrollingLayer>();
    private readonly Queue<int> themePrewarmQueue = new Queue<int>();
    private readonly HashSet<int> queuedThemeLevels = new HashSet<int>();

    private GameManager gameManager;
    private Camera targetCamera;
    private Coroutine themePrewarmRoutine;
    private int appliedBaseLevel = -1;
    private int cachedScreenWidth;
    private int cachedScreenHeight;
    private float cachedOrthographicSize;
    private float tileWorldWidth;
    private float tileWorldHeight;

    void Awake()
    {
        name = "RuntimeCaveBackground";
        EnsureSceneReferences();
        CreateLayersIfNeeded();
    }

    void Start()
    {
        RefreshViewportMetrics(forceRefresh: true);

        int baseLevel = GetBaseThemeLevel();
        WarmThemeImmediate(baseLevel);
        WarmThemeImmediate(baseLevel + CaveThemeLibrary.LevelsPerBiome);
        QueueAllBiomeThemes();
        RefreshThemeSprites(forceRefresh: true);
        QueueThemePrewarm(baseLevel + (CaveThemeLibrary.LevelsPerBiome * 2));
        UpdateThemeBlend();
    }

    void LateUpdate()
    {
        EnsureSceneReferences();
        RefreshViewportMetrics(forceRefresh: false);
        QueueUpcomingThemes();
        RefreshThemeSprites(forceRefresh: false);
        UpdateThemeBlend();
        ScrollLayers();
    }

    private void EnsureSceneReferences()
    {
        if (gameManager == null)
            gameManager = GameManager.instance;

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera != null)
            transform.position = new Vector3(targetCamera.transform.position.x, targetCamera.transform.position.y, 0f);
    }

    private void CreateLayersIfNeeded()
    {
        if (layers.Count > 0)
            return;

        layers.Add(CreateLayer("FarLayer", 0.14f, 0.66f, 0, -82));
        layers.Add(CreateLayer("MidLayer", 0.28f, 0.58f, 1, -72));
        layers.Add(CreateLayer("FrontLayer", 0.46f, 0.48f, 2, -62));
    }

    private ScrollingLayer CreateLayer(string layerName, float scrollSpeed, float baseOpacity, int layerIndex, int sortingOrder)
    {
        GameObject layerRoot = new GameObject(layerName);
        layerRoot.transform.SetParent(transform, false);

        SpriteRenderer[] primaryTiles = CreateTileSet(layerRoot.transform, layerName + "_Current", sortingOrder);
        SpriteRenderer[] secondaryTiles = CreateTileSet(layerRoot.transform, layerName + "_Next", sortingOrder + 1);

        return new ScrollingLayer(
            layerName,
            scrollSpeed,
            baseOpacity,
            layerIndex,
            layerRoot.transform,
            primaryTiles,
            secondaryTiles);
    }

    private SpriteRenderer[] CreateTileSet(Transform parent, string setName, int sortingOrder)
    {
        SpriteRenderer[] tiles = new SpriteRenderer[2];

        for (int i = 0; i < tiles.Length; i++)
        {
            GameObject tileObject = new GameObject(setName + "_Tile" + i);
            tileObject.transform.SetParent(parent, false);

            SpriteRenderer renderer = tileObject.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = sortingOrder;
            tiles[i] = renderer;
        }

        return tiles;
    }

    private void RefreshViewportMetrics(bool forceRefresh)
    {
        if (targetCamera == null || !targetCamera.orthographic)
            return;

        bool viewportChanged = forceRefresh ||
                               cachedScreenWidth != Screen.width ||
                               cachedScreenHeight != Screen.height ||
                               !Mathf.Approximately(cachedOrthographicSize, targetCamera.orthographicSize);

        if (!viewportChanged)
            return;

        cachedScreenWidth = Screen.width;
        cachedScreenHeight = Screen.height;
        cachedOrthographicSize = targetCamera.orthographicSize;

        tileWorldHeight = targetCamera.orthographicSize * 2f * 1.08f;
        tileWorldWidth = tileWorldHeight * targetCamera.aspect * 1.08f;

        for (int i = 0; i < layers.Count; i++)
        {
            AlignTileSet(layers[i].PrimaryTiles);
            AlignTileSet(layers[i].SecondaryTiles);
        }
    }

    private void AlignTileSet(SpriteRenderer[] tiles)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            SpriteRenderer renderer = tiles[i];

            if (renderer == null || renderer.sprite == null)
                continue;

            renderer.transform.localPosition = new Vector3(0f, i == 0 ? 0f : tileWorldHeight, 0f);
            ScaleSpriteToWorldSize(renderer, tileWorldWidth, tileWorldHeight);
        }
    }

    private void QueueUpcomingThemes()
    {
        int baseLevel = GetBaseThemeLevel();
        QueueThemePrewarm(baseLevel + CaveThemeLibrary.LevelsPerBiome);
        QueueThemePrewarm(baseLevel + (CaveThemeLibrary.LevelsPerBiome * 2));
    }

    private void QueueAllBiomeThemes()
    {
        for (int biomeIndex = 0; biomeIndex < 4; biomeIndex++)
            QueueThemePrewarm((biomeIndex * CaveThemeLibrary.LevelsPerBiome) + 1);
    }

    private void QueueThemePrewarm(int themeLevel)
    {
        if (themeLevel < 1 || IsThemeReady(themeLevel) || queuedThemeLevels.Contains(themeLevel))
            return;

        queuedThemeLevels.Add(themeLevel);
        themePrewarmQueue.Enqueue(themeLevel);

        if (themePrewarmRoutine == null)
            themePrewarmRoutine = StartCoroutine(PrewarmQueuedThemes());
    }

    private IEnumerator PrewarmQueuedThemes()
    {
        while (themePrewarmQueue.Count > 0)
        {
            int themeLevel = themePrewarmQueue.Dequeue();
            queuedThemeLevels.Remove(themeLevel);
            yield return StartCoroutine(WarmThemeGradually(themeLevel));
        }

        themePrewarmRoutine = null;
    }

    private IEnumerator WarmThemeGradually(int themeLevel)
    {
        RuntimeCaveTheme theme = CaveThemeLibrary.GetThemeForLevel(themeLevel);

        for (int layerIndex = 0; layerIndex < 3; layerIndex++)
        {
            for (int tileIndex = 0; tileIndex < 2; tileIndex++)
            {
                GetOrCreateLayerSprite(theme, layerIndex, tileIndex);
                yield return null;
            }
        }

        CaveHazardVisuals.PrewarmTheme(themeLevel);
    }

    private void WarmThemeImmediate(int themeLevel)
    {
        RuntimeCaveTheme theme = CaveThemeLibrary.GetThemeForLevel(themeLevel);

        for (int layerIndex = 0; layerIndex < 3; layerIndex++)
        {
            for (int tileIndex = 0; tileIndex < 2; tileIndex++)
                GetOrCreateLayerSprite(theme, layerIndex, tileIndex);
        }

        CaveHazardVisuals.PrewarmTheme(themeLevel);
    }

    private bool IsThemeReady(int themeLevel)
    {
        RuntimeCaveTheme theme = CaveThemeLibrary.GetThemeForLevel(themeLevel);

        for (int layerIndex = 0; layerIndex < 3; layerIndex++)
        {
            for (int tileIndex = 0; tileIndex < 2; tileIndex++)
            {
                if (!spriteCache.ContainsKey(GetSpriteKey(theme, layerIndex, tileIndex)))
                    return false;
            }
        }

        return true;
    }

    private void RefreshThemeSprites(bool forceRefresh)
    {
        int baseLevel = GetBaseThemeLevel();
        int nextLevel = baseLevel + CaveThemeLibrary.LevelsPerBiome;

        if (!forceRefresh && baseLevel == appliedBaseLevel)
            return;

        if (!IsThemeReady(baseLevel) || !IsThemeReady(nextLevel))
            return;

        appliedBaseLevel = baseLevel;

        for (int i = 0; i < layers.Count; i++)
        {
            ScrollingLayer layer = layers[i];
            AssignTileSetSprites(layer, layer.PrimaryTiles, baseLevel, isSecondary: false, forceRefresh);
            AssignTileSetSprites(layer, layer.SecondaryTiles, nextLevel, isSecondary: true, forceRefresh);
        }
    }

    private void AssignTileSetSprites(ScrollingLayer layer, SpriteRenderer[] tiles, int themeLevel, bool isSecondary, bool forceRefresh)
    {
        if (!forceRefresh)
        {
            if (!isSecondary && layer.PrimaryThemeLevel == themeLevel)
                return;

            if (isSecondary && layer.SecondaryThemeLevel == themeLevel)
                return;
        }

        RuntimeCaveTheme theme = CaveThemeLibrary.GetThemeForLevel(themeLevel);

        for (int tileIndex = 0; tileIndex < tiles.Length; tileIndex++)
        {
            SpriteRenderer renderer = tiles[tileIndex];

            if (renderer == null)
                continue;

            renderer.sprite = GetOrCreateLayerSprite(theme, layer.LayerIndex, tileIndex);
            renderer.transform.localPosition = new Vector3(0f, tileIndex == 0 ? 0f : tileWorldHeight, 0f);
            ScaleSpriteToWorldSize(renderer, tileWorldWidth, tileWorldHeight);
        }

        if (isSecondary)
            layer.SecondaryThemeLevel = themeLevel;
        else
            layer.PrimaryThemeLevel = themeLevel;
    }

    private void UpdateThemeBlend()
    {
        int visualBaseLevel = appliedBaseLevel > 0 ? appliedBaseLevel : GetBaseThemeLevel();
        RuntimeCaveTheme currentTheme = CaveThemeLibrary.GetThemeForLevel(visualBaseLevel);
        RuntimeCaveTheme nextTheme = CaveThemeLibrary.GetThemeForLevel(visualBaseLevel + CaveThemeLibrary.LevelsPerBiome);
        float blend = GetBiomeTransitionBlend01();

        if (targetCamera != null)
        {
            Color currentBackground = Color.Lerp(Color.Lerp(currentTheme.BackgroundBottom, currentTheme.WallColor, 0.58f), Color.black, 0.2f);
            Color nextBackground = Color.Lerp(Color.Lerp(nextTheme.BackgroundBottom, nextTheme.WallColor, 0.58f), Color.black, 0.2f);
            targetCamera.backgroundColor = Color.Lerp(currentBackground, nextBackground, blend);
        }

        for (int i = 0; i < layers.Count; i++)
        {
            ScrollingLayer layer = layers[i];
            float currentAlpha = layer.BaseOpacity * (1f - blend);
            float nextAlpha = layer.BaseOpacity * blend;
            SetTileSetAlpha(layer.PrimaryTiles, currentAlpha);
            SetTileSetAlpha(layer.SecondaryTiles, nextAlpha);
        }
    }

    private void SetTileSetAlpha(SpriteRenderer[] tiles, float alpha)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == null)
                continue;

            Color color = tiles[i].color;
            color.a = alpha;
            tiles[i].color = color;
        }
    }

    private void ScrollLayers()
    {
        if (targetCamera == null)
            return;

        transform.position = new Vector3(targetCamera.transform.position.x, targetCamera.transform.position.y, 0f);

        float worldSpeedMultiplier = 1f;

        if (gameManager != null)
            worldSpeedMultiplier = gameManager.GetWorldSpeedMultiplier();

        float wrapThreshold = tileWorldHeight * 1.5f;

        for (int i = 0; i < layers.Count; i++)
        {
            ScrollingLayer layer = layers[i];
            float delta = layer.ScrollSpeed * worldSpeedMultiplier * Time.deltaTime;
            ScrollTileSet(layer.PrimaryTiles, delta, wrapThreshold);
            ScrollTileSet(layer.SecondaryTiles, delta, wrapThreshold);
        }
    }

    private void ScrollTileSet(SpriteRenderer[] tiles, float delta, float wrapThreshold)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            Transform tileTransform = tiles[i].transform;
            tileTransform.localPosition += Vector3.down * delta;

            if (tileTransform.localPosition.y < -wrapThreshold)
                tileTransform.localPosition += Vector3.up * tileWorldHeight * tiles.Length;
        }
    }

    private int GetBaseThemeLevel()
    {
        if (gameManager == null)
            return 1;

        return CaveThemeLibrary.GetBiomeStartLevel(Mathf.Max(1, gameManager.GetDifficultyLevel()));
    }

    private float GetBiomeTransitionBlend01()
    {
        if (gameManager == null)
            return 0f;

        float continuousLevel = Mathf.Max(1f, gameManager.GetContinuousDifficultyLevel());
        float biomeProgress = Mathf.Repeat(continuousLevel - 1f, CaveThemeLibrary.LevelsPerBiome);
        return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(1.8f, 2.95f, biomeProgress));
    }

    private void ScaleSpriteToWorldSize(SpriteRenderer renderer, float worldWidth, float worldHeight)
    {
        if (renderer == null || renderer.sprite == null)
            return;

        Vector2 spriteSize = renderer.sprite.bounds.size;

        if (spriteSize.x <= 0.0001f || spriteSize.y <= 0.0001f)
            return;

        renderer.transform.localScale = new Vector3(worldWidth / spriteSize.x, worldHeight / spriteSize.y, 1f);
    }

    private static Sprite GetOrCreateLayerSprite(RuntimeCaveTheme theme, int layerIndex, int tileIndex)
    {
        string key = GetSpriteKey(theme, layerIndex, tileIndex);

        if (spriteCache.TryGetValue(key, out Sprite cachedSprite) && cachedSprite != null)
            return cachedSprite;

        Sprite sprite = CreateLayerSprite(theme, layerIndex, tileIndex);
        spriteCache[key] = sprite;
        return sprite;
    }

    private static string GetSpriteKey(RuntimeCaveTheme theme, int layerIndex, int tileIndex)
    {
        return "CaveLayer_" + theme.BiomeIndex + "_" + layerIndex + "_" + tileIndex;
    }

    private static Sprite CreateLayerSprite(RuntimeCaveTheme theme, int layerIndex, int tileIndex)
    {
        const int width = 448;
        const int height = 896;

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        Color[] pixels = new Color[width * height];

        float seed = (theme.Level * 97f) + (layerIndex * 43f) + (tileIndex * 13f);
        int columnCount = layerIndex == 0 ? 1 : (layerIndex == 1 ? 2 : 3);
        float[] columnX = new float[columnCount];
        float[] columnHalfWidth = new float[columnCount];
        float[] gapCenter = new float[columnCount];
        float[] gapHalfHeight = new float[columnCount];

        for (int i = 0; i < columnCount; i++)
        {
            columnX[i] = Mathf.Lerp(0.14f, 0.86f, Hash01(seed + i * 17.3f));
            columnHalfWidth[i] = Mathf.Lerp(0.045f, 0.1f, Hash01(seed + i * 29.7f));
            gapCenter[i] = Mathf.Lerp(0.28f, 0.76f, Hash01(seed + i * 41.9f));
            gapHalfHeight[i] = Mathf.Lerp(0.08f, 0.16f, Hash01(seed + i * 53.1f));
        }

        int spikeCount = 6 + layerIndex * 2;
        float[] topSpikeX = new float[spikeCount];
        float[] topSpikeHalfWidth = new float[spikeCount];
        float[] topSpikeHeight = new float[spikeCount];
        float[] bottomSpikeX = new float[spikeCount];
        float[] bottomSpikeHalfWidth = new float[spikeCount];
        float[] bottomSpikeHeight = new float[spikeCount];

        for (int i = 0; i < spikeCount; i++)
        {
            topSpikeX[i] = Hash01(seed + i * 7.1f);
            topSpikeHalfWidth[i] = Mathf.Lerp(0.035f, 0.085f, Hash01(seed + i * 9.4f));
            topSpikeHeight[i] = Mathf.Lerp(0.06f, 0.19f, Hash01(seed + i * 11.2f));
            bottomSpikeX[i] = Hash01(seed + i * 12.5f);
            bottomSpikeHalfWidth[i] = Mathf.Lerp(0.035f, 0.085f, Hash01(seed + i * 15.2f));
            bottomSpikeHeight[i] = Mathf.Lerp(0.05f, 0.15f, Hash01(seed + i * 18.7f));
        }

        Color deepVoid = Color.Lerp(theme.BackgroundBottom, theme.WallColor, 0.34f);
        Color upperVoid = Color.Lerp(theme.BackgroundTop, theme.FogColor, 0.06f);

        for (int y = 0; y < height; y++)
        {
            float y01 = y / (height - 1f);
            float tunnelHalfWidth = 0.19f + layerIndex * 0.075f;
            tunnelHalfWidth += (Mathf.PerlinNoise(seed * 0.014f, y * 0.0085f + seed * 0.004f) - 0.5f) * 0.18f;
            tunnelHalfWidth += Mathf.Sin((y01 * (2.4f + layerIndex * 0.4f) + seed * 0.01f) * Mathf.PI * 2f) * 0.028f;
            tunnelHalfWidth = Mathf.Clamp(tunnelHalfWidth, 0.15f, 0.44f);

            for (int x = 0; x < width; x++)
            {
                float x01 = x / (width - 1f);
                float centeredX = Mathf.Abs((x01 * 2f) - 1f);
                float sideNoise = (Mathf.PerlinNoise((x + seed * 1.2f) * 0.019f, (y + seed * 0.8f) * 0.018f) - 0.5f) * 0.075f;
                float topRidge = 0.08f + (Mathf.PerlinNoise((x + seed * 2.1f) * 0.016f, seed * 0.021f) - 0.5f) * 0.07f;
                float bottomRidge = 0.07f + (Mathf.PerlinNoise((x + seed * 1.4f) * 0.014f, seed * 0.017f) - 0.5f) * 0.06f;
                float extraTop = ComputeSpikeDepth(x01, topSpikeX, topSpikeHalfWidth, topSpikeHeight);
                float extraBottom = ComputeSpikeDepth(x01, bottomSpikeX, bottomSpikeHalfWidth, bottomSpikeHeight);
                bool rockMask = centeredX > tunnelHalfWidth + sideNoise;
                rockMask |= y01 > 1f - (topRidge + extraTop);
                rockMask |= y01 < (bottomRidge + extraBottom);

                if (!rockMask && layerIndex > 0)
                {
                    for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                    {
                        float xDistance = Mathf.Abs(x01 - columnX[columnIndex]);

                        if (xDistance > columnHalfWidth[columnIndex])
                            continue;

                        if (Mathf.Abs(y01 - gapCenter[columnIndex]) > gapHalfHeight[columnIndex])
                        {
                            rockMask = true;
                            break;
                        }
                    }
                }

                Color pixel = Color.Lerp(deepVoid, upperVoid, Mathf.Pow(y01, 1.18f) * 0.68f);

                if (rockMask)
                {
                    float rockNoise = Mathf.PerlinNoise((x + seed * 0.9f) * 0.061f, (y + seed * 0.41f) * 0.056f);
                    float edgeLight = Mathf.Max(
                        Mathf.Clamp01((centeredX - tunnelHalfWidth) / 0.05f),
                        Mathf.Clamp01((y01 - (1f - (topRidge + extraTop))) / 0.07f),
                        Mathf.Clamp01(((bottomRidge + extraBottom) - y01) / 0.07f));
                    float crystalSpark = Mathf.SmoothStep(0.93f, 1f, rockNoise) * (layerIndex == 2 ? 0.15f : 0.08f);

                    pixel = Color.Lerp(Color.Lerp(theme.WallColor, theme.FogColor, 0.12f), theme.AccentColor, rockNoise * 0.08f + edgeLight * 0.22f);
                    pixel = Color.Lerp(pixel, theme.CrystalColor, crystalSpark);
                    pixel.a = 1f;
                }
                else if (layerIndex > 0)
                {
                    float mistNoise = Mathf.PerlinNoise((x + seed * 3.1f) * 0.034f, (y + seed * 2.7f) * 0.028f);
                    float lightColumn = Mathf.SmoothStep(0.18f, 0.04f, centeredX) * Mathf.SmoothStep(0.4f, 1f, y01) * 0.11f;
                    pixel = Color.Lerp(pixel, theme.FogColor, mistNoise * 0.03f + lightColumn);

                    if (layerIndex == 2)
                    {
                        float moteNoise = Mathf.PerlinNoise((x + seed * 4.2f) * 0.075f, (y + seed * 3.7f) * 0.075f);

                        if (moteNoise > 0.95f)
                            pixel = Color.Lerp(pixel, theme.CrystalColor, Mathf.InverseLerp(0.95f, 1f, moteNoise) * 0.26f);
                    }

                    pixel.a = 1f;
                }

                pixels[y * width + x] = pixel;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, width, height),
            new Vector2(0.5f, 0.5f),
            100f);
        sprite.name = GetSpriteKey(theme, layerIndex, tileIndex);
        return sprite;
    }

    private static float ComputeSpikeDepth(float x01, float[] spikeX, float[] halfWidths, float[] heights)
    {
        float depth = 0f;

        for (int i = 0; i < spikeX.Length; i++)
        {
            float distance = Mathf.Abs(x01 - spikeX[i]);

            if (distance >= halfWidths[i])
                continue;

            float normalized = 1f - distance / halfWidths[i];
            depth = Mathf.Max(depth, heights[i] * normalized * normalized);
        }

        return depth;
    }

    private static float Hash01(float value)
    {
        return Mathf.Repeat(Mathf.Sin(value * 12.9898f + 78.233f) * 43758.5453f, 1f);
    }
}
