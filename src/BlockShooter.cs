using UnityEngine;

public class BlockShooter : MonoBehaviour
{
    public GameObject fireballPrefab; // Fireball prefab reference
    public Transform fireballSpawnPoint; // Spawn point of fireball
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Shoot fireballs every 5 seconds
        InvokeRepeating("ShootFireball", 0f, 2f);
    }

    void ShootFireball()
    {
        animator.SetTrigger("OpenMouth");

        // Instantiate the fireball
        GameObject fireball = Instantiate(fireballPrefab, fireballSpawnPoint.position, fireballSpawnPoint.rotation);

        // Determine the direction the block is facing
        Vector2 direction = transform.right * (transform.localScale.x > 0 ? 1 : -1);

        // Initialize the fireball's direction
        fireball.GetComponent<FireballBehavior>().Initialize(direction);

        // Close mouth animation after delay
        Invoke("CloseMouth", 0.5f);
    }

    void CloseMouth()
    {
        animator.SetTrigger("CloseMouth");
    }
}
