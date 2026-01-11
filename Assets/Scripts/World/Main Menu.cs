using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    [Header("Objects")]
    public float MinSpawnDelay = 0.5f;
    public float MaxSpawnDelay = 4.0f;

    public GameObject Agent;
    public int MaxSpin = 50;

    public GameObject QuitButton;
    public AudioMixer AudioMixer;
    public UnityEngine.UI.Slider Slider;

    public TextMeshProUGUI[] BestTimesTextObjects;

    private void Awake()
    {
        AudioMixer.GetFloat("Master", out float DecibelLevel);
        Slider.value = Mathf.Pow(10, DecibelLevel / 20);

        StartCoroutine(SpawnAgent());

        if (UnityEngine.Application.platform == RuntimePlatform.WebGLPlayer)
        {
            QuitButton.SetActive(false);
        }

        GameObject times = GameObject.FindGameObjectWithTag("Music Controller");
        float[] BestTimesFloat = times.GetComponent<MusicController>().BestTimes;
        for (int i = 0; i < BestTimesFloat.Length; i++)
        {
            if (BestTimesTextObjects[i] != null)
            {
                BestTimesTextObjects[i].text = FloatToTime(BestTimesFloat[i]);
            }
        }
    }
    public void LoadLevel(int SceneIndex)
    {
        SceneManager.LoadScene(SceneIndex);
    }
    public void Quit()
    {
        UnityEngine.Application.Quit();
    }
    public void SetVolumeMaster(float volume)
    {
        AudioMixer.SetFloat("Master", Mathf.Log10(volume) * 20);
    }
    public string FloatToTime(float time)
    {
        string[] TimerDigits = new string[6];
        float minutes = (int)(time / 60);
        TimerDigits[0] = ((int)(minutes / 10)).ToString();
        TimerDigits[1] = ((int)(minutes % 10)).ToString();
        float seconds = (int)(time % 60);
        TimerDigits[2] = ((int)(seconds / 10)).ToString();
        TimerDigits[3] = ((int)(seconds % 10)).ToString();

        TimerDigits[4] = ((int)((time - (int)time) * 10)).ToString();
        TimerDigits[5] = (((int)((time - (int)time) * 100)) % 10).ToString();

        string NewString = "";
        for (int i = 0; i < TimerDigits.Length; i++)
        {
            NewString += TimerDigits[i];
            if (i == 1 || i == 3)
            {
                NewString += ":";
            }
        }
        return NewString;
    }
    IEnumerator SpawnAgent()
    {
        while (true)
        {
            float CameraSize = Camera.main.aspect * Camera.main.orthographicSize; //half actual horizontal size
            Vector2 Position = new Vector2(Random.Range(-CameraSize, CameraSize), Camera.main.orthographicSize + 5);
            float Rotation = Random.Range(-MaxSpin, MaxSpin);

            GameObject AgentInstance = Instantiate(Agent, Position, Quaternion.Euler(0, 0, Rotation));
            AgentInstance.GetComponentInChildren<Rigidbody2D>().AddTorque(Rotation);

            float DelayTime = Random.Range(MinSpawnDelay, MaxSpawnDelay);
            yield return new WaitForSeconds(DelayTime);
        }
    }
}
