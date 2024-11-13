using UnityEngine;

public class FireballBehavior : MonoBehaviour
{
    public float speed = 0.01f; // Speed of the fireball (adjust for desired speed)
    private Vector2 moveDirection;

    public void Initialize(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }

    void FixedUpdate() // Use FixedUpdate for physics-based movement
    {
        // Move the fireball at a consistent speed
        GetComponent<Rigidbody2D>().MovePosition(GetComponent<Rigidbody2D>().position + moveDirection * speed * Time.fixedDeltaTime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
