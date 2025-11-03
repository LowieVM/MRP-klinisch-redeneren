using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [Tooltip("Name of the scene to load when game over occurs")]
    public string gameOverSceneName = "GameOverScreen";

    public bool isGameOver = false;

    // internal guard so scene is loaded only once
    private bool _sceneLoading = false;

    //TODO: Real game over logic to be implemented
    private void Update()
    {
        if (isGameOver && !_sceneLoading)
        {
            _sceneLoading = true;
            SceneManager.LoadScene(gameOverSceneName);
        }
    }
}
