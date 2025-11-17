using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    
    public void PlayGame()
    {
        SceneManager.LoadScene("FullNewHospital");
    }

    public void PlaySecondLevel()
    {
        SceneManager.LoadScene("FullOldHospital");
    }

    public void OpenInfo()
    {
        SceneManager.LoadScene("GameInfoScreen");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quit the built application
        Application.Quit();
#endif
    }
}
