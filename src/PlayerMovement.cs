using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator anim;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float wallSlidingSpeed = 2f;
    [SerializeField] private Vector2 wallJumpingPower = new Vector2(7f, 9f);
    [SerializeField] private float wallJumpingDuration = 0.4f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;

    private float horizontal;
    private bool isFacingRight = true;
    private bool isWallSliding;
    private bool isWallJumping;
    private bool isDashing;
    private float dashCooldownTimer;
    private bool isDucking = false;

    private MovementState currentState;
    private enum MovementState { idle, running, jumping, falling, ducking, wallSliding}

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && IsGrounded() && !isWallJumping && !isDashing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0f && !isDashing)
        {
            StartDash();
        }

        WallSlide();
        if (isWallSliding)
        {
            WallJump();
        }

        HandleDucking();

        if (!isWallJumping && !isDashing)
        {
            Flip();
        }

        UpdateAnimationState();
    }

    private void FixedUpdate()
    {
        if (!isWallJumping && !isDashing && !isDucking)
        {
            rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && horizontal != 0f)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void WallJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            isWallJumping = true;
            float wallJumpDirection = -Mathf.Sign(transform.localScale.x);
            rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpingPower.x, wallJumpingPower.y);

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    private void StartDash()
    {
        isDashing = true;
        rb.linearVelocity = new Vector2(horizontal * dashSpeed, 0f);
        Invoke(nameof(StopDash), dashDuration);
        dashCooldownTimer = dashCooldown;
    }

    private void StopDash()
    {
        isDashing = false;
    }

    private void HandleDucking()
    {
        if (Input.GetKey(KeyCode.S) && IsGrounded())
        {
            isDucking = true;
        }
        else
        {
            isDucking = false;
        }
    }

    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private void UpdateAnimationState()
    {
        MovementState state;

        if (isWallSliding)
        {
            state = MovementState.wallSliding;
        }
        else if (isDucking)
        {
            state = MovementState.ducking;
        }
        else if (rb.linearVelocity.y > 0.1f)
        {
            state = MovementState.jumping;
        }
        else if (rb.linearVelocity.y < -0.1f)
        {
            state = MovementState.falling;
        }
        else if (horizontal != 0f)
        {
            state = MovementState.running;
        }
        else
        {
            state = MovementState.idle;
        }

        if (state != currentState)
        {
            anim.SetInteger("state", (int)state);
            currentState = state;
        }
    }

    private void LateUpdate()
    {
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }
}
