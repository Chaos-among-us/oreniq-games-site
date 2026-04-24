using UnityEngine;

public class Coin : MonoBehaviour
{
    public float fallSpeed = 5f;
    public float destroyY = -6f;
    public float magnetMoveSpeed = 14f;

    private static Sprite runtimeCoinSprite;
    private SpriteRenderer spriteRenderer;
    private float spinSeed;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spinSeed = Random.Range(0f, 360f);
        EnsureCircularCoinVisual();
        ConfigureCollectorCollider();
    }

    void Update()
    {
        AnimateCoinVisual();

        float worldSpeedMultiplier = 1f;

        if (GameManager.instance != null)
            worldSpeedMultiplier = GameManager.instance.GetWorldSpeedMultiplier();

        Transform magnetTarget = null;

        if (GameManager.instance != null)
            magnetTarget = GameManager.instance.GetCoinMagnetTarget(transform.position);

        if (magnetTarget != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                magnetTarget.position,
                magnetMoveSpeed * Time.deltaTime);
        }
        else
        {
            transform.Translate(Vector3.down * fallSpeed * worldSpeedMultiplier * Time.deltaTime);
        }

        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }

    void EnsureCircularCoinVisual()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.sprite = GetRuntimeCoinSprite();
        spriteRenderer.color = Color.white;
        spriteRenderer.sortingOrder = 10;
    }

    void ConfigureCollectorCollider()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] == null)
                continue;

            colliders[i].enabled = colliders[i] is CircleCollider2D;
        }

        CircleCollider2D circle = GetComponent<CircleCollider2D>();

        if (circle == null)
            circle = gameObject.AddComponent<CircleCollider2D>();

        circle.enabled = true;
        circle.isTrigger = true;
        circle.offset = Vector2.zero;
        circle.radius = 0.34f;
    }

    void AnimateCoinVisual()
    {
        if (spriteRenderer == null)
            return;

        float pulse = 0.5f + Mathf.Sin(Time.time * 5f + spinSeed) * 0.5f;
        spriteRenderer.color = Color.Lerp(new Color(1f, 0.9f, 0.42f, 1f), Color.white, pulse * 0.28f);
        transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 2.2f + spinSeed) * 9f);
    }

    Sprite GetRuntimeCoinSprite()
    {
        if (runtimeCoinSprite != null)
            return runtimeCoinSprite;

        const int size = 96;
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float glowRadius = size * 0.48f;
        float outerRadius = size * 0.39f;
        float innerRadius = size * 0.27f;
        Vector2 highlightCenter = center + new Vector2(-13f, 13f);
        float highlightRadius = size * 0.13f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);

                if (distance > glowRadius)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                if (distance > outerRadius)
                {
                    float glowAlpha = Mathf.Pow(1f - Mathf.InverseLerp(outerRadius, glowRadius, distance), 2.2f) * 0.28f;
                    texture.SetPixel(x, y, new Color(1f, 0.74f, 0.22f, glowAlpha));
                    continue;
                }

                Color color = Color.Lerp(
                    new Color(0.72f, 0.34f, 0.03f, 1f),
                    new Color(1f, 0.86f, 0.24f, 1f),
                    Mathf.InverseLerp(outerRadius, 0f, distance));

                if (distance > innerRadius)
                {
                    float ringBlend = Mathf.InverseLerp(innerRadius, outerRadius, distance);
                    color = Color.Lerp(color, new Color(0.5f, 0.22f, 0.01f, 1f), ringBlend);
                }
                else
                {
                    float ember = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                    color = Color.Lerp(color, new Color(1f, 0.96f, 0.62f, 1f), ember * 0.16f);
                }

                float highlightDistance = Vector2.Distance(new Vector2(x, y), highlightCenter);

                if (highlightDistance < highlightRadius)
                {
                    float highlightBlend = Mathf.InverseLerp(highlightRadius, 0f, highlightDistance) * 0.55f;
                    color = Color.Lerp(color, new Color(1f, 0.98f, 0.75f, 1f), highlightBlend);
                }

                float diagonal = Mathf.Abs((x - y) / (float)size);

                if (diagonal < 0.045f && distance < innerRadius * 0.9f)
                    color = Color.Lerp(color, Color.white, 0.16f);

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        runtimeCoinSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            size);
        runtimeCoinSprite.name = "RuntimeCoinSpritePolished";
        return runtimeCoinSprite;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.AddCoin();
            Destroy(gameObject);
        }
    }
}
