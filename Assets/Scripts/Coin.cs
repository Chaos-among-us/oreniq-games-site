using UnityEngine;

public class Coin : MonoBehaviour
{
    public float fallSpeed = 5f;
    public float destroyY = -6f;

    void Update()
    {
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

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