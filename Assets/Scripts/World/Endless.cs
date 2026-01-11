using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Endless : MonoBehaviour
{
    public GameObject Player;
    private Player PlayerScript;


    public GameObject DeathScreen;
    public GameObject PauseScreen;

    public float SpawnDelay;
    private float LastSpawnTime = -100;
    public GameObject Enemy;

    public bool SpawnDisabled;

    public Transform Spawn1;
    public Transform Spawn2;
    public Transform Spawn3;
    public Transform Spawn4;
    public Transform Spawn5;

    private bool StartClock = false;
    private float ClockStartTime;
    private float ClockTimeRaw;

    public GameObject Timer;

    public TextMeshProUGUI TimerDigitMin1;
    public TextMeshProUGUI TimerDigitMin2;
    public TextMeshProUGUI TimerDigitSec1;
    public TextMeshProUGUI TimerDigitSec2;
    public TextMeshProUGUI TimerDigitMilSec1;
    public TextMeshProUGUI TimerDigitMilSec2;

    private void Awake()
    {
        PlayerScript = Player.GetComponent<Player>();
    }
    private void Update()
    {
        if (LastSpawnTime + SpawnDelay < Time.time && !SpawnDisabled)
        {
            Instantiate(Enemy, Spawn1.position, Spawn1.rotation);
            Instantiate(Enemy, Spawn2.position, Spawn2.rotation);
            Instantiate(Enemy, Spawn3.position, Spawn3.rotation);
            Instantiate(Enemy, Spawn4.position, Spawn4.rotation);
            Instantiate(Enemy, Spawn5.position, Spawn5.rotation);
            LastSpawnTime = Time.time;
        }

        if (Player.transform.position.y < -5)
        {
            DeathScreen.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Escape) && !DeathScreen.activeSelf)
        {
            PauseScreen.SetActive(true);
            Time.timeScale = 0.0f;
            PlayerScript.Paused = true;
        }

        if (!StartClock)
        {
            StartClock = true;
            ClockStartTime = Time.time;
        }
        if (StartClock && !Player.GetComponent<Player>().Dead)
        {
            ClockTimeRaw = Time.time - ClockStartTime;

            float minutes = (int)(ClockTimeRaw / 60);
            TimerDigitMin1.text = ((int)(minutes / 10)).ToString();
            TimerDigitMin2.text = ((int)(minutes % 10)).ToString();
            float seconds = (int)(ClockTimeRaw % 60);
            TimerDigitSec1.text = ((int)(seconds / 10)).ToString();
            TimerDigitSec2.text = ((int)(seconds % 10)).ToString();

            TimerDigitMilSec1.text = ((int)((ClockTimeRaw - (int)ClockTimeRaw) * 10)).ToString();
            TimerDigitMilSec2.text = (((int)((ClockTimeRaw - (int)ClockTimeRaw) * 100)) % 10).ToString();
        }
    }
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1.0f;
        PlayerScript.Paused = false;
    }
    public void Restart()
    {
        SceneManager.LoadScene("Endless");
        Time.timeScale = 1.0f;
        PlayerScript.Paused = false;
    }
    public void Resume()
    {
        PauseScreen.SetActive(false);
        Time.timeScale = 1.0f;
        PlayerScript.Paused = false;
    }
}
