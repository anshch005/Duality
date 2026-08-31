using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBlue : MonoBehaviour
{
    [SerializeField] InputAction moveInputAction;
    [SerializeField] InputAction jumpInputAction;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 7f;

    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;

    private bool isGrounded;
    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        // Only read input here
        moveInput = moveInputAction.ReadValue<float>();

        // Flip sprite (visual only, safe in Update)
        FlipSprite();

        // Jump input
        if (jumpInputAction.WasPressedThisFrame() && isGrounded)
        {
            Jump();
        }
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
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    void FixedUpdate()
    {
        // Move using physics here
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
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