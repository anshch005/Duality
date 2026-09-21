using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBlue : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private InputAction moveInputAction;
    [SerializeField] private InputAction jumpInputAction;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private bool isGrounded;
    private bool isDead;

    // Prevents the ground collision from immediately
    // making the player grounded again after jumping.
    private bool jumpInProgress;

    private float moveInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        isGrounded = true;
        jumpInProgress = false;

        UpdateAnimator();
    }

    private void OnEnable()
    {
        moveInputAction.Enable();
        jumpInputAction.Enable();
    }

    private void OnDisable()
    {
        moveInputAction.Disable();
        jumpInputAction.Disable();
    }

    private void Update()
    {
        if (isDead)
            return;

        moveInput = moveInputAction.ReadValue<float>();

        FlipSprite();

        if (jumpInputAction.WasPressedThisFrame() && isGrounded)
        {
            Jump();
        }

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (isDead)
            return;

        // Movement
        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );

        UpdateAirState();
    }

    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        bool isWalking = Mathf.Abs(moveInput) > 0.01f;

        // EXACT Animator parameter names
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isGrounded", isGrounded);

        // YValue:
        //  1 = going up
        //  0 = grounded
        // -1 = falling
        if (isGrounded)
        {
            animator.SetInteger("YValue", 0);
        }
        else if (rb.linearVelocity.y > 0.05f)
        {
            animator.SetInteger("YValue", 1);
        }
        else if (rb.linearVelocity.y < -0.05f)
        {
            animator.SetInteger("YValue", -1);
        }
        else
        {
            animator.SetInteger("YValue", 0);
        }
    }

    private void UpdateAirState()
    {
        if (isGrounded)
        {
            animator.SetInteger("YValue", 0);
            return;
        }

        if (rb.linearVelocity.y > 0.05f)
        {
            animator.SetInteger("YValue", 1);
        }
        else if (rb.linearVelocity.y < -0.05f)
        {
            animator.SetInteger("YValue", -1);
        }
    }

    private void Jump()
    {
        if (!isGrounded)
            return;

        // Immediately become airborne
        isGrounded = false;

        // Tell collision code that a jump is happening
        jumpInProgress = true;

        // Trigger ONLY the jump animation
        animator.SetTrigger("Jump");

        // Apply jump velocity
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );

        UpdateAnimator();
    }

    private void FlipSprite()
    {
        if (moveInput > 0.01f)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveInput < -0.01f)
        {
            spriteRenderer.flipX = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckGroundCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckGroundCollision(collision);
    }

    private void CheckGroundCollision(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground"))
            return;

        // Check the actual collision direction.
        // A collision from the side should NOT count as landing.
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                if (jumpInProgress && rb.linearVelocity.y > 0.05f)
                {
                    return;
                }

                // Actual landing
                bool wasAirborne = !isGrounded;

                isGrounded = true;
                jumpInProgress = false;

                UpdateAnimator();

                if (wasAirborne)
                {
                    Debug.Log("PLAYER LANDED");
                }

                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground"))
            return;

        // Don't immediately force grounded false from
        // unrelated collision contacts.
        if (!jumpInProgress)
        {
            isGrounded = false;
            UpdateAnimator();
        }
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;

        rb.linearVelocity = Vector2.zero;

        moveInputAction.Disable();
        jumpInputAction.Disable();

        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float fadeDuration = 1.5f;
        float elapsedTime = 0f;

        Color originalColor = spriteRenderer.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float alpha = Mathf.Lerp(
                1f,
                0f,
                elapsedTime / fadeDuration
            );

            Color newColor = originalColor;
            newColor.a = alpha;

            spriteRenderer.color = newColor;

            yield return null;
        }

        Color finalColor = spriteRenderer.color;
        finalColor.a = 0f;

        spriteRenderer.color = finalColor;

        gameObject.SetActive(false);
    }
}