using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private SpriteRenderer sprite;
    private Animator anim;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float minJumpForce = 5f;
    [SerializeField] private float apexBoost = 1.5f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float maxFallSpeed = -10f;
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float deceleration = 20f;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 1f;

    private float dirX = 0f;
    private float currentSpeed = 0f;
    private float graceTime = 0.15f;
    private float graceTimer;
    private float jumpBufferTime = 0.2f;
    private float jumpBufferTimer;
    private bool isDucking = false;

    private bool isDashing = false;
    private float dashTimer;
    private float dashCooldownTimer;

    private MovementState currentState;
    private enum MovementState { idle, running, jumping, falling, ducking }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>(); // For sprite flipping
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        HandleInput();
        UpdateAnimationState();
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        ApplyMovement();
        ApplyJump();
        ApplyFallModifiers();
        ApplyWallSlide();
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
            graceTimer = graceTime;
        }
        else
        {
            graceTimer -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0f && !isDashing && !isDucking)
        {
            StartDash();
        }

        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        Vector2 dashDirection = new Vector2(dirX, 0).normalized;
        if (dashDirection == Vector2.zero) dashDirection = Vector2.right * Mathf.Sign(transform.localScale.x);

        rb.velocity = dashDirection * dashSpeed;
        Invoke(nameof(ResetDash), dashDuration);
    }

    private void ResetDash()
    {
        isDashing = false;
    }

    private void ApplyMovement()
    {
        if (isDucking)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        float targetSpeed = dirX * moveSpeed;

        if (dirX != 0)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }

        rb.velocity = new Vector2(currentSpeed, rb.velocity.y);

        // Flip sprite based on movement direction
        if (dirX > 0)
            sprite.flipX = false;
        else if (dirX < 0)
            sprite.flipX = true;
    }

    private void ApplyJump()
    {
        if (jumpBufferTimer > 0 && graceTimer > 0 && !isDucking)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpBufferTimer = 0;
            graceTimer = 0;
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > minJumpForce)
        {
            rb.velocity = new Vector2(rb.velocity.x, minJumpForce);
        }
    }

    private void ApplyFallModifiers()
    {
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = fallMultiplier;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.gravityScale = lowJumpMultiplier;
        }
        else
        {
            rb.gravityScale = 1f;
        }

        if (IsInApex())
        {
            moveSpeed += apexBoost * Time.fixedDeltaTime;
        }
        else
        {
            moveSpeed = Mathf.Clamp(moveSpeed, 5f, 8f);
        }

        if (rb.velocity.y < maxFallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, maxFallSpeed);
        }
    }

    private bool IsInApex()
    {
        return Mathf.Abs(rb.velocity.y) < 0.1f && !IsGrounded();
    }

    private void ApplyWallSlide()
    {
        if (!IsGrounded() && rb.velocity.y <= 0)
        {
            rb.velocity = new Vector2(rb.velocity.x * 0.9f, rb.velocity.y);
        }
    }

    private void UpdateAnimationState()
    {
        MovementState state;

        if (isDucking)
        {
            state = MovementState.ducking;
        }
        else if (dirX > 0f || dirX < 0f)
        {
            state = MovementState.running;
        }
        else
        {
            state = MovementState.idle;
        }

        if (rb.velocity.y > 0.1f)
        {
            state = MovementState.jumping;
        }
        else if (rb.velocity.y < -0.1f)
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
