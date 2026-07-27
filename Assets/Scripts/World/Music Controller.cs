using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicController : MonoBehaviour
{
    private static MusicController instance;

    public AudioSource MainMenu;
    public AudioSource Level;

    public float[] BestTimes;

    public List<Ghost.GhostPoint>[] SceneGhostPaths;


    void Awake()
    {
        int SceneCount = SceneManager.sceneCountInBuildSettings;
        BestTimes = new float[SceneCount];
        SceneGhostPaths = new List<Ghost.GhostPoint>[SceneCount];

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            MainMenu.enabled = true;
            Level.enabled = false;
        }
        else
        {
            MainMenu.enabled = false;
            Level.enabled = true;
        }
    }

}
