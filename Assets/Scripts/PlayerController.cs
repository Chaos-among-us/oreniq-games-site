using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;

    public float minX = -4.15f;
    public float maxX = 4.15f;

    public float minY = -6f;
    public float maxY = 3f;

    private bool isDead = false;

    void Update()
    {
        if (isDead) return;

        float moveX = 0f;
        float moveY = 0f;

#if UNITY_EDITOR || UNITY_STANDALONE
        moveX = Input.GetAxisRaw("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchPos = Camera.main.ScreenToWorldPoint(touch.position);
            Vector3 direction = touchPos - transform.position;
            moveX = Mathf.Sign(direction.x);
            moveY = Mathf.Sign(direction.y);
        }
#endif

        Vector3 position = transform.position;

        position.x += moveX * moveSpeed * Time.deltaTime;
        position.y += moveY * moveSpeed * Time.deltaTime;

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);

        transform.position = position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Obstacle"))
        {
            if (GameManager.instance != null && GameManager.instance.ConsumeShieldIfAvailable())
            {
                Destroy(other.gameObject);
                return;
            }

            Die();
        }
    }

    public void Die()
    {
        isDead = true;
    }
}