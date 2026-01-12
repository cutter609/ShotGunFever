using UnityEngine;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
    public GameObject StartScreen;
    public GameObject WinScreen;
    public GameObject DeathScreen;
    public GameObject PauseScreen;

    private LevelController levelController;
    private Player player;

    private void Awake()
    {
        levelController = GameObject.FindGameObjectWithTag("World").GetComponent<LevelController>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
        ResetTime();
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        ResetTime();
    }
    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        ResetTime();
    }
    public void Resume()
    {
        PauseScreen.SetActive(false);
        player.Paused = false;
        ResetTime();
    }
    private void ResetTime()
    {
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = levelController.DefaultDeltatime;
    }
}
