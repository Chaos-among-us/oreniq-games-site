using UnityEngine;

public class Coin : MonoBehaviour
{
    public float fallSpeed = 5f;
    public float destroyY = -6f;
    public float magnetMoveSpeed = 14f;

    private static Sprite runtimeCoinSprite;

    void Awake()
    {
        EnsureCircularCoinVisual();
    }

    void Update()
    {
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
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            return;

        spriteRenderer.sprite = GetRuntimeCoinSprite();
        spriteRenderer.color = Color.white;
    }

    Sprite GetRuntimeCoinSprite()
    {
        if (runtimeCoinSprite != null)
            return runtimeCoinSprite;

        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float outerRadius = size * 0.45f;
        float innerRadius = size * 0.34f;
        Vector2 highlightCenter = center + new Vector2(-10f, 10f);
        float highlightRadius = size * 0.14f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);

                if (distance > outerRadius)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                Color color = Color.Lerp(
                    new Color(0.76f, 0.42f, 0.02f, 1f),
                    new Color(1f, 0.9f, 0.22f, 1f),
                    Mathf.InverseLerp(outerRadius, 0f, distance));

                if (distance > innerRadius)
                {
                    float ringBlend = Mathf.InverseLerp(innerRadius, outerRadius, distance);
                    color = Color.Lerp(color, new Color(0.58f, 0.28f, 0.01f, 1f), ringBlend);
                }

                float highlightDistance = Vector2.Distance(new Vector2(x, y), highlightCenter);

                if (highlightDistance < highlightRadius)
                {
                    float highlightBlend = Mathf.InverseLerp(highlightRadius, 0f, highlightDistance) * 0.55f;
                    color = Color.Lerp(color, new Color(1f, 0.98f, 0.75f, 1f), highlightBlend);
                }

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        runtimeCoinSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            size);
        runtimeCoinSprite.name = "RuntimeCoinSprite";
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
