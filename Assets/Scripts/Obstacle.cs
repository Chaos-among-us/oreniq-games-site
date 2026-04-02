using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float baseFallSpeed = 6f;
    public float destroyY = -6f;

    void Update()
    {
        float speedMultiplier = 1f + (GameManager.instance.GetDifficultyLevel() * 0.15f);
        float finalSpeed = baseFallSpeed * speedMultiplier;

        transform.Translate(Vector3.down * finalSpeed * Time.deltaTime, Space.World);

        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.GameOver();
        }
    }
}