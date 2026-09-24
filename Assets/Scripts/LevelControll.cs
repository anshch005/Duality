using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelControll : MonoBehaviour
{
    [Header("Options Menu")]
    [SerializeField] private GameObject OptionsMenu;

    [Header("Objects Disabled While Options Is Open")]
    [SerializeField] private GameObject ObjectToDisable1;
    [SerializeField] private GameObject ObjectToDisable2;
    [SerializeField] private GameObject ObjectToDisable3;
    [SerializeField] private GameObject ObjectToDisable4;

    private static bool blueAtNext = false;
    private static bool pinkAtNext = false;

    private void Start()
    {
        if (OptionsMenu != null)
        {
            OptionsMenu.SetActive(false);
        }
        else
        {
            Debug.LogError(
                "OptionsMenu is NOT assigned on " + gameObject.name
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Next"))
        {
            if (CompareTag("PlayerBlue"))
            {
                blueAtNext = true;
                Debug.Log("Blue reached Start/Next");
            }

            if (CompareTag("PlayerPink"))
            {
                pinkAtNext = true;
                Debug.Log("Pink reached Start/Next");
            }

            if (blueAtNext && pinkAtNext)
            {
                StartGame();
            }

            return;
        }

        if (other.CompareTag("Options"))
        {
            Debug.Log(gameObject.name + " stepped on Options");

            if (OptionsMenu != null)
            {
                OptionsMenu.SetActive(true);
                Debug.Log("Options Menu OPENED");
            }
            else
            {
                Debug.LogError("OptionsMenu is NOT assigned!");
            }

            SetObjectsActive(false);

            return;
        }

        if (other.CompareTag("Back"))
        {
            Debug.Log(gameObject.name + " stepped on Back");

            if (OptionsMenu != null)
            {
                OptionsMenu.SetActive(false);
                Debug.Log("Options Menu CLOSED");
            }

            SetObjectsActive(true);

            return;
        }

        if (other.CompareTag("Quit"))
        {
            Debug.Log(gameObject.name + " stepped on Quit");

#if UNITY_EDITOR
            Debug.Log("Quit requested - stopping Unity Play Mode.");
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            return;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Next"))
            return;

        if (CompareTag("PlayerBlue"))
        {
            blueAtNext = false;
        }

        if (CompareTag("PlayerPink"))
        {
            pinkAtNext = false;
        }
    }

    private void SetObjectsActive(bool active)
    {
        if (ObjectToDisable1 != null)
        {
            ObjectToDisable1.SetActive(active);
        }

        if (ObjectToDisable2 != null)
        {
            ObjectToDisable2.SetActive(active);
        }

        if (ObjectToDisable3 != null)
        {
            ObjectToDisable3.SetActive(active);
        }

        if (ObjectToDisable4 != null)
        {
            ObjectToDisable4.SetActive(active);
        }

        Debug.Log("Options objects " + (active ? "ENABLED" : "DISABLED"));
    }

    public void StartGame()
    {
        Debug.Log("Starting game / loading next level...");

        blueAtNext = false;
        pinkAtNext = false;

        int currentScene = SceneManager.GetActiveScene().buildIndex;
        int nextScene = currentScene + 1;

        if (nextScene >= SceneManager.sceneCountInBuildSettings)
        {
            nextScene = 0;
        }

        SceneManager.LoadScene(nextScene);
    }

    /// <summary>
    /// Loads the first gameplay level (Level 1 / build index 1).
    /// </summary>
    public void PlayAgain()
    {
        Debug.Log("Play Again clicked");
        if (Application.CanStreamedLevelBeLoaded("Level 1"))
        {
            SceneManager.LoadScene("Level 1");
        }
        else
        {
            int targetIndex = SceneManager.sceneCountInBuildSettings > 1 ? 1 : 0;
            SceneManager.LoadScene(targetIndex);
        }
    }

    /// <summary>
    /// Loads the Main Menu scene (Level / build index 0).
    /// </summary>
    public void MainMenu()
    {
        Debug.Log("Main Menu clicked");
        if (Application.CanStreamedLevelBeLoaded("Level"))
        {
            SceneManager.LoadScene("Level");
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }

    /// <summary>
    /// Quits the game or stops play mode in editor.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quit Game clicked");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}