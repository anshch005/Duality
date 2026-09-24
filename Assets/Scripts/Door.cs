using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    private bool blueAtDoor = false;
    private bool pinkAtDoor = false;
    private bool levelTransitionStarted = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (levelTransitionStarted)
            return;

        if (other.CompareTag("PlayerBlue"))
        {
            blueAtDoor = true;
            Debug.Log("PlayerBlue reached the door.");
        }
        else if (other.CompareTag("PlayerPink"))
        {
            pinkAtDoor = true;
            Debug.Log("PlayerPink reached the door.");
        }

        if (blueAtDoor && pinkAtDoor)
        {
            StartCoroutine(LoadNextLevel());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (levelTransitionStarted)
            return;

        if (other.CompareTag("PlayerBlue"))
        {
            blueAtDoor = false;
            Debug.Log("PlayerBlue left the door.");
        }
        else if (other.CompareTag("PlayerPink"))
        {
            pinkAtDoor = false;
            Debug.Log("PlayerPink left the door.");
        }
    }

    private IEnumerator LoadNextLevel()
    {
        levelTransitionStarted = true;
        Debug.Log("Both players reached the door! Loading next level...");

        yield return new WaitForSeconds(0.1f);

        int currentScene = SceneManager.GetActiveScene().buildIndex;
        int nextScene = currentScene + 1;

        if (nextScene >= SceneManager.sceneCountInBuildSettings)
        {
            nextScene = 0;
        }

        SceneManager.LoadScene(nextScene);
    }
}
