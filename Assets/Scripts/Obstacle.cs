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
        spriteRenderer.sortingOrder = currentStyle == HazardStyle.Bat ? 8 : 6;
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
                return Color.Lerp(new Color(0.28f, 0.29f, 0.33f, 1f), theme.AccentColor, 0.24f);
            case HazardStyle.CrystalShard:
                return Color.Lerp(theme.CrystalColor, Color.white, 0.18f);
            default:
                return Color.Lerp(theme.WallColor, Color.white, 0.34f);
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
                    Color stoneBase = Color.Lerp(theme.WallColor, theme.FogColor, 0.24f);
                    color = Color.Lerp(stoneBase, theme.AccentColor, facet * 0.1f + ridgeNoise * 0.06f);
                    color = Color.Lerp(color, theme.CrystalColor, Mathf.SmoothStep(0.95f, 1f, ridgeNoise) * 0.08f);
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
                    float edgeHighlight = Mathf.Max(
                        Mathf.Clamp01((topEdge - y01) / 0.12f),
                        Mathf.Clamp01((y01 - bottomEdge) / 0.12f));
                    Color ledgeBase = Color.Lerp(theme.WallColor, theme.FogColor, 0.2f);
                    color = Color.Lerp(ledgeBase, theme.AccentColor, edgeHighlight * 0.18f + ridgeNoise * 0.08f);
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
                    Color spikeBase = Color.Lerp(theme.WallColor, theme.FogColor, 0.18f);
                    color = Color.Lerp(spikeBase, theme.AccentColor, shade * 0.16f + y01 * 0.09f);
                    color = Color.Lerp(color, theme.CrystalColor, Mathf.SmoothStep(0.9f, 1f, shade) * 0.12f);
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
                    color = Color.Lerp(theme.CrystalColor, Color.white, vertical * 0.48f + shimmer * 0.16f);
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
        Color body = Color.Lerp(new Color(0.15f, 0.16f, 0.19f, 1f), theme.AccentColor, 0.14f);

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
                Color color = Color.clear;

                if (wing || bodyMask || earLeft || earRight)
                {
                    color = body;
                    color.a = 1f;
                }

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
        texture.filterMode = FilterMode.Point;
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
                        new Vector2(-0.42f, 0.17f),
                        new Vector2(-0.38f, -0.14f),
                        new Vector2(-0.14f, -0.25f),
                        new Vector2(0.18f, -0.23f),
                        new Vector2(0.42f, -0.08f),
                        new Vector2(0.38f, 0.2f),
                        new Vector2(0.1f, 0.25f),
                        new Vector2(-0.2f, 0.23f)
                    });
                break;
            case CaveHazardVisuals.HazardStyle.Stalactite:
                ConfigurePolygon(
                    target,
                    new[]
                    {
                        new Vector2(-0.34f, -0.28f),
                        new Vector2(-0.12f, 0.34f),
                        new Vector2(0f, 0.42f),
                        new Vector2(0.12f, 0.34f),
                        new Vector2(0.34f, -0.28f),
                        new Vector2(0f, -0.42f)
                    });
                break;
            case CaveHazardVisuals.HazardStyle.CrystalShard:
                ConfigurePolygon(
                    target,
                    new[]
                    {
                        new Vector2(0f, 0.34f),
                        new Vector2(-0.24f, -0.08f),
                        new Vector2(-0.02f, -0.34f),
                        new Vector2(0.26f, -0.02f)
                    });
                break;
            case CaveHazardVisuals.HazardStyle.Bat:
                ConfigureCapsule(target, new Vector2(0f, -0.02f), new Vector2(0.46f, 0.22f));
                break;
            default:
                ConfigurePolygon(
                    target,
                    new[]
                    {
                        new Vector2(-0.34f, -0.18f),
                        new Vector2(-0.28f, 0.14f),
                        new Vector2(-0.06f, 0.34f),
                        new Vector2(0.18f, 0.3f),
                        new Vector2(0.34f, 0.04f),
                        new Vector2(0.28f, -0.22f),
                        new Vector2(0.04f, -0.36f),
                        new Vector2(-0.2f, -0.3f)
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
