using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MenuButtons : MonoBehaviour
{
    [Header("Buttons to Navigate")]
    [Tooltip("List of buttons in navigation order. If empty, automatically gathers child Buttons.")]
    [SerializeField] private Button[] buttons;

    [Header("Selection Visuals")]
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float normalScale = 1.0f;
    [SerializeField] private float scaleSpeed = 12f;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip navigateSFX;
    [SerializeField] private AudioClip submitSFX;

    [Header("Scene Configuration")]
    [Tooltip("Name or build index 0 for the Main Menu scene")]
    [SerializeField] private string mainMenuSceneName = "Level";

    [Tooltip("Name or build index 1 for the First Level")]
    [SerializeField] private string firstLevelSceneName = "Level 1";

    private int selectedIndex = 0;
    private bool stickMoved = false;

    private void Start()
    {
        if (buttons == null || buttons.Length == 0)
        {
            buttons = GetComponentsInChildren<Button>();
        }

        UpdateSelection(0, playAudio: false);
    }

    private void Update()
    {
        if (buttons == null || buttons.Length == 0)
            return;

        HandleInput();
        AnimateButtonScales();
    }

    private void HandleInput()
    {
        bool navigateUp = false;
        bool navigateDown = false;
        bool submit = false;

        if (Gamepad.current != null)
        {
            var gp = Gamepad.current;

            if (gp.dpad.up.wasPressedThisFrame || gp.dpad.left.wasPressedThisFrame)
                navigateUp = true;
            if (gp.dpad.down.wasPressedThisFrame || gp.dpad.right.wasPressedThisFrame)
                navigateDown = true;

            if (gp.buttonSouth.wasPressedThisFrame)
                submit = true;
        }
        if (Keyboard.current != null)
        {
            var kb = Keyboard.current;

            if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame)
                navigateUp = true;
            if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame)
                navigateDown = true;

            if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
                submit = true;
        }

        if (navigateUp)
        {
            int newIndex = selectedIndex - 1;
            if (newIndex < 0) newIndex = buttons.Length - 1;
            UpdateSelection(newIndex, playAudio: true);
        }
        else if (navigateDown)
        {
            int newIndex = selectedIndex + 1;
            if (newIndex >= buttons.Length) newIndex = 0;
            UpdateSelection(newIndex, playAudio: true);
        }

        if (submit)
        {
            ExecuteCurrentButton();
        }
    }

    public void UpdateSelection(int newIndex, bool playAudio = true)
    {
        if (buttons == null || buttons.Length == 0)
            return;

        selectedIndex = Mathf.Clamp(newIndex, 0, buttons.Length - 1);

        // Highlight in Unity EventSystem
        if (EventSystem.current != null && buttons[selectedIndex] != null)
        {
            EventSystem.current.SetSelectedGameObject(buttons[selectedIndex].gameObject);
        }

        if (playAudio && navigateSFX != null && SFXManager.Instance != null)
        {
            SFXManager.Instance.PlaySFX(navigateSFX);
        }
    }

    private void AnimateButtonScales()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;

            float targetScale = (i == selectedIndex) ? selectedScale : normalScale;
            Vector3 targetVector = Vector3.one * targetScale;

            buttons[i].transform.localScale = Vector3.Lerp(
                buttons[i].transform.localScale,
                targetVector,
                Time.unscaledDeltaTime * scaleSpeed
            );
        }
    }

    public void ExecuteCurrentButton()
    {
        if (buttons == null || selectedIndex < 0 || selectedIndex >= buttons.Length)
            return;

        if (buttons[selectedIndex] != null && buttons[selectedIndex].interactable)
        {
            if (submitSFX != null && SFXManager.Instance != null)
            {
                SFXManager.Instance.PlaySFX(submitSFX);
            }

            buttons[selectedIndex].onClick.Invoke();
        }
    }

    public void SelectButtonByIndex(int index)
    {
        UpdateSelection(index, playAudio: true);
    }

    public void PlayAgain()
    {
        Debug.Log("Play Again clicked - Loading first level: " + firstLevelSceneName);

        if (!string.IsNullOrEmpty(firstLevelSceneName) && Application.CanStreamedLevelBeLoaded(firstLevelSceneName))
        {
            SceneManager.LoadScene(firstLevelSceneName);
        }
        else
        {
            int targetIndex = SceneManager.sceneCountInBuildSettings > 1 ? 1 : 0;
            SceneManager.LoadScene(targetIndex);
        }
    }

    public void MainMenu()
    {
        Debug.Log("Main Menu clicked - Loading main menu: " + mainMenuSceneName);

        if (!string.IsNullOrEmpty(mainMenuSceneName) && Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }

    public void RestartCurrentLevel()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentScene);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game clicked.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
