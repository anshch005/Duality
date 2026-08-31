using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelControl : MonoBehaviour
{   
     private void OnCollisionEnter(Collision other) 
    {
        switch (other.gameObject.tag)
        {
            case "Hostile":
                ReloadLevel();
                break;
            case "End":
                Debug.Log("Why did you pick me up, I'm not in this game");
                LoadNextLevel();
                break;
        }

        void LoadNextLevel()
            {
            int currentScene = SceneManager.GetActiveScene().buildIndex;
            int nextScene = currentScene + 1;
            
            if (nextScene == SceneManager.sceneCountInBuildSettings)
                {
                    nextScene = 0;
                }
            
                SceneManager.LoadScene(nextScene);
            }

        void ReloadLevel()
        {
            int currentScene = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentScene);
        }
    }
}
