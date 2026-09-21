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
        // Make sure Options Menu starts disabled
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
                Debug.Log("Blue reached Next");
            }

            if (CompareTag("PlayerPink"))
            {
                pinkAtNext = true;
                Debug.Log("Pink reached Next");
            }

            if (blueAtNext && pinkAtNext)
            {
                LoadNextLevel();
            }

            return;
        }

        if (other.CompareTag("Options"))
        {
            Debug.Log(
                gameObject.name + " stepped on Options"
            );

            // Open Options Menu
            if (OptionsMenu != null)
            {
                OptionsMenu.SetActive(true);
                Debug.Log("Options Menu OPENED");
            }
            else
            {
                Debug.LogError(
                    "OptionsMenu is NOT assigned!"
                );
            }

            // Disable the 4 selected GameObjects
            SetObjectsActive(false);

            return;
        }

        if (other.CompareTag("Back"))
        {
            Debug.Log(
                gameObject.name + " stepped on Back"
            );

            // Close Options Menu
            if (OptionsMenu != null)
            {
                OptionsMenu.SetActive(false);
                Debug.Log("Options Menu CLOSED");
            }

            // Re-enable the 4 selected GameObjects
            SetObjectsActive(true);

            return;
        }

        if (other.CompareTag("Quit"))
        {
            Debug.Log(
                gameObject.name + " stepped on Quit"
            );

#if UNITY_EDITOR
            Debug.Log(
                "Quit requested - stopping Unity Play Mode."
            );

            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif

            return;
        }

        if (other.CompareTag("Hostile"))
        {
            Debug.Log(gameObject.name + " hit Hostile - Respawning...");

            StartCoroutine(ReloadLevel());

            return;
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

        Debug.Log(
            "Options objects " +
            (active ? "ENABLED" : "DISABLED")
        );
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

    private IEnumerator ReloadLevel()
    {
        PlayerBlue movement = GetComponent<PlayerBlue>();

        if (movement != null)
        {
            movement.enabled = false;
            movement.Die();
        }

        yield return new WaitForSeconds(0.5f);

        Debug.Log("Respawning level...");

        blueAtNext = false;
        pinkAtNext = false;

        int currentScene = SceneManager.GetActiveScene().buildIndex;

        SceneManager.LoadScene(currentScene);
    }

    private void LoadNextLevel()
    {
        Debug.Log("Both players reached Next!");

        blueAtNext = false;
        pinkAtNext = false;

        int currentScene =
            SceneManager.GetActiveScene().buildIndex;

        int nextScene = currentScene + 1;

        if (nextScene >= SceneManager.sceneCountInBuildSettings)
        {
            nextScene = 0;
        }

        SceneManager.LoadScene(nextScene);
    }
}