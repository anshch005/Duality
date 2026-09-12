using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerBlue : MonoBehaviour
{
    [SerializeField] InputAction moveInputAction;
    [SerializeField] InputAction jumpInputAction;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 7f;
    [SerializeField] Animator animator;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private bool isGrounded;
    private bool wasGrounded;
    private bool isDead;

    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        isGrounded = true;
        wasGrounded = true;

        UpdateAnimator();
    }

    void OnEnable()
    {
        moveInputAction.Enable();
        jumpInputAction.Enable();
    }

    void OnDisable()
    {
        moveInputAction.Disable();
        jumpInputAction.Disable();
    }

    void Update()
    {
        if (isDead)
            return;

        // Only read input here
        moveInput = moveInputAction.ReadValue<float>();

        // Flip sprite (visual only, safe in Update)
        FlipSprite();

        // Jump input
        if (jumpInputAction.WasPressedThisFrame() && isGrounded)
        {
            Jump();
        }

        UpdateAnimator();

        wasGrounded = isGrounded;
    }

    private void UpdateAnimator()
    {
        bool isWalking = Mathf.Abs(moveInput) > 0.01f;

        animator.SetBool("IsWalking", isWalking);
        animator.SetBool("IsGrounded", isGrounded);
    }

    private void FlipSprite()
    {
        if (moveInput > 0.01f)
            spriteRenderer.flipX = false;
        else if (moveInput < -0.01f)
            spriteRenderer.flipX = true;
    }

    private void Jump()
    {
        isGrounded = false;

        animator.SetTrigger("Jump");
        
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    void FixedUpdate()
    {
        if (isDead)
            return;

        // Move using physics here
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
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

            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}
