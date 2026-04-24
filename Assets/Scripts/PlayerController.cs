using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;

    public float minX = -4.15f;
    public float maxX = 4.15f;

    public float minY = -6f;
    public float maxY = 3f;
    public float borderPadding = 0.3f;
    public float cameraSidePadding = 0.15f;
    public float cameraBottomPadding = 0.15f;
    public float cameraTopPadding = 1f;
    public float touchDeadzonePixels = 6f;
    public string leftBorderObjectName = "LeftBorder";
    public string rightBorderObjectName = "RightBorder";

    private bool isDead = false;
    private bool isInvulnerable = false;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private Coroutine invulnerabilityRoutine;
    private Rigidbody2D playerBody;
    private Collider2D playerCollider;
    private Camera mainCamera;
    private Transform leftBorderTransform;
    private Transform rightBorderTransform;
    private Vector2 moveInput;
    private float configuredMinX;
    private float configuredMaxX;
    private float configuredMinY;
    private float configuredMaxY;
    private float lastObstacleHitTime = -10f;
    private int cachedScreenWidth;
    private int cachedScreenHeight;
    private int activeTouchFingerId = -1;
    private const float ObstacleHitCooldown = 0.08f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        playerBody = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        configuredMinX = minX;
        configuredMaxX = maxX;
        configuredMinY = minY;
        configuredMaxY = maxY;

        if (GetComponent<CavePlayerVisuals>() == null)
            gameObject.AddComponent<CavePlayerVisuals>();

        CavePlayerVisuals.ApplyCollisionProfile(gameObject);
        playerCollider = CavePlayerVisuals.GetActiveCollider(gameObject);
    }

    void Start()
    {
        CacheScreenSize();
        RefreshMovementBounds();
        ClampInsideBoundsImmediate();
    }

    void Update()
    {
        if (DidScreenSizeChange())
        {
            CacheScreenSize();
            RefreshMovementBounds();
            ClampInsideBoundsImmediate();
        }

        if (isDead)
        {
            moveInput = Vector2.zero;
            ClearTouchDrag();
            return;
        }

        if (UpdateTouchDragMovement())
            return;

        moveInput = ReadMovementInput();

        if (playerBody == null)
            ApplyMovement(Time.deltaTime, false);
    }

    void FixedUpdate()
    {
        if (isDead || playerBody == null)
            return;

        ApplyMovement(Time.fixedDeltaTime, true);
    }

    Vector2 ReadMovementInput()
    {
        float moveX = 0f;
        float moveY = 0f;

#if UNITY_EDITOR || UNITY_STANDALONE
        moveX = Input.GetAxisRaw("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");
#endif

        return new Vector2(moveX, moveY);
    }

    void ApplyMovement(float deltaTime, bool useRigidbody)
    {
        Vector2 position = useRigidbody ? playerBody.position : (Vector2)transform.position;

        float currentSpeed = moveSpeed;

        if (GameManager.instance != null)
            currentSpeed *= GameManager.instance.GetPlayerMoveSpeedMultiplier();

        position.x += moveInput.x * currentSpeed * deltaTime;
        position.y += moveInput.y * currentSpeed * deltaTime;

        position = ClampToBounds(position);

        if (useRigidbody)
        {
            playerBody.MovePosition(position);
        }
        else
        {
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHandleObstacleTrigger(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryHandleObstacleTrigger(other);
    }

    void TryHandleObstacleTrigger(Collider2D other)
    {
        if (isDead || other == null || !other.CompareTag("Obstacle"))
            return;

        if (isInvulnerable)
        {
            Destroy(other.gameObject);
            return;
        }

        if (Time.time < lastObstacleHitTime + ObstacleHitCooldown)
            return;

        lastObstacleHitTime = Time.time;

        if (GameManager.instance != null)
        {
            GameManager.instance.HandlePlayerHit(other.gameObject);
        }
        else
        {
            Die();
        }
    }

    public void Die()
    {
        if (invulnerabilityRoutine != null)
        {
            StopCoroutine(invulnerabilityRoutine);
            invulnerabilityRoutine = null;
        }

        isInvulnerable = false;
        isDead = true;
        moveInput = Vector2.zero;

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
    }

    public void Revive(float invulnerabilityDuration)
    {
        isDead = false;

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        TriggerInvulnerability(invulnerabilityDuration);
    }

    public void SetSizeMultiplier(float multiplier)
    {
        transform.localScale = originalScale * multiplier;
        playerCollider = CavePlayerVisuals.GetActiveCollider(gameObject);
        RefreshMovementBounds();
        ClampInsideBoundsImmediate();
    }

    public bool IsInvulnerable()
    {
        return isInvulnerable;
    }

    public Bounds GetBounds()
    {
        if (playerCollider == null || !playerCollider.enabled)
            playerCollider = CavePlayerVisuals.GetActiveCollider(gameObject);

        if (playerCollider != null && playerCollider.enabled)
            return playerCollider.bounds;

        if (spriteRenderer != null)
            return spriteRenderer.bounds;

        return new Bounds(transform.position, new Vector3(0.8f, 0.8f, 0.1f));
    }

    public void TriggerInvulnerability(float duration)
    {
        lastObstacleHitTime = Time.time;

        if (invulnerabilityRoutine != null)
            StopCoroutine(invulnerabilityRoutine);

        invulnerabilityRoutine = StartCoroutine(InvulnerabilityRoutine(duration));
    }

    IEnumerator InvulnerabilityRoutine(float duration)
    {
        isInvulnerable = true;
        float elapsed = 0f;
        bool visible = true;

        while (elapsed < duration)
        {
            elapsed += 0.1f;
            visible = !visible;

            if (spriteRenderer != null)
                spriteRenderer.enabled = visible;

            yield return new WaitForSeconds(0.1f);
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        isInvulnerable = false;
        invulnerabilityRoutine = null;
    }

    Vector2 ClampToBounds(Vector2 position)
    {
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
        return position;
    }

    void ClampInsideBoundsImmediate()
    {
        Vector2 clampedPosition = ClampToBounds(playerBody != null ? playerBody.position : (Vector2)transform.position);

        if (playerBody != null)
            playerBody.position = clampedPosition;

        transform.position = new Vector3(clampedPosition.x, clampedPosition.y, transform.position.z);
    }

    void RefreshMovementBounds()
    {
        mainCamera = Camera.main != null ? Camera.main : mainCamera;

        if (leftBorderTransform == null)
        {
            GameObject leftBorderObject = GameObject.Find(leftBorderObjectName);

            if (leftBorderObject != null)
                leftBorderTransform = leftBorderObject.transform;
        }

        if (rightBorderTransform == null)
        {
            GameObject rightBorderObject = GameObject.Find(rightBorderObjectName);

            if (rightBorderObject != null)
                rightBorderTransform = rightBorderObject.transform;
        }

        Vector2 halfExtents = GetPlayerHalfExtents();
        float computedMinX = configuredMinX;
        float computedMaxX = configuredMaxX;
        float computedMinY = configuredMinY;
        float computedMaxY = configuredMaxY;

        if (leftBorderTransform != null)
            computedMinX = leftBorderTransform.position.x + halfExtents.x + borderPadding;

        if (rightBorderTransform != null)
            computedMaxX = rightBorderTransform.position.x - halfExtents.x - borderPadding;

        if (mainCamera != null)
        {
            float cameraDistance = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
            Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, cameraDistance));
            Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, cameraDistance));

            computedMinX = Mathf.Max(computedMinX, bottomLeft.x + halfExtents.x + cameraSidePadding);
            computedMaxX = Mathf.Min(computedMaxX, topRight.x - halfExtents.x - cameraSidePadding);
            computedMinY = Mathf.Max(computedMinY, bottomLeft.y + halfExtents.y + cameraBottomPadding);
            computedMaxY = Mathf.Min(computedMaxY, topRight.y - halfExtents.y - cameraTopPadding);
        }

        minX = computedMinX;
        maxX = computedMaxX;
        minY = computedMinY;
        maxY = computedMaxY;
    }

    Vector2 GetPlayerHalfExtents()
    {
        if (playerCollider == null || !playerCollider.enabled)
            playerCollider = CavePlayerVisuals.GetActiveCollider(gameObject);

        if (playerCollider != null && playerCollider.enabled)
            return playerCollider.bounds.extents;

        if (spriteRenderer != null)
            return spriteRenderer.bounds.extents;

        return new Vector2(0.3f, 0.3f);
    }

    bool DidScreenSizeChange()
    {
        return Screen.width != cachedScreenWidth || Screen.height != cachedScreenHeight;
    }

    void CacheScreenSize()
    {
        cachedScreenWidth = Screen.width;
        cachedScreenHeight = Screen.height;
    }

    bool UpdateTouchDragMovement()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        ClearTouchDrag();
        return false;
#else
        if (Input.touchCount <= 0)
        {
            ClearTouchDrag();
            return false;
        }

        if (TryGetTouchByFingerId(activeTouchFingerId, out Touch activeTouch))
        {
            if (activeTouch.phase == TouchPhase.Canceled || activeTouch.phase == TouchPhase.Ended)
            {
                ClearTouchDrag();
                return false;
            }

            ApplyTouchDragDelta(activeTouch);
            return true;
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
                continue;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                continue;

            BeginTouchDrag(touch);
            ApplyTouchDragDelta(touch);
            return true;
        }

        ClearTouchDrag();
        return false;
#endif
    }

    void BeginTouchDrag(Touch touch)
    {
        activeTouchFingerId = touch.fingerId;
    }

    void ApplyTouchDragDelta(Touch touch)
    {
        moveInput = Vector2.zero;

        Camera touchCamera = Camera.main != null ? Camera.main : mainCamera;

        if (touchCamera == null)
            return;

        if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Stationary)
            return;

        if (touch.deltaPosition.sqrMagnitude < touchDeadzonePixels * touchDeadzonePixels)
            return;

        float cameraDistance = Mathf.Abs(touchCamera.transform.position.z - transform.position.z);
        Vector3 previousWorld = touchCamera.ScreenToWorldPoint(
            new Vector3(
                touch.position.x - touch.deltaPosition.x,
                touch.position.y - touch.deltaPosition.y,
                cameraDistance));
        Vector3 currentWorld = touchCamera.ScreenToWorldPoint(
            new Vector3(touch.position.x, touch.position.y, cameraDistance));

        Vector2 worldDelta = currentWorld - previousWorld;
        Vector2 currentPosition = playerBody != null ? playerBody.position : (Vector2)transform.position;
        Vector2 nextPosition = ClampToBounds(currentPosition + worldDelta);

        if (playerBody != null)
            playerBody.position = nextPosition;

        transform.position = new Vector3(nextPosition.x, nextPosition.y, transform.position.z);
    }

    bool TryGetTouchByFingerId(int fingerId, out Touch matchingTouch)
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.fingerId != fingerId)
                continue;

            matchingTouch = touch;
            return true;
        }

        matchingTouch = default;
        return false;
    }

    void ClearTouchDrag()
    {
        activeTouchFingerId = -1;
        moveInput = Vector2.zero;
    }
}

[RequireComponent(typeof(SpriteRenderer))]
public class CavePlayerVisuals : MonoBehaviour
{
    private static Sprite wingsOpenSprite;
    private static Sprite wingsClosedSprite;
    private static Sprite glowSprite;

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer glowRenderer;
    private float animationSeed;
    private bool showingOpenWings = true;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animationSeed = Random.Range(0f, 10f);
        EnsureGlowRenderer();
        ApplyCollisionProfile(gameObject);
        ApplySprite(forceOpen: true);
        spriteRenderer.sortingOrder = 12;
    }

    void Update()
    {
        float flutter = Mathf.Sin(Time.time * 16f + animationSeed);
        bool useOpenWings = flutter > 0f;

        if (useOpenWings != showingOpenWings)
            ApplySprite(useOpenWings);

        float glowPulse = 0.84f + (Mathf.Sin(Time.time * 3.4f + animationSeed) * 0.08f);
        spriteRenderer.color = new Color(glowPulse, glowPulse, glowPulse, 1f);

        if (glowRenderer != null)
        {
            float auraPulse = 0.72f + (Mathf.Sin(Time.time * 2.8f + animationSeed) * 0.1f);
            glowRenderer.enabled = spriteRenderer.enabled;
            glowRenderer.color = new Color(0.42f, 0.92f, 1f, 0.28f + auraPulse * 0.08f);
            glowRenderer.transform.localScale = Vector3.one * (1.08f + auraPulse * 0.08f);
        }
    }

    private void ApplySprite(bool forceOpen)
    {
        showingOpenWings = forceOpen;
        spriteRenderer.sprite = showingOpenWings ? GetWingsOpenSprite() : GetWingsClosedSprite();
        spriteRenderer.color = Color.white;
    }

    private void EnsureGlowRenderer()
    {
        Transform existing = transform.Find("PlayerCoreGlow");
        GameObject glowObject = existing != null ? existing.gameObject : new GameObject("PlayerCoreGlow");

        if (existing == null)
            glowObject.transform.SetParent(transform, false);

        glowRenderer = glowObject.GetComponent<SpriteRenderer>();

        if (glowRenderer == null)
            glowRenderer = glowObject.AddComponent<SpriteRenderer>();

        glowRenderer.sprite = GetGlowSprite();
        glowRenderer.sortingOrder = 11;
        glowRenderer.enabled = true;
    }

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

    public static void ApplyCollisionProfile(GameObject target)
    {
        if (target == null)
            return;

        Collider2D[] colliders = target.GetComponents<Collider2D>();

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] == null)
                continue;

            colliders[i].enabled = colliders[i] is CapsuleCollider2D;
        }

        CapsuleCollider2D capsule = target.GetComponent<CapsuleCollider2D>();

        if (capsule == null)
            capsule = target.AddComponent<CapsuleCollider2D>();

        capsule.enabled = true;
        capsule.isTrigger = true;
        capsule.direction = CapsuleDirection2D.Vertical;
        capsule.offset = new Vector2(0f, -0.02f);
        capsule.size = new Vector2(0.5f, 0.58f);
    }

    private static Sprite GetWingsOpenSprite()
    {
        if (wingsOpenSprite == null)
            wingsOpenSprite = BuildPlayerSprite("LumenRunnerOpen", wingsOpen: true);

        return wingsOpenSprite;
    }

    private static Sprite GetWingsClosedSprite()
    {
        if (wingsClosedSprite == null)
            wingsClosedSprite = BuildPlayerSprite("LumenRunnerClosed", wingsOpen: false);

        return wingsClosedSprite;
    }

    private static Sprite GetGlowSprite()
    {
        if (glowSprite == null)
            glowSprite = BuildGlowSprite();

        return glowSprite;
    }

    private static Sprite BuildPlayerSprite(string name, bool wingsOpen)
    {
        const int size = 160;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 point = new Vector2(x, y);
                Vector2 normalized = (point - center) / size;
                float bodyMask = EllipseMask(normalized, Vector2.zero, 0.11f, 0.18f);
                float coreMask = EllipseMask(normalized, new Vector2(0f, -0.012f), 0.064f, 0.12f);
                float headMask = EllipseMask(normalized, new Vector2(0f, 0.14f), 0.074f, 0.064f);
                float lowerGlow = EllipseMask(normalized, new Vector2(0f, -0.05f), 0.18f, 0.23f);
                float wingSpread = wingsOpen ? 0.27f : 0.18f;
                float wingHeight = wingsOpen ? 0.135f : 0.09f;
                float leftWing = EllipseMask(normalized, new Vector2(-wingSpread, 0.016f), 0.19f, wingHeight);
                float rightWing = EllipseMask(normalized, new Vector2(wingSpread, 0.016f), 0.19f, wingHeight);
                float leftVein = LineMask(normalized, new Vector2(-0.05f, 0.08f), new Vector2(-wingSpread - 0.09f, 0.055f), 0.012f);
                float rightVein = LineMask(normalized, new Vector2(0.05f, 0.08f), new Vector2(wingSpread + 0.09f, 0.055f), 0.012f);
                float antennaLeft = LineMask(normalized, new Vector2(-0.035f, 0.18f), new Vector2(-0.11f, 0.255f), 0.008f);
                float antennaRight = LineMask(normalized, new Vector2(0.035f, 0.18f), new Vector2(0.11f, 0.255f), 0.008f);
                Color color = Color.clear;
                float wingMask = Mathf.Max(leftWing, rightWing);

                if (wingMask > 0f)
                {
                    float wingAlpha = wingMask * (wingsOpen ? 0.68f : 0.46f);
                    Color wingColor = Color.Lerp(new Color(0.42f, 0.92f, 1f, wingAlpha), new Color(0.94f, 1f, 1f, wingAlpha), wingMask * 0.55f);
                    color = Blend(color, wingColor);
                }

                float veinMask = Mathf.Max(leftVein, rightVein);

                if (veinMask > 0f)
                    color = Blend(color, new Color(0.93f, 1f, 1f, 0.28f * veinMask));

                if (lowerGlow > 0f)
                {
                    Color glowColor = Color.Lerp(new Color(0.18f, 0.8f, 1f, 0.1f), new Color(1f, 0.88f, 0.3f, 0.72f), lowerGlow);
                    glowColor.a *= lowerGlow;
                    color = Blend(color, glowColor);
                }

                if (bodyMask > 0f || headMask > 0f)
                {
                    float mask = Mathf.Max(bodyMask, headMask);
                    Color shell = Color.Lerp(new Color(0.13f, 0.11f, 0.08f, 1f), new Color(0.58f, 0.38f, 0.12f, 1f), mask);
                    color = Blend(color, shell);
                }

                if (coreMask > 0f)
                    color = Blend(color, new Color(1f, 0.88f, 0.28f, coreMask));

                float antennaMask = Mathf.Max(antennaLeft, antennaRight);

                if (antennaMask > 0f)
                    color = Blend(color, new Color(0.75f, 0.95f, 1f, antennaMask * 0.82f));

                if (color.a > 0f)
                {
                    float topLight = Mathf.Clamp01((normalized.y + 0.25f) / 0.48f);
                    color = Color.Lerp(color, Color.white, topLight * Mathf.Max(coreMask, wingMask) * 0.12f);
                }

                pixels[y * size + x] = color;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = name;
        return sprite;
    }

    private static Sprite BuildGlowSprite()
    {
        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / (size * 0.5f);
                float alpha = Mathf.Pow(Mathf.Clamp01(1f - distance), 2.4f) * 0.68f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        sprite.name = "PlayerCoreGlow";
        return sprite;
    }

    private static float EllipseMask(Vector2 point, Vector2 center, float halfWidth, float halfHeight)
    {
        float dx = (point.x - center.x) / Mathf.Max(0.001f, halfWidth);
        float dy = (point.y - center.y) / Mathf.Max(0.001f, halfHeight);
        float distance = Mathf.Sqrt((dx * dx) + (dy * dy));
        return 1f - Smooth01(0.82f, 1f, distance);
    }

    private static float LineMask(Vector2 point, Vector2 start, Vector2 end, float width)
    {
        Vector2 line = end - start;
        float lengthSq = Mathf.Max(0.0001f, line.sqrMagnitude);
        float t = Mathf.Clamp01(Vector2.Dot(point - start, line) / lengthSq);
        float distance = Vector2.Distance(point, start + line * t);
        return 1f - Smooth01(width * 0.35f, width, distance);
    }

    private static float Smooth01(float from, float to, float value)
    {
        return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(from, to, value));
    }

    private static Color Blend(Color baseColor, Color overlay)
    {
        if (overlay.a <= 0f)
            return baseColor;

        float outputAlpha = overlay.a + baseColor.a * (1f - overlay.a);

        if (outputAlpha <= 0f)
            return Color.clear;

        Color output = (overlay * overlay.a + baseColor * baseColor.a * (1f - overlay.a)) / outputAlpha;
        output.a = outputAlpha;
        return output;
    }
}
