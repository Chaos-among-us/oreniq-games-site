using UnityEngine;

public class ObstacleZigZag : MonoBehaviour
{
    public float baseFallSpeed = 5f;
    public float baseHorizontalSpeed = 2f;
    public float frequency = 2f;
    public float destroyY = -6f;

    private float timeOffset;
    private bool nearMissEvaluated;
    private Collider2D obstacleCollider;
    private SpriteRenderer obstacleRenderer;

    void Awake()
    {
        obstacleRenderer = GetComponent<SpriteRenderer>();
        CaveHazardVisuals.EnsureStyled(gameObject, preferBat: true);
        obstacleCollider = CaveHazardCollisionProfiles.GetActiveCollider(gameObject);
    }

    void Start()
    {
        // Prevent all zig-zags syncing together
        timeOffset = Random.Range(0f, 10f);
    }

    void Update()
    {
        float speedMultiplier = 1.15f;
        float horizontalScale = 1.05f;
        float worldSpeedMultiplier = 1f;

        if (GameManager.instance != null)
        {
            speedMultiplier = GameManager.instance.GetObstacleSpeedRampMultiplier();
            horizontalScale = 1f + GameManager.instance.GetContinuousDifficultyLevel() * 0.05f;
            worldSpeedMultiplier = GameManager.instance.GetWorldSpeedMultiplier();
        }

        float fallSpeed = baseFallSpeed * speedMultiplier;

        // Limit horizontal scaling so it doesn't become unfair
        float horizontalSpeed = baseHorizontalSpeed * horizontalScale;

        float xOffset = Mathf.Sin((Time.time + timeOffset) * frequency) * horizontalSpeed;

        transform.position = new Vector3(
            transform.position.x + xOffset * worldSpeedMultiplier * Time.deltaTime,
            transform.position.y - fallSpeed * worldSpeedMultiplier * Time.deltaTime,
            0f
        );

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

        if (horizontalEdgeGap > 0.94f)
            return;

        float closeness = 1f - Mathf.InverseLerp(1.04f, -0.08f, horizontalEdgeGap);
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
