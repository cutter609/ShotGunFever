using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [HideInInspector] public bool TimerActive = false;
    [HideInInspector] public float ClockTime = 0.0f;

    private TextMeshProUGUI[] TimerDigits;

    private bool TimerEnded = false;

    private void Awake()
    {
        TimerDigits = gameObject.GetComponentsInChildren<TextMeshProUGUI>();
    }
    void Update()
    {
        if (TimerActive)
        {
            //ClockTime = Time.time - StartTime + TimePenalty;
            float TimeAdjustment = (Time.timeScale == 0) ? 1 : 1 / Time.timeScale;
            ClockTime += Time.deltaTime * TimeAdjustment;

            float minutes = (int)(ClockTime / 60);
            TimerDigits[0].text = ((int)(minutes / 10)).ToString();
            TimerDigits[1].text = ((int)(minutes % 10)).ToString();
            float seconds = (int)(ClockTime % 60);
            TimerDigits[2].text = ((int)(seconds / 10)).ToString();
            TimerDigits[3].text = ((int)(seconds % 10)).ToString();

            TimerDigits[4].text = ((int)((ClockTime - (int)ClockTime) * 10)).ToString();
            TimerDigits[5].text = (((int)((ClockTime - (int)ClockTime) * 100)) % 10).ToString();
        }
    }
    public void StartTimer()
    {
        if (!TimerActive)
        {
            TimerActive = true;
        }
    }
    public void EndTimer(float Penalty)
    {
        if (!TimerEnded)
        {
            ClockTime += Penalty;
            transform.position -= new Vector3(0, 175, 0);

            LevelController levelController = GameObject.FindGameObjectWithTag("World").GetComponent<LevelController>();
            if (levelController.Endless)
            {
                TimerDigits[TimerDigits.Length - 1].enabled = false;
            }
            else
            {
                TimerDigits[TimerDigits.Length - 1].text = "+" + Penalty.ToString() + " Seconds";
            }

            //prevents function from accidently running twice
            TimerEnded = true;
        }
    }
}