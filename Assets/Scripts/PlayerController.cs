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
        playerCollider = GetComponent<Collider2D>();
        mainCamera = Camera.main;

        configuredMinX = minX;
        configuredMaxX = maxX;
        configuredMinY = minY;
        configuredMaxY = maxY;
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
        RefreshMovementBounds();
        ClampInsideBoundsImmediate();
    }

    public bool IsInvulnerable()
    {
        return isInvulnerable;
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
        if (playerCollider != null)
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
