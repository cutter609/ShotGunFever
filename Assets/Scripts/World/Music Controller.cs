using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicController : MonoBehaviour
{
    private static MusicController instance;

    public AudioSource MainMenu;
    public AudioSource Level;

    public float[] BestTimes;
    public bool? GhostEnabled = true;

    public List<Ghost.GhostPoint>[] SceneGhostPaths;

    public TextMeshProUGUI FPS;
    private const int ArrayLength = 100;
    private double[] FPSArray = new double[ArrayLength];
    private int ArrayIndex = 0;


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

        StartCoroutine(FPSCounter());
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

        if (ArrayIndex > ArrayLength - 1)
        {
            ArrayIndex -= ArrayLength;
        }
        FPSArray[ArrayIndex] = 1.0f / Time.unscaledDeltaTime;
        ArrayIndex++;
    }

    IEnumerator FPSCounter()
    {
        while (true)
        {
            FPS.text = ((int)ArrayAverage(FPSArray)).ToString();
            yield return new WaitForSeconds(1);
        }
    }
    double ArrayAverage(double[] array)
    {
        double avg = 0;
        int count = array.Length;
        for (int i = 0; i < count; ++i)
        {
            avg += array[i];
        }
        return avg / count;
    }
}
