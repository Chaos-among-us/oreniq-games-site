using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float baseFallSpeed = 6f;
    public float destroyY = -6f;

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
        float finalSpeed = baseFallSpeed * speedMultiplier;

        transform.Translate(Vector3.down * finalSpeed * worldSpeedMultiplier * Time.deltaTime, Space.World);

        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }
}
