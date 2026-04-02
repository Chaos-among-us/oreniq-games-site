using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public string menuSceneName = "MainMenu";

    public void GoToMenu()
    {
        Time.timeScale = 1f; // ensure game isn't paused
        SceneManager.LoadScene(menuSceneName);
    }
}