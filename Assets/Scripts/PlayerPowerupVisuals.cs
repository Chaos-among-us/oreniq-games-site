using UnityEngine;

public class PlayerPowerupVisuals : MonoBehaviour
{
    private GameManager gameManager;
    private PlayerController playerController;
    private SpriteRenderer shieldRing;
    private SpriteRenderer magnetRing;
    private SpriteRenderer slowTimeRing;
    private SpriteRenderer auraGlow;
    private SpriteRenderer speedTrailA;
    private SpriteRenderer speedTrailB;
    private SpriteRenderer sparkleA;
    private SpriteRenderer sparkleB;
    private SpriteRenderer burstRing;
    private Vector3 previousPosition;
    private Vector3 smoothedMotionDirection = Vector3.up;
    private float burstEndTime;
    private Color burstColor = Color.clear;

    private static Sprite ringSprite;
    private static Sprite glowSprite;
    private static Sprite sparkSprite;
    private static Sprite trailSprite;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        previousPosition = transform.position;
        EnsureVisualObjects();
    }

    void LateUpdate()
    {
        if (gameManager == null)
            gameManager = GameManager.instance;

        UpdateMotionDirection();
        UpdatePersistentVisuals();
        UpdateBurstVisual();
    }

    public void PlayUpgradeBurst(UpgradeType type)
    {
        burstColor = GetUpgradeColor(type);
        burstEndTime = Time.time + 0.42f;
    }

    public void PlayShieldBlockBurst()
    {
        burstColor = new Color(0.4f, 0.86f, 1f, 1f);
        burstEndTime = Time.time + 0.34f;
    }

    public void PlayBombBurst()
    {
        burstColor = new Color(1f, 0.38f, 0.18f, 1f);
        burstEndTime = Time.time + 0.48f;
    }

    private void EnsureVisualObjects()
    {
        shieldRing = CreateOrFindRenderer("ShieldRing", GetRingSprite(), 30);
        magnetRing = CreateOrFindRenderer("MagnetRing", GetRingSprite(), 31);
        slowTimeRing = CreateOrFindRenderer("SlowTimeRing", GetRingSprite(), 32);
        auraGlow = CreateOrFindRenderer("AuraGlow", GetGlowSprite(), 29);
        speedTrailA = CreateOrFindRenderer("SpeedTrailA", GetTrailSprite(), 27);
        speedTrailB = CreateOrFindRenderer("SpeedTrailB", GetTrailSprite(), 27);
        sparkleA = CreateOrFindRenderer("SparkleA", GetSparkSprite(), 33);
        sparkleB = CreateOrFindRenderer("SparkleB", GetSparkSprite(), 33);
        burstRing = CreateOrFindRenderer("BurstRing", GetRingSprite(), 34);
    }

    private SpriteRenderer CreateOrFindRenderer(string objectName, Sprite sprite, int sortingOrder)
    {
        Transform existing = transform.Find(objectName);
        GameObject visualObject;

        if (existing != null)
        {
            visualObject = existing.gameObject;
        }
        else
        {
            visualObject = new GameObject(objectName);
            visualObject.transform.SetParent(transform, false);
        }

        SpriteRenderer renderer = visualObject.GetComponent<SpriteRenderer>();

        if (renderer == null)
            renderer = visualObject.AddComponent<SpriteRenderer>();

        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
        renderer.enabled = false;
        return renderer;
    }

    private void UpdateMotionDirection()
    {
        Vector3 motion = transform.position - previousPosition;
        previousPosition = transform.position;

        if (motion.sqrMagnitude > 0.00002f)
            smoothedMotionDirection = Vector3.Lerp(smoothedMotionDirection, motion.normalized, 0.35f);
    }

    private void UpdatePersistentVisuals()
    {
        if (gameManager == null)
            return;

        float pulse = 0.5f + Mathf.Sin(Time.time * 4.2f) * 0.5f;
        float slowPulse = 0.5f + Mathf.Sin(Time.time * 2.1f) * 0.5f;
        bool shieldActive = gameManager.activeShields > 0 || (playerController != null && playerController.IsInvulnerable());
        bool magnetActive = gameManager.IsUpgradeActive(UpgradeType.CoinMagnet);
        bool slowTimeActive = gameManager.IsUpgradeActive(UpgradeType.SlowTime);
        bool speedActive = gameManager.IsUpgradeActive(UpgradeType.SpeedBoost);
        bool doubleCoinsActive = gameManager.IsUpgradeActive(UpgradeType.DoubleCoins);
        bool scoreBoostActive = gameManager.IsUpgradeActive(UpgradeType.ScoreBooster);
        bool rareCoinActive = gameManager.IsUpgradeActive(UpgradeType.RareCoinBoost);
        bool smallerActive = gameManager.IsUpgradeActive(UpgradeType.SmallerPlayer);
        bool extraLifeArmed = gameManager.armedExtraLives > 0;

        SetVisualState(
            shieldRing,
            shieldActive,
            new Color(0.38f, 0.86f, 1f, 0.48f + pulse * 0.24f),
            Vector3.zero,
            Vector3.one * (1.65f + pulse * 0.08f),
            0f);

        SetVisualState(
            magnetRing,
            magnetActive,
            new Color(1f, 0.82f, 0.28f, 0.24f + pulse * 0.15f),
            Vector3.zero,
            Vector3.one * (2.2f + pulse * 0.12f),
            0f);

        SetVisualState(
            slowTimeRing,
            slowTimeActive,
            new Color(0.66f, 0.58f, 1f, 0.24f + slowPulse * 0.18f),
            Vector3.zero,
            Vector3.one * (1.95f + slowPulse * 0.18f),
            Time.time * 18f);

        Color auraColor = Color.clear;
        float auraAlpha = 0f;

        if (extraLifeArmed)
        {
            auraColor += new Color(1f, 0.78f, 0.4f, 1f);
            auraAlpha += 0.22f;
        }

        if (doubleCoinsActive)
        {
            auraColor += new Color(1f, 0.88f, 0.35f, 1f);
            auraAlpha += 0.2f;
        }

        if (scoreBoostActive)
        {
            auraColor += new Color(0.44f, 1f, 0.48f, 1f);
            auraAlpha += 0.18f;
        }

        if (smallerActive)
        {
            auraColor += new Color(0.94f, 0.97f, 1f, 1f);
            auraAlpha += 0.12f;
        }

        if (auraAlpha > 0.001f)
            auraColor /= Mathf.Max(1f, auraAlpha / 0.18f);

        SetVisualState(
            auraGlow,
            auraAlpha > 0.001f,
            new Color(auraColor.r, auraColor.g, auraColor.b, 0.18f + slowPulse * 0.08f + auraAlpha * 0.22f),
            Vector3.zero,
            Vector3.one * (1.38f + slowPulse * 0.08f),
            0f);

        if (speedActive)
        {
            Vector3 trailOffset = -smoothedMotionDirection.normalized * 0.28f;
            Vector3 trailScale = new Vector3(0.55f, 0.95f + pulse * 0.2f, 1f);
            float trailAngle = Mathf.Atan2(smoothedMotionDirection.y, smoothedMotionDirection.x) * Mathf.Rad2Deg - 90f;

            SetVisualState(
                speedTrailA,
                true,
                new Color(0.42f, 0.88f, 1f, 0.18f + pulse * 0.12f),
                trailOffset + new Vector3(-0.14f, -0.04f, 0f),
                trailScale,
                trailAngle);

            SetVisualState(
                speedTrailB,
                true,
                new Color(0.42f, 0.88f, 1f, 0.12f + pulse * 0.1f),
                trailOffset + new Vector3(0.14f, -0.02f, 0f),
                trailScale * 0.88f,
                trailAngle);
        }
        else
        {
            speedTrailA.enabled = false;
            speedTrailB.enabled = false;
        }

        bool sparkleActive = doubleCoinsActive || rareCoinActive || scoreBoostActive;

        if (sparkleActive)
        {
            Color sparkleColor = scoreBoostActive
                ? new Color(0.46f, 1f, 0.52f, 0.92f)
                : rareCoinActive
                    ? new Color(1f, 0.66f, 0.2f, 0.92f)
                    : new Color(1f, 0.88f, 0.32f, 0.92f);

            float orbitRadius = 0.74f + pulse * 0.06f;
            Vector3 sparkleOffsetA = new Vector3(
                Mathf.Cos(Time.time * 2.7f) * orbitRadius,
                Mathf.Sin(Time.time * 2.7f) * orbitRadius,
                0f);
            Vector3 sparkleOffsetB = new Vector3(
                Mathf.Cos(Time.time * 2.7f + Mathf.PI) * orbitRadius,
                Mathf.Sin(Time.time * 2.7f + Mathf.PI) * orbitRadius,
                0f);

            SetVisualState(sparkleA, true, sparkleColor, sparkleOffsetA, Vector3.one * 0.28f, Time.time * 120f);
            SetVisualState(sparkleB, true, sparkleColor, sparkleOffsetB, Vector3.one * 0.22f, -Time.time * 140f);
        }
        else
        {
            sparkleA.enabled = false;
            sparkleB.enabled = false;
        }
    }

    private void UpdateBurstVisual()
    {
        if (burstRing == null)
            return;

        if (Time.time >= burstEndTime)
        {
            burstRing.enabled = false;
            return;
        }

        float normalized = 1f - Mathf.Clamp01((burstEndTime - Time.time) / 0.48f);
        float alpha = Mathf.Lerp(0.42f, 0f, normalized);
        float scale = Mathf.Lerp(1.2f, 2.6f, normalized);

        SetVisualState(
            burstRing,
            true,
            new Color(burstColor.r, burstColor.g, burstColor.b, alpha),
            Vector3.zero,
            Vector3.one * scale,
            Time.time * 90f);
    }

    private void SetVisualState(
        SpriteRenderer renderer,
        bool isVisible,
        Color color,
        Vector3 localPosition,
        Vector3 localScale,
        float zRotation)
    {
        if (renderer == null)
            return;

        renderer.enabled = isVisible;

        if (!isVisible)
            return;

        renderer.color = color;
        renderer.transform.localPosition = localPosition;
        renderer.transform.localScale = localScale;
        renderer.transform.localRotation = Quaternion.Euler(0f, 0f, zRotation);
    }

    private Color GetUpgradeColor(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Shield:
                return new Color(0.38f, 0.86f, 1f, 1f);
            case UpgradeType.ExtraLife:
                return new Color(1f, 0.8f, 0.42f, 1f);
            case UpgradeType.SpeedBoost:
                return new Color(0.38f, 0.94f, 1f, 1f);
            case UpgradeType.CoinMagnet:
                return new Color(1f, 0.82f, 0.32f, 1f);
            case UpgradeType.DoubleCoins:
                return new Color(1f, 0.9f, 0.3f, 1f);
            case UpgradeType.SlowTime:
                return new Color(0.68f, 0.58f, 1f, 1f);
            case UpgradeType.SmallerPlayer:
                return new Color(0.92f, 0.97f, 1f, 1f);
            case UpgradeType.ScoreBooster:
                return new Color(0.46f, 1f, 0.52f, 1f);
            case UpgradeType.Bomb:
                return new Color(1f, 0.36f, 0.18f, 1f);
            case UpgradeType.RareCoinBoost:
                return new Color(1f, 0.62f, 0.18f, 1f);
            default:
                return Color.white;
        }
    }

    private static Sprite GetRingSprite()
    {
        if (ringSprite != null)
            return ringSprite;

        const int size = 96;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float outerRadius = size * 0.46f;
        float innerRadius = size * 0.34f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);

                if (distance > outerRadius || distance < innerRadius)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float alpha = Mathf.InverseLerp(outerRadius, innerRadius, distance);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        ringSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        ringSprite.name = "RuntimeRingSprite";
        return ringSprite;
    }

    private static Sprite GetGlowSprite()
    {
        if (glowSprite != null)
            return glowSprite;

        const int size = 96;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.44f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);

                if (distance > radius)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float alpha = Mathf.Pow(1f - Mathf.InverseLerp(0f, radius, distance), 1.8f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        glowSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        glowSprite.name = "RuntimeGlowSprite";
        return glowSprite;
    }

    private static Sprite GetSparkSprite()
    {
        if (sparkSprite != null)
            return sparkSprite;

        const int size = 48;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.18f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 point = new Vector2(x, y);
                float diamond = Mathf.Abs(point.x - center.x) + Mathf.Abs(point.y - center.y);

                if (diamond > radius * 3.2f)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float alpha = 1f - Mathf.InverseLerp(0f, radius * 3.2f, diamond);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        sparkSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        sparkSprite.name = "RuntimeSparkSprite";
        return sparkSprite;
    }

    private static Sprite GetTrailSprite()
    {
        if (trailSprite != null)
            return trailSprite;

        const int width = 48;
        const int height = 96;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2((width - 1) * 0.5f, (height - 1) * 0.5f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float horizontal = Mathf.Abs(x - center.x) / (width * 0.5f);
                float vertical = Mathf.Abs(y - center.y) / (height * 0.5f);
                float ellipse = (horizontal * horizontal) + (vertical * vertical);

                if (ellipse > 1f)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float alpha = Mathf.Pow(1f - ellipse, 1.4f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        trailSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), height);
        trailSprite.name = "RuntimeTrailSprite";
        return trailSprite;
    }
}
