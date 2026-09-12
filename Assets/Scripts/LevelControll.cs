using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelControll : MonoBehaviour
{
    [Header("Options Menu")]
    [SerializeField] private GameObject OptionsMenu;
    private static bool blueAtNext = false;
    private static bool pinkAtNext = false;
    private void OnTriggerEnter2D(Collider2D other)
    {
        switch (other.gameObject.tag)
        {
            case "Next":

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

                break;

            case "Options":
                if (other.gameObject.CompareTag("PlayerBlue"))
                {
                    Debug.Log("Options Menu Opened");
                    OptionsMenu.SetActive(true);

                    foreach (var text in FindObjectsByType<Text>(FindObjectsSortMode.None))
                    {
                        text.enabled = false;
                    }
                }
                break;

            case "Back":
                if (other.gameObject.CompareTag("PlayerBlue"))
                {
                    Debug.Log("Options Menu Closed");
                    OptionsMenu.SetActive(false);

                    foreach (var text in FindObjectsByType<Text>(FindObjectsSortMode.None))
                    {
                        text.enabled = true;
                    }
                }
                break;

            case "Quit":
                if (other.gameObject.CompareTag("PlayerBlue"))
                {
                    Debug.Log("Quit Game");
                    Application.Quit();
                }
                break;

            case "Hostile":
                Debug.Log("Player hit Hostile - Respawning in 3 seconds...");
                StartCoroutine(ReloadLevel());
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Next"))
        {
            if (CompareTag("PlayerBlue"))
            {
                blueAtNext = false;
            }

            if (CompareTag("PlayerPink"))
            {
                pinkAtNext = false;
            }
        }
    }

    private IEnumerator ReloadLevel()
    {
        // Disable movement of the player that hit Hostile
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
        blueAtNext = false;
        pinkAtNext = false;

        int currentScene = SceneManager.GetActiveScene().buildIndex;
        int nextScene = currentScene + 1;

        if (nextScene == SceneManager.sceneCountInBuildSettings)
        {
            nextScene = 0;
        }

        SceneManager.LoadScene(nextScene);
    }
}