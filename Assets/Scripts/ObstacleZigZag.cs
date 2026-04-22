using UnityEngine;

public class ObstacleZigZag : MonoBehaviour
{
    public float baseFallSpeed = 5f;
    public float baseHorizontalSpeed = 2f;
    public float frequency = 2f;
    public float destroyY = -6f;

    private float timeOffset;

    void Awake()
    {
        CaveHazardVisuals.EnsureStyled(gameObject, preferBat: true);
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

        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }
}
