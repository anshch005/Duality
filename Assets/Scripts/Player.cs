using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public enum PlayerType
    {
        AutoDetect,
        PlayerBlue,
        PlayerPink
    }

    [Header("Player Identity")]
    [SerializeField] private PlayerType playerType = PlayerType.AutoDetect;

    [Header("Movement")]
    [SerializeField] private InputAction moveInputAction;
    [SerializeField] private InputAction jumpInputAction;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float sensitivityMultiplier = 1f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Audio")]
    [SerializeField] private AudioClip jumpSFX;
    [SerializeField] private AudioClip walkSFX;
    [SerializeField] private AudioClip deathSFX;
    [SerializeField] private float walkStepInterval = 0.35f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private bool isGrounded;
    private bool isDead;

    private bool jumpInProgress;
    private float moveInput;
    private float walkStepTimer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        isGrounded = true;
        jumpInProgress = false;
        walkStepTimer = walkStepInterval;

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

        HandleWalkSFX();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (isDead)
            return;

        float effectiveSpeed = moveSpeed * sensitivityMultiplier * GetCurrentSensitivity();

        rb.linearVelocity = new Vector2(
            moveInput * effectiveSpeed,
            rb.linearVelocity.y
        );

        UpdateAirState();
    }

    private void HandleWalkSFX()
    {
        if (isGrounded && Mathf.Abs(moveInput) > 0.01f)
        {
            walkStepTimer += Time.deltaTime;
            if (walkStepTimer >= walkStepInterval)
            {
                PlaySFX(walkSFX);
                walkStepTimer = 0f;
            }
        }
        else
        {
            walkStepTimer = walkStepInterval;
        }
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && SFXManager.Instance != null)
        {
            SFXManager.Instance.PlaySFX(clip);
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        bool isWalking = Mathf.Abs(moveInput) > 0.01f;

        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isGrounded", isGrounded);

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

        isGrounded = false;
        jumpInProgress = true;

        animator.SetTrigger("Jump");

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );

        PlaySFX(jumpSFX);
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hostile"))
        {
            Die();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Hostile"))
        {
            Die();
            return;
        }

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

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                if (jumpInProgress && rb.linearVelocity.y > 0.05f)
                {
                    return;
                }

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

        if (!jumpInProgress)
        {
            isGrounded = false;
            UpdateAnimator();
        }
    }

    public void DisableInput()
    {
        moveInput = 0f;
        moveInputAction.Disable();
        jumpInputAction.Disable();

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        UpdateAnimator();
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;
        Debug.Log(gameObject.name + " died from Hostile!");

        PlaySFX(deathSFX);

        Player[] allPlayers = FindObjectsByType<Player>(FindObjectsSortMode.None);
        foreach (Player player in allPlayers)
        {
            player.DisableInput();
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        StartCoroutine(FadeOutAndReload());
    }

    private IEnumerator FadeOutAndReload()
    {
        float fadeDuration = 1.0f;
        float elapsedTime = 0f;

        Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float alpha = Mathf.Lerp(
                originalColor.a,
                0f,
                elapsedTime / fadeDuration
            );

            if (spriteRenderer != null)
            {
                Color newColor = originalColor;
                newColor.a = alpha;
                spriteRenderer.color = newColor;
            }

            yield return null;
        }

        if (spriteRenderer != null)
        {
            Color finalColor = originalColor;
            finalColor.a = 0f;
            spriteRenderer.color = finalColor;
        }

        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentScene);
    }

    public float GetCurrentSensitivity()
    {
        if (SensitivityManager.Instance != null)
        {
            return SensitivityManager.Instance.GetSensitivityForPlayer(this);
        }

        if (IsPlayerBlue())
        {
            return PlayerPrefs.GetFloat("BlueSensitivity", 1f);
        }
        else if (IsPlayerPink())
        {
            return PlayerPrefs.GetFloat("PinkSensitivity", 1f);
        }

        return PlayerPrefs.GetFloat("GlobalSensitivity", 1f);
    }

    public bool IsPlayerBlue()
    {
        return playerType == PlayerType.PlayerBlue ||
               (playerType == PlayerType.AutoDetect && (CompareTag("PlayerBlue") || gameObject.name.Contains("Blue")));
    }

    public bool IsPlayerPink()
    {
        return playerType == PlayerType.PlayerPink ||
               (playerType == PlayerType.AutoDetect && (CompareTag("PlayerPink") || gameObject.name.Contains("Pink")));
    }

    public void SetPlayerType(PlayerType type)
    {
        playerType = type;
    }

    public void SetSensitivityMultiplier(float multiplier)
    {
        sensitivityMultiplier = multiplier;
    }
}