using UnityEngine;

public class Coin : MonoBehaviour
{
    public float fallSpeed = 5f;
    public float destroyY = -6f;
    public float magnetMoveSpeed = 14f;

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.AddCoin();
            Destroy(gameObject);
        }
    }
}
