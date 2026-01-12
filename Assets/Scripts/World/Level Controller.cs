using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class LevelController : MonoBehaviour
{
    [Header("Enemies")]
    public float AliveEnemyTimePenalty = 3;

    [HideInInspector] public int EnemyCount;
    private GameObject EnemyFolder;


    [Space(15)]
    [Header("Map")]
    public float StartScreenFlashSpeed = 2;

    public bool FrozenStart = false;
    public float AngleToStart = 5;
    public Transform FrozenStartTarget;

    public bool RainbowEnabled = false;
    public Tilemap tileMap;
    public float TilemapColorCycleSpeed = 10;

    private float TilemapHue;


    [Space(15)]
    [Header("Endless")]
    public bool Endless = false;
    public GameObject Agent;
    public float SpawnIntervalTime;
    public int SpawnAmount;
    public float NoSpawnDistance;

    public List<Transform> SpawnLocations = new();

    
    private Player player;
    private Timer timer;
    private UI UIObject;
    [HideInInspector] public readonly float DefaultDeltatime = 0.02f;//50 per second




    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        timer = GameObject.FindGameObjectWithTag("Timer").GetComponent<Timer>();
        UIObject = GameObject.FindGameObjectWithTag("UI").GetComponent<UI>();
        EnemyFolder = GameObject.FindGameObjectWithTag("Enemy Folder");

        //Resumes game when level is loaded
        player.Paused = false;

        //Gets amount of enemies in level from enemy folder
        if (EnemyFolder != null)
        {
            EnemyCount = EnemyFolder.transform.childCount;
        }

        if (FrozenStart)
        {
            UIObject.StartScreen.SetActive(false);
            player.IsGrounded = false;
            player.InAir = true;
            player.gameObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }

        if (Endless)
        {
            StartCoroutine(EndlessSpawn());
        }
    }
    private void Update()
    {
        //Pause
        if (Input.GetKeyDown(KeyCode.Escape) && !UIObject.DeathScreen.activeSelf && !UIObject.WinScreen.activeSelf && !UIObject.PauseScreen.activeSelf)
        {
            UIObject.PauseScreen.SetActive(true);
            Time.timeScale = 0.0f;
            player.Paused = true;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && UIObject.PauseScreen.activeSelf)
        {
            Resume();
        }

        if (!timer.TimerActive && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0)))
        {
            timer.StartTimer();
        }

        if (!timer.TimerActive)
        {
            UIObject.StartScreen.GetComponent<CanvasGroup>().alpha = Mathf.Pow(Mathf.Sin(Time.time * StartScreenFlashSpeed + 3.1415f / 2.0f), 2);
        }
        else
        {
            UIObject.StartScreen.SetActive(false);
        }

        //Control level where player start frozen
        if (FrozenStart)
        {
            player.gameObject.transform.position = new Vector3(0, 0.27f, 0);
            player.gameObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

            Vector2 direction = player.transform.position - FrozenStartTarget.gameObject.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float deltaAngle = Mathf.Abs(Mathf.DeltaAngle(player.transform.rotation.eulerAngles.z + 180, angle));
            if (Input.GetKeyDown(KeyCode.Mouse0) || deltaAngle < AngleToStart)
            {
                FrozenStart = false;
            }
        }

        //Endless control
        if (Endless && player.Dead)
        {
            Time.timeScale = 0.0f;
            player.Paused = true;
            timer.EndTimer(0);

            if (GameObject.FindGameObjectWithTag("Music Controller") != null)
            {
                //sets best time
                int SceneIndex = SceneManager.GetActiveScene().buildIndex;
                MusicController MusicControllerScript = GameObject.FindGameObjectWithTag("Music Controller").GetComponent<MusicController>();
                float PreviousBestTime = MusicControllerScript.BestTimes[SceneIndex];
                if (timer.ClockTime > PreviousBestTime)
                {
                    MusicControllerScript.BestTimes[SceneIndex] = timer.ClockTime;
                }
            }
            Endless = false;
        }


        //Makes map cycle through the rainbow
        if (RainbowEnabled)
        {
            TilemapHue += Time.deltaTime * TilemapColorCycleSpeed / 100;
            if (TilemapHue > 1)//Hue must be value between 0 and 1
            { TilemapHue -= 1; }
            Color NewColor = Color.HSVToRGB(TilemapHue, 1.0f, 1.0f);

            tileMap.color = NewColor;
        }
    }
    IEnumerator EndlessSpawn()
    {
        while (true)
        {
            if (timer.TimerActive)
            {
                yield return new WaitForSecondsRealtime(SpawnIntervalTime / 2);

                int[] SpawnIndexes = new int[SpawnAmount];

                int x = 0;
                for (int i = 0; i < SpawnAmount; x++)
                {
                    SpawnIndexes[i]--; //allows check for index 0, array defaults values to 0
                    int RandomIndex = Random.Range(0, SpawnLocations.Count);
                    bool ContainsIndex = false;
                    for (int j = 0; j < SpawnAmount; j++)
                    {
                        if (RandomIndex == SpawnIndexes[j])
                        {
                            ContainsIndex = true;
                            break;
                        }
                    }
                    if (!ContainsIndex)
                    {
                        SpawnIndexes[i] = RandomIndex;
                        i++;
                    }
                    //breaks to stop game from freezing if loop runs over 100 times
                    if (x > 100)
                    {
                        Debug.LogError("For Loop Overload Error: Endless Spawn");
                        break;
                    }
                }

                for (int i = 0; i < SpawnAmount; i++)
                {
                    Transform Location = SpawnLocations[SpawnIndexes[i]];
                    GameObject[] Agents = GameObject.FindGameObjectsWithTag("Agent");
                    bool DontSpawn = false;
                    foreach (GameObject Agent in Agents)
                    {
                        float Distance = Vector2.Distance(Agent.transform.position, Location.position);

                        if (Distance < NoSpawnDistance && Agent.GetComponent<Agent>().Health > 0)
                        {
                            DontSpawn = true;
                        }
                    }
                    if (!DontSpawn)
                    {
                        Instantiate(Agent, Location.position, Location.rotation);
                    }
                }
                yield return new WaitForSecondsRealtime(SpawnIntervalTime / 2);
            }
            else
            {
                yield return null;
            }
        }
    }

    //Win Trigger zone collider on World Object
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && !player.Dead)
        {
            UIObject.WinScreen.SetActive(true);
            Time.timeScale = 0.0f;
            player.Paused = true;
            timer.EndTimer(AliveEnemyTimePenalty * EnemyCount);

            //sets best time
            if (GameObject.FindGameObjectWithTag("Music Controller") != null)
            {
                int SceneIndex = SceneManager.GetActiveScene().buildIndex;
                MusicController MusicControllerScript = GameObject.FindGameObjectWithTag("Music Controller").GetComponent<MusicController>();
                float PreviousBestTime = MusicControllerScript.BestTimes[SceneIndex];
                if (timer.ClockTime < PreviousBestTime || PreviousBestTime == 0)
                {
                    MusicControllerScript.BestTimes[SceneIndex] = timer.ClockTime;
                }
            }
        }
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex +1);
        ResetTime();
    }
    public void Resume()
    {
        UIObject.PauseScreen.SetActive(false);
        player.Paused = false;
        ResetTime();
    }
    private void ResetTime()
    {
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = DefaultDeltatime;
    }
}
