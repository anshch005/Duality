using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    private enum DoorState
    {
        Default,
        BlueOnly,
        PinkOnly,
        Both
    }

    [Header("Door Sprites")]
    [Tooltip("Default sprite when neither player is at the door (auto-assigned from SpriteRenderer if empty).")]
    [SerializeField] private Sprite defaultSprite;

    [Tooltip("Sprite when only PlayerBlue reaches the door.")]
    [SerializeField] private Sprite blueReachedSprite;

    [Tooltip("Sprite when only PlayerPink reaches the door.")]
    [SerializeField] private Sprite pinkReachedSprite;

    [Tooltip("Sprite when both PlayerBlue and PlayerPink reach the door.")]
    [SerializeField] private Sprite bothReachedSprite;

    [Header("Transition Settings")]
    [Tooltip("Duration in seconds for the smooth sprite fade transition.")]
    [SerializeField] private float fadeDuration = 0.25f;

    [Tooltip("Delay in seconds after both players reach before loading the next level.")]
    [SerializeField] private float levelTransitionDelay = 0.6f;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip playerReachedSFX;
    [SerializeField] private AudioClip bothReachedSFX;

    private SpriteRenderer spriteRenderer;
    private bool blueAtDoor = false;
    private bool pinkAtDoor = false;
    private bool levelTransitionStarted = false;
    private DoorState currentState = DoorState.Default;
    private Coroutine transitionCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (defaultSprite == null && spriteRenderer != null)
        {
            defaultSprite = spriteRenderer.sprite;
        }
    }

    private void Start()
    {
        if (spriteRenderer != null && defaultSprite != null && spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = defaultSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (levelTransitionStarted)
            return;

        bool stateChanged = false;

        if (other.CompareTag("PlayerBlue"))
        {
            blueAtDoor = true;
            stateChanged = true;
            Debug.Log("PlayerBlue reached the door.");
        }
        else if (other.CompareTag("PlayerPink"))
        {
            pinkAtDoor = true;
            stateChanged = true;
            Debug.Log("PlayerPink reached the door.");
        }

        if (stateChanged)
        {
            UpdateDoorState();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (levelTransitionStarted)
            return;

        bool stateChanged = false;

        if (other.CompareTag("PlayerBlue"))
        {
            blueAtDoor = false;
            stateChanged = true;
            Debug.Log("PlayerBlue left the door.");
        }
        else if (other.CompareTag("PlayerPink"))
        {
            pinkAtDoor = false;
            stateChanged = true;
            Debug.Log("PlayerPink left the door.");
        }

        if (stateChanged)
        {
            UpdateDoorState();
        }
    }

    private void UpdateDoorState()
    {
        DoorState newState;

        if (blueAtDoor && pinkAtDoor)
        {
            newState = DoorState.Both;
        }
        else if (blueAtDoor)
        {
            newState = DoorState.BlueOnly;
        }
        else if (pinkAtDoor)
        {
            newState = DoorState.PinkOnly;
        }
        else
        {
            newState = DoorState.Default;
        }

        if (newState == currentState)
            return;

        currentState = newState;
        Sprite targetSprite = GetSpriteForState(newState);

        if (targetSprite != null)
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }

            transitionCoroutine = StartCoroutine(SmoothChangeSprite(targetSprite));
        }

        if (newState == DoorState.Both)
        {
            if (bothReachedSFX != null && SFXManager.Instance != null)
            {
                SFXManager.Instance.PlaySFX(bothReachedSFX);
            }

            if (!levelTransitionStarted)
            {
                StartCoroutine(LoadNextLevel());
            }
        }
        else if (newState == DoorState.BlueOnly || newState == DoorState.PinkOnly)
        {
            if (playerReachedSFX != null && SFXManager.Instance != null)
            {
                SFXManager.Instance.PlaySFX(playerReachedSFX);
            }
        }
    }

    private Sprite GetSpriteForState(DoorState state)
    {
        switch (state)
        {
            case DoorState.BlueOnly:
                return blueReachedSprite != null ? blueReachedSprite : defaultSprite;

            case DoorState.PinkOnly:
                return pinkReachedSprite != null ? pinkReachedSprite : defaultSprite;

            case DoorState.Both:
                return bothReachedSprite != null ? bothReachedSprite : (blueReachedSprite != null ? blueReachedSprite : defaultSprite);

            case DoorState.Default:
            default:
                return defaultSprite;
        }
    }

    private IEnumerator SmoothChangeSprite(Sprite newSprite)
    {
        if (spriteRenderer == null || newSprite == null || spriteRenderer.sprite == newSprite)
            yield break;

        if (fadeDuration <= 0.01f)
        {
            spriteRenderer.sprite = newSprite;
            yield break;
        }

        float halfDuration = fadeDuration * 0.5f;
        Color baseColor = spriteRenderer.color;
        float startAlpha = baseColor.a;

        // Fade out slightly to create a smooth dissolve
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            float a = Mathf.Lerp(startAlpha, 0f, t);
            spriteRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            yield return null;
        }

        // Swap to the new sprite
        spriteRenderer.sprite = newSprite;

        // Fade back in
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            float a = Mathf.Lerp(0f, 1f, t);
            spriteRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            yield return null;
        }

        spriteRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
    }

    private IEnumerator LoadNextLevel()
    {
        levelTransitionStarted = true;
        Debug.Log("Both players reached the door! Loading next level...");

        yield return new WaitForSeconds(levelTransitionDelay);

        int currentScene = SceneManager.GetActiveScene().buildIndex;
        int nextScene = currentScene + 1;

        if (nextScene >= SceneManager.sceneCountInBuildSettings)
        {
            nextScene = 0;
        }

        SceneManager.LoadScene(nextScene);
    }
}
