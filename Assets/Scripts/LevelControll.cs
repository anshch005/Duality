using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelControll : MonoBehaviour
{
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

            case "Hostile":
                Debug.Log("Player hit Hostile");
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