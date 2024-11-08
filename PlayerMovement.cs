using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private SpriteRenderer sprite;
    private Animator anim;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f; // Base jump force
    [SerializeField] private float minJumpForce = 5f; // Variable jump height (hold for max, release for min)
    [SerializeField] private float apexBoost = 1.5f; // Apex modifier for smoother jump
    [SerializeField] private float fallMultiplier = 2.5f; // Increase gravity when falling
    [SerializeField] private float lowJumpMultiplier = 2f; // Increase gravity for short jumps
    [SerializeField] private float maxFallSpeed = -10f; // Clamped fall speed
    [SerializeField] private float acceleration = 15f; 
    [SerializeField] private float deceleration = 20f; 
    [SerializeField] private float duckSpeed = 2f; 
    [SerializeField] private LayerMask groundLayer;

    private float dirX = 0f;
    private float currentSpeed = 0f;
    private float graceTime = 0.15f; // Coyote time duration
    private float graceTimer;
    private float jumpBufferTime = 0.2f; // Time allowed for jump buffering
    private float jumpBufferTimer;
    private bool isDucking = false;

    private MovementState currentState;
    private enum MovementState { idle, running, jumping, falling, ducking }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        HandleInput();
        UpdateAnimationState();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        ApplyJump();
        ApplyFallModifiers();
    }

    private void HandleInput()
    {
        dirX = Input.GetAxisRaw("Horizontal");

        if (Input.GetKey(KeyCode.S) && IsGrounded())
        {
            isDucking = true;
        }
        else
        {
            isDucking = false;
        }

        if (IsGrounded())
        {
            graceTimer = graceTime; // Reset coyote time
        }
        else
        {
            graceTimer -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferTimer = jumpBufferTime; // Start jump buffering
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }
    }

    private void ApplyMovement()
    {
        float targetSpeed = isDucking ? dirX * duckSpeed : dirX * moveSpeed;

        if (dirX != 0)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }

        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
    }

    private void ApplyJump()
    {
        if (jumpBufferTimer > 0 && graceTimer > 0 && !isDucking)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferTimer = 0; // Consume jump buffer
            graceTimer = 0; // Consume coyote time
        }

        // Apply variable jump height using minJumpForce
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > minJumpForce)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, minJumpForce);
        }
    }


    private void ApplyFallModifiers()
    {
        if (rb.linearVelocity.y < 0) // Falling
        {
            rb.gravityScale = fallMultiplier;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump")) // Short jump
        {
            rb.gravityScale = lowJumpMultiplier;
        }
        else
        {
            rb.gravityScale = 1f;
        }

        // Apply Apex Modifier (for smoother height transition)
        if (IsInApex())
        {
            moveSpeed += apexBoost * Time.fixedDeltaTime;
        }
        else
        {
            moveSpeed = Mathf.Clamp(moveSpeed, 5f, 8f); // Reset base after apex boosts
        }

        // Clamp fall speed
        if (rb.linearVelocity.y < maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxFallSpeed);
        }
    }

    private bool IsInApex()
    {
        return Mathf.Abs(rb.linearVelocity.y) < 0.1f && !IsGrounded();
    }

    private void UpdateAnimationState()
    {
        MovementState state;

        if (isDucking)
        {
            state = MovementState.ducking;
        }
        else if (dirX > 0f)
        {
            state = MovementState.running;
            sprite.flipX = false;
        }
        else if (dirX < 0f)
        {
            state = MovementState.running;
            sprite.flipX = true;
        }
        else
        {
            state = MovementState.idle;
        }

        if (rb.linearVelocity.y > 0.1f)
        {
            state = MovementState.jumping;
        }
        else if (rb.linearVelocity.y < -0.1f)
        {
            state = MovementState.falling;
        }

        if (state != currentState)
        {
            anim.SetInteger("state", (int)state);
            currentState = state;
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, 0.2f, groundLayer);
    }
}
