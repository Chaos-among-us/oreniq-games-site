using UnityEngine;

public class ObstacleZigZag : MonoBehaviour
{
    public float baseFallSpeed = 5f;
    public float baseHorizontalSpeed = 2f;
    public float frequency = 2f;
    public float destroyY = -6f;

    private float timeOffset;

    void Start()
    {
        // Prevent all zig-zags syncing together
        timeOffset = Random.Range(0f, 10f);
    }

    void Update()
    {
        int difficulty = 1;
        float worldSpeedMultiplier = 1f;

        if (GameManager.instance != null)
        {
            difficulty = GameManager.instance.GetDifficultyLevel();
            worldSpeedMultiplier = GameManager.instance.GetWorldSpeedMultiplier();
        }

        float speedMultiplier = 1f + (difficulty * 0.15f);

        float fallSpeed = baseFallSpeed * speedMultiplier;

        // Limit horizontal scaling so it doesn't become unfair
        float horizontalSpeed = baseHorizontalSpeed * (1f + (difficulty * 0.05f));

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
