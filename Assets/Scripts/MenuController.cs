using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    
    public void PlayGame()
    {
        SceneManager.LoadScene("Arno_Hospital");
    }

    public void OpenInfo()
    {
        SceneManager.LoadScene("GameInfoMenu");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        // Stop playing the scene in the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quit the built application
        Application.Quit();
#endif
    }
}
