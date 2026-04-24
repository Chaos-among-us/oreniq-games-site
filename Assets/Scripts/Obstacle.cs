using System;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float baseFallSpeed = 6f;
    public float destroyY = -6f;

    private bool nearMissEvaluated;
    private Collider2D obstacleCollider;
    private SpriteRenderer obstacleRenderer;

    void Awake()
    {
        obstacleRenderer = GetComponent<SpriteRenderer>();
        CaveHazardVisuals.EnsureStyled(gameObject, preferBat: false);
        obstacleCollider = CaveHazardCollisionProfiles.GetActiveCollider(gameObject);
    }

    void Update()
    {
        float speedMultiplier = 1.15f;
        float worldSpeedMultiplier = 1f;

        if (GameManager.instance != null)
        {
            speedMultiplier = GameManager.instance.GetObstacleSpeedRampMultiplier();
            worldSpeedMultiplier = GameManager.instance.GetWorldSpeedMultiplier();
        }

        float finalSpeed = baseFallSpeed * speedMultiplier;

        transform.Translate(Vector3.down * finalSpeed * worldSpeedMultiplier * Time.deltaTime, Space.World);
        TryRegisterNearMiss();

        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }

    void TryRegisterNearMiss()
    {
        if (nearMissEvaluated ||
            GameManager.instance == null ||
            GameManager.instance.player == null ||
            GameManager.instance.player.IsInvulnerable())
        {
            return;
        }

        Bounds obstacleBounds = GetBounds();
        Bounds playerBounds = GameManager.instance.player.GetBounds();

        if (obstacleBounds.max.y > playerBounds.min.y - 0.04f)
            return;

        nearMissEvaluated = true;
        float horizontalEdgeGap = Mathf.Abs(obstacleBounds.center.x - playerBounds.center.x) -
                                  (obstacleBounds.extents.x + playerBounds.extents.x);

        if (horizontalEdgeGap > 0.8f)
            return;

        float closeness = 1f - Mathf.InverseLerp(0.9f, -0.08f, horizontalEdgeGap);
        GameManager.instance.RegisterNearMiss(Mathf.Clamp01(closeness));
    }

    Bounds GetBounds()
    {
        if (obstacleCollider == null || !obstacleCollider.enabled)
            obstacleCollider = CaveHazardCollisionProfiles.GetActiveCollider(gameObject);

        if (obstacleCollider != null)
            return obstacleCollider.bounds;

        if (obstacleRenderer != null)
            return obstacleRenderer.bounds;

        return new Bounds(transform.position, new Vector3(1f, 1f, 0.1f));
    }
}

[RequireComponent(typeof(SpriteRenderer))]
public class CaveHazardVisuals : MonoBehaviour
{
    internal enum HazardStyle
    {
        Rock,
        WideLedge,
        Stalactite,
        CrystalShard,
        Bat
    }

    private static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer hazardGlowRenderer;
    private Vector3 baseScale;
    private bool prefersBat;
    private HazardStyle currentStyle;
    private float animationSeed;
    private int spawnThemeLevel = -1;
    private bool styleApplied;

    public static CaveHazardVisuals EnsureStyled(GameObject target, bool preferBat)
    {
        CaveHazardVisuals visuals = target.GetComponent<CaveHazardVisuals>();

        if (visuals == null)
            visuals = target.AddComponent<CaveHazardVisuals>();

        visuals.Configure(preferBat);
        return visuals;
    }

    public static void PrewarmTheme(int themeLevel)
    {
        RuntimeCaveTheme theme = CaveThemeLibrary.GetThemeForLevel(themeLevel);
        GetOrCreateSprite(theme, HazardStyle.Rock);
        GetOrCreateSprite(theme, HazardStyle.WideLedge);
        GetOrCreateSprite(theme, HazardStyle.Stalactite);
        GetOrCreateSprite(theme, HazardStyle.CrystalShard);
        GetOrCreateSprite(theme, HazardStyle.Bat);
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        EnsureGlowRenderer();
        baseScale = transform.localScale;
        animationSeed = UnityEngine.Random.Range(0f, 10f);
        spawnThemeLevel = Mathf.Max(1, GameManager.instance != null ? GameManager.instance.GetDifficultyLevel() : 1);
    }

    void OnEnable()
    {
        ApplyStyle(forceRefresh: !styleApplied);
    }

    void Update()
    {
        if (currentStyle == HazardStyle.Bat)
        {
            float flap = 0.85f + Mathf.Sin(Time.time * 18f + animationSeed) * 0.18f;
            transform.localScale = new Vector3(baseScale.x * (1f + Mathf.Sin(Time.time * 8f + animationSeed) * 0.06f), baseScale.y * flap, baseScale.z);
            spriteRenderer.flipX = Mathf.Sin(Time.time * 5f + animationSeed) > 0f;

            if (hazardGlowRenderer != null)
                hazardGlowRenderer.flipX = spriteRenderer.flipX;
        }
        else if (currentStyle == HazardStyle.CrystalShard)
        {
            transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Sin(Time.time * 5f + animationSeed) * 6f);
        }
    }

    public void Configure(bool preferBat)
    {
        prefersBat = preferBat;
        ApplyStyle(forceRefresh: true);
    }

    private void ApplyStyle(bool forceRefresh)
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (!forceRefresh && styleApplied && spriteRenderer.sprite != null)
            return;

        if (spawnThemeLevel < 1)
            spawnThemeLevel = Mathf.Max(1, GameManager.instance != null ? GameManager.instance.GetDifficultyLevel() : 1);

        currentStyle = ResolveStyle();
        RuntimeCaveTheme theme = CaveThemeLibrary.GetThemeForLevel(spawnThemeLevel);
        spriteRenderer.sprite = GetOrCreateSprite(theme, currentStyle);
        spriteRenderer.color = ResolveTint(theme, currentStyle);
        spriteRenderer.sortingOrder = currentStyle == HazardStyle.Bat ? 10 : 9;
        RefreshGlow(theme, currentStyle);
        CaveHazardCollisionProfiles.Apply(gameObject, currentStyle);
        styleApplied = true;

        if (currentStyle != HazardStyle.Bat)
            transform.localScale = baseScale;

        if (currentStyle != HazardStyle.CrystalShard)
            transform.localEulerAngles = Vector3.zero;
    }

    private HazardStyle ResolveStyle()
    {
        if (prefersBat || GetComponent<ObstacleZigZag>() != null)
            return HazardStyle.Bat;

        if (baseScale.x >= 2.1f)
            return HazardStyle.WideLedge;

        if (baseScale.y >= 1.35f)
            return HazardStyle.Stalactite;

        if (baseScale.x <= 0.82f || gameObject.name.IndexOf("Small", StringComparison.OrdinalIgnoreCase) >= 0 || gameObject.name.IndexOf("Fast", StringComparison.OrdinalIgnoreCase) >= 0)
            return HazardStyle.CrystalShard;

        return HazardStyle.Rock;
    }

    private static Sprite GetOrCreateSprite(RuntimeCaveTheme theme, HazardStyle style)
    {
        string key = style + "_" + theme.BiomeIndex;

        if (spriteCache.TryGetValue(key, out Sprite cachedSprite) && cachedSprite != null)
            return cachedSprite;

        Sprite sprite;

        switch (style)
        {
            case CaveHazardVisuals.HazardStyle.WideLedge:
                sprite = BuildWideLedgeSprite(theme, key);
                break;
            case CaveHazardVisuals.HazardStyle.Stalactite:
                sprite = BuildStalactiteSprite(theme, key);
                break;
            case CaveHazardVisuals.HazardStyle.CrystalShard:
                sprite = BuildCrystalSprite(theme, key);
                break;
            case CaveHazardVisuals.HazardStyle.Bat:
                sprite = BuildBatSprite(theme, key);
                break;
            default:
                sprite = BuildRockSprite(theme, key);
                break;
        }

        spriteCache[key] = sprite;
        return sprite;
    }

    private Color ResolveTint(RuntimeCaveTheme theme, HazardStyle style)
    {
        switch (style)
        {
            case HazardStyle.Bat:
                return Color.Lerp(Color.white, theme.AccentColor, 0.04f);
            case HazardStyle.CrystalShard:
                return Color.Lerp(Color.white, theme.CrystalColor, 0.04f);
            default:
                return Color.white;
        }
    }

    private void EnsureGlowRenderer()
    {
        Transform existing = transform.Find("HazardReadabilityGlow");
        GameObject glowObject = existing != null ? existing.gameObject : new GameObject("HazardReadabilityGlow");

        if (existing == null)
            glowObject.transform.SetParent(transform, false);

        hazardGlowRenderer = glowObject.GetComponent<SpriteRenderer>();

        if (hazardGlowRenderer == null)
            hazardGlowRenderer = glowObject.AddComponent<SpriteRenderer>();

        hazardGlowRenderer.enabled = true;
    }

    private void RefreshGlow(RuntimeCaveTheme theme, HazardStyle style)
    {
        if (hazardGlowRenderer == null)
            EnsureGlowRenderer();

        if (hazardGlowRenderer == null || spriteRenderer == null)
            return;

        float outlineScale = style == HazardStyle.WideLedge ? 1.16f : (style == HazardStyle.Stalactite ? 1.2f : 1.06f);
        Color glowColor = ResolveGlowTint(theme, style);

        hazardGlowRenderer.sprite = spriteRenderer.sprite;
        hazardGlowRenderer.color = glowColor;
        hazardGlowRenderer.sortingOrder = style == HazardStyle.Bat ? 9 : 8;
        hazardGlowRenderer.transform.localPosition = Vector3.zero;
        hazardGlowRenderer.transform.localRotation = Quaternion.identity;
        hazardGlowRenderer.transform.localScale = new Vector3(outlineScale, outlineScale, 1f);
        hazardGlowRenderer.flipX = spriteRenderer.flipX;
        hazardGlowRenderer.enabled = true;
    }

    private Color ResolveGlowTint(RuntimeCaveTheme theme, HazardStyle style)
    {
        Color color;

        switch (style)
        {
            case HazardStyle.WideLedge:
                color = Color.Lerp(theme.CrystalColor, StudioUiTheme.Gold, 0.58f);
                color.a = 0.62f;
                return color;
            case HazardStyle.Stalactite:
                color = Color.Lerp(theme.CrystalColor, Color.white, 0.55f);
                color.a = 0.64f;
                return color;
            case HazardStyle.CrystalShard:
                color = Color.Lerp(theme.CrystalColor, Color.white, 0.36f);
                color.a = 0.46f;
                return color;
            case HazardStyle.Bat:
                color = Color.Lerp(theme.AccentColor, Color.white, 0.18f);
                color.a = 0.34f;
                return color;
            default:
                color = Color.Lerp(theme.FogColor, StudioUiTheme.Gold, 0.3f);
                color.a = 0.42f;
                return color;
        }
    }

    private static Sprite BuildRockSprite(RuntimeCaveTheme theme, string name)
    {
        const int size = 128;
        Texture2D texture = NewTexture(size, size);
        Color[] pixels = new Color[size * size];
        float seed = theme.Level * 0.71f;

        for (int y = 0; y < size; y++)
        {
            float y01 = y / (size - 1f);

            for (int x = 0; x < size; x++)
            {
                float x01 = x / (size - 1f);
                float centeredX = (x01 * 2f) - 1f;
                float centeredY = (y01 * 2f) - 1f;
                float angle = Mathf.Atan2(centeredY, centeredX);
                float radius = Mathf.Sqrt(centeredX * centeredX + centeredY * centeredY);
                float ridgeNoise = Mathf.PerlinNoise((x + theme.Level * 9f) * 0.09f, (y + theme.Level * 7f) * 0.09f);
                float angularShape = 0.56f
                                     + Mathf.Sin(angle * 3f + seed) * 0.08f
                                     + Mathf.Cos(angle * 5f + seed * 1.4f) * 0.05f
                                     + Mathf.Sin(angle * 7f + seed * 0.7f) * 0.03f;
                angularShape += (ridgeNoise - 0.5f) * 0.12f;
                bool inside = radius < angularShape && centeredY > -0.86f;

                Color color = Color.clear;

                if (inside)
                {
                    float facet = Mathf.Clamp01((angularShape - radius) / 0.18f);
                    float rim = 1f - Smooth01(0.035f, 0.11f, angularShape - radius);
                    float upperLight = Mathf.Clamp01((centeredY + 0.82f) * 0.46f + (1f - Mathf.Abs(centeredX)) * 0.18f);
                    Color stoneBase = Color.Lerp(theme.WallColor, theme.FogColor, 0.72f);
                    Color warmFacet = Color.Lerp(theme.AccentColor, Color.white, 0.42f);
                    color = Color.Lerp(stoneBase, warmFacet, facet * 0.26f + ridgeNoise * 0.12f + upperLight * 0.2f);
                    color = Color.Lerp(color, Color.black, (1f - Smooth01(-0.78f, -0.15f, centeredY)) * 0.08f);
                    color = Color.Lerp(color, theme.CrystalColor, Smooth01(0.86f, 1f, ridgeNoise) * 0.22f);
                    color = Color.Lerp(color, Color.white, rim * 0.24f);
                    color.a = 1f;
                }

                pixels[y * size + x] = color;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return CreateSprite(texture, name);
    }

    private static Sprite BuildWideLedgeSprite(RuntimeCaveTheme theme, string name)
    {
        const int width = 192;
        const int height = 96;
        Texture2D texture = NewTexture(width, height);
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            float y01 = y / (height - 1f);

            for (int x = 0; x < width; x++)
            {
                float topEdge = 0.72f + (Mathf.PerlinNoise((x + theme.Level * 21f) * 0.035f, theme.Level * 0.12f) - 0.5f) * 0.12f;
                float bottomEdge = 0.2f + (Mathf.PerlinNoise((x + theme.Level * 15f) * 0.04f, theme.Level * 0.19f) - 0.5f) * 0.14f;
                bool inside = y01 >= bottomEdge && y01 <= topEdge;
                Color color = Color.clear;

                if (inside)
                {
                    float ridgeNoise = Mathf.PerlinNoise((x + theme.Level * 11f) * 0.07f, (y + theme.Level * 13f) * 0.09f);
                    float topRim = 1f - Smooth01(0.02f, 0.12f, topEdge - y01);
                    float bottomRim = 1f - Smooth01(0.02f, 0.12f, y01 - bottomEdge);
                    float edgeHighlight = Mathf.Max(topRim, bottomRim);
                    Color ledgeBase = Color.Lerp(theme.WallColor, theme.FogColor, 0.82f);
                    Color bevel = Color.Lerp(theme.CrystalColor, Color.white, 0.58f);
                    color = Color.Lerp(ledgeBase, bevel, edgeHighlight * 0.78f + ridgeNoise * 0.14f);
                    color = Color.Lerp(color, StudioUiTheme.Gold, topRim * 0.22f);
                    color = Color.Lerp(color, Color.black, Mathf.Clamp01((0.22f - y01) * 0.06f));
                    color = Color.Lerp(color, theme.CrystalColor, Smooth01(0.88f, 1f, ridgeNoise) * 0.24f);
                    color = Color.Lerp(color, Color.white, 0.08f);
                    color.a = 1f;
                }

                pixels[y * width + x] = color;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return CreateSprite(texture, name);
    }

    private static Sprite BuildStalactiteSprite(RuntimeCaveTheme theme, string name)
    {
        const int width = 112;
        const int height = 160;
        Texture2D texture = NewTexture(width, height);
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            float y01 = y / (height - 1f);

            for (int x = 0; x < width; x++)
            {
                float x01 = x / (width - 1f);
                float centeredX = Mathf.Abs((x01 * 2f) - 1f);
                float widthAtY = Mathf.Lerp(0.48f, 0.06f, y01);
                float jag = (Mathf.PerlinNoise((x + theme.Level * 8f) * 0.09f, (y + theme.Level * 4f) * 0.06f) - 0.5f) * 0.14f;
                bool inside = centeredX < widthAtY + jag;
                Color color = Color.clear;

                if (inside)
                {
                    float shade = Mathf.Clamp01(1f - centeredX / Mathf.Max(0.06f, widthAtY));
                    float rim = 1f - Smooth01(0.02f, 0.12f, (widthAtY + jag) - centeredX);
                    float centerLine = 1f - Smooth01(0.02f, 0.18f, centeredX);
                    Color spikeBase = Color.Lerp(theme.WallColor, theme.FogColor, 0.84f);
                    Color centerLight = Color.Lerp(theme.CrystalColor, Color.white, 0.62f);
                    color = Color.Lerp(spikeBase, centerLight, 0.18f + shade * 0.38f + centerLine * 0.22f + y01 * 0.1f);
                    color = Color.Lerp(color, StudioUiTheme.Gold, centerLine * 0.08f);
                    color = Color.Lerp(color, theme.CrystalColor, Smooth01(0.86f, 1f, shade) * 0.24f);
                    color = Color.Lerp(color, Color.white, rim * 0.34f + 0.08f);
                    color.a = 1f;
                }

                pixels[y * width + x] = color;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return CreateSprite(texture, name);
    }

    private static Sprite BuildCrystalSprite(RuntimeCaveTheme theme, string name)
    {
        const int size = 128;
        Texture2D texture = NewTexture(size, size);
        Color[] pixels = new Color[size * size];
        Vector2 a = new Vector2(size * 0.5f, size * 0.92f);
        Vector2 b = new Vector2(size * 0.18f, size * 0.38f);
        Vector2 c = new Vector2(size * 0.48f, size * 0.08f);
        Vector2 d = new Vector2(size * 0.84f, size * 0.46f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 point = new Vector2(x, y);
                bool inside = PointInTriangle(point, a, b, c) || PointInTriangle(point, a, c, d);
                Color color = Color.clear;

                if (inside)
                {
                    float vertical = y / (size - 1f);
                    float shimmer = Mathf.PerlinNoise((x + theme.Level * 6f) * 0.12f, (y + theme.Level * 3f) * 0.12f);
                    float centerFacet = 1f - Mathf.Clamp01(Mathf.Abs((x / (size - 1f)) - 0.5f) * 2f);
                    color = Color.Lerp(Color.Lerp(theme.CrystalColor, Color.black, 0.1f), Color.white, vertical * 0.38f + shimmer * 0.14f);
                    color = Color.Lerp(color, theme.AccentColor, (1f - centerFacet) * 0.18f);
                    color = Color.Lerp(color, Color.white, Smooth01(0.72f, 1f, centerFacet) * 0.18f);
                    color.a = 1f;
                }

                pixels[y * size + x] = color;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return CreateSprite(texture, name);
    }

    private static Sprite BuildBatSprite(RuntimeCaveTheme theme, string name)
    {
        const int width = 144;
        const int height = 96;
        Texture2D texture = NewTexture(width, height);
        Color[] pixels = new Color[width * height];
        Color body = Color.Lerp(new Color(0.16f, 0.17f, 0.2f, 1f), theme.AccentColor, 0.22f);
        Color wingLight = Color.Lerp(theme.FogColor, theme.CrystalColor, 0.2f);

        for (int y = 0; y < height; y++)
        {
            float y01 = y / (height - 1f);

            for (int x = 0; x < width; x++)
            {
                float x01 = x / (width - 1f);
                float centeredX = (x01 * 2f) - 1f;
                float centeredY = (y01 * 2f) - 1f;
                float wingCurve = 0.55f - Mathf.Abs(centeredX) * 0.42f + Mathf.Sin(Mathf.Abs(centeredX) * Mathf.PI * 3f) * 0.08f;
                bool wing = Mathf.Abs(centeredY + 0.06f) < wingCurve && Mathf.Abs(centeredX) > 0.12f;
                bool bodyMask = centeredX * centeredX * 2.4f + centeredY * centeredY * 4.6f < 0.22f;
                bool earLeft = centeredX > -0.22f && centeredX < -0.06f && centeredY > 0.2f && centeredY < 0.52f;
                bool earRight = centeredX < 0.22f && centeredX > 0.06f && centeredY > 0.2f && centeredY < 0.52f;
                bool eyeLeft = centeredX > -0.12f && centeredX < -0.06f && centeredY > 0.02f && centeredY < 0.08f;
                bool eyeRight = centeredX < 0.12f && centeredX > 0.06f && centeredY > 0.02f && centeredY < 0.08f;
                Color color = Color.clear;

                if (wing || bodyMask || earLeft || earRight)
                {
                    float wingAccent = Mathf.Clamp01((0.78f - Mathf.Abs(centeredX)) * 0.5f + (centeredY + 0.28f) * 0.08f);
                    color = Color.Lerp(body, wingLight, wingAccent * (wing ? 0.14f : 0.08f));
                    color = Color.Lerp(color, Color.black, Mathf.Clamp01((-centeredY - 0.25f) * 0.16f));
                    color.a = 1f;
                }

                if (eyeLeft || eyeRight)
                    color = Color.Lerp(color, new Color(1f, 0.72f, 0.18f, 1f), 0.86f);

                pixels[y * width + x] = color;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return CreateSprite(texture, name);
    }

    private static Texture2D NewTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        return texture;
    }

    private static Sprite CreateSprite(Texture2D texture, string name)
    {
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = name;
        return sprite;
    }

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float area = 0.5f * (-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
        float sign = area < 0f ? -1f : 1f;
        float s = (a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y) * sign;
        float t = (a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y) * sign;
        return s >= 0f && t >= 0f && (s + t) <= 2f * area * sign;
    }

    private static float Smooth01(float from, float to, float value)
    {
        return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(from, to, value));
    }
}

internal static class CaveHazardCollisionProfiles
{
    public static Collider2D GetActiveCollider(GameObject target)
    {
        if (target == null)
            return null;

        Collider2D[] colliders = target.GetComponents<Collider2D>();

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null && colliders[i].enabled)
                return colliders[i];
        }

        return target.GetComponent<Collider2D>();
    }

    internal static void Apply(GameObject target, CaveHazardVisuals.HazardStyle style)
    {
        if (target == null)
            return;

        switch (style)
        {
            case CaveHazardVisuals.HazardStyle.WideLedge:
                ConfigurePolygon(
                    target,
                    new[]
                    {
                        new Vector2(-0.46f, 0.2f),
                        new Vector2(-0.43f, -0.11f),
                        new Vector2(-0.2f, -0.24f),
                        new Vector2(0.18f, -0.22f),
                        new Vector2(0.44f, -0.08f),
                        new Vector2(0.46f, 0.18f),
                        new Vector2(0.16f, 0.25f),
                        new Vector2(-0.18f, 0.25f)
                    });
                break;
            case CaveHazardVisuals.HazardStyle.Stalactite:
                ConfigurePolygon(
                    target,
                    new[]
                    {
                        new Vector2(-0.36f, -0.28f),
                        new Vector2(-0.18f, 0.32f),
                        new Vector2(0f, 0.44f),
                        new Vector2(0.18f, 0.32f),
                        new Vector2(0.36f, -0.28f),
                        new Vector2(0f, -0.46f)
                    });
                break;
            case CaveHazardVisuals.HazardStyle.CrystalShard:
                ConfigurePolygon(
                    target,
                    new[]
                    {
                        new Vector2(0f, 0.38f),
                        new Vector2(-0.27f, -0.08f),
                        new Vector2(-0.02f, -0.37f),
                        new Vector2(0.3f, -0.02f)
                    });
                break;
            case CaveHazardVisuals.HazardStyle.Bat:
                ConfigureCapsule(target, new Vector2(0f, -0.03f), new Vector2(0.52f, 0.24f));
                break;
            default:
                ConfigurePolygon(
                    target,
                    new[]
                    {
                        new Vector2(-0.3f, -0.24f),
                        new Vector2(-0.36f, 0.02f),
                        new Vector2(-0.2f, 0.3f),
                        new Vector2(0.1f, 0.34f),
                        new Vector2(0.34f, 0.14f),
                        new Vector2(0.3f, -0.22f),
                        new Vector2(0.04f, -0.34f)
                    });
                break;
        }
    }

    static void ConfigureCapsule(GameObject target, Vector2 offset, Vector2 size)
    {
        DisableOtherColliders<CapsuleCollider2D>(target);
        CapsuleCollider2D collider = target.GetComponent<CapsuleCollider2D>();

        if (collider == null)
            collider = target.AddComponent<CapsuleCollider2D>();

        collider.enabled = true;
        collider.isTrigger = true;
        collider.offset = offset;
        collider.size = size;
        collider.direction = CapsuleDirection2D.Horizontal;
    }

    static void ConfigurePolygon(GameObject target, Vector2[] points)
    {
        DisableOtherColliders<PolygonCollider2D>(target);
        PolygonCollider2D collider = target.GetComponent<PolygonCollider2D>();

        if (collider == null)
            collider = target.AddComponent<PolygonCollider2D>();

        collider.enabled = true;
        collider.isTrigger = true;
        collider.pathCount = 1;
        collider.SetPath(0, points);
    }

    static void DisableOtherColliders<T>(GameObject target) where T : Collider2D
    {
        Collider2D[] colliders = target.GetComponents<Collider2D>();

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] == null)
                continue;

            colliders[i].enabled = colliders[i] is T;
        }
    }
}
