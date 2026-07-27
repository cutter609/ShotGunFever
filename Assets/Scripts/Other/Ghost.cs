using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Ghost : MonoBehaviour
{
    [System.Serializable]
    public class GhostPoint
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public float Times;
    }

    public List<GhostPoint> GhostPathNew = new(); //Path that is tracked during level
    private List<GhostPoint> GhostPathSaved = new(); //Path that is played during level

    private List<GhostPoint> GhostPathPar = new();


    [Header("Settings")]
    public int PointsPerSecond = 30;

    //Ghost playing
    private int CurrentIndex;
    private bool RunGhost = false;
    private float RunStartTime;

    //Path Tracking
    private bool TrackActive = false;
    private float TrackStartTime;
    private float TrackedDeltaTime = 0;
    private bool GhostHasStarted = false;

    //References
    private Transform Player;
    private Player PlayerScript;
    private Timer timer;




    void Awake()
    {
        Player = GameObject.FindGameObjectWithTag("Player").transform;
        PlayerScript = Player.GetComponent<Player>();
        timer = GameObject.FindGameObjectWithTag("Timer").GetComponent<Timer>();
        gameObject.transform.position = Player.position;
        gameObject.transform.rotation = Player.rotation;

        StartCoroutine(PointTracking());


        MusicController MCScript = GameObject.FindGameObjectWithTag("Music Controller").GetComponent<MusicController>();
        int SceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (MCScript.BestTimes[SceneIndex] == 0)
        {
            GhostPathSaved = GhostPathPar;
            Debug.Log("Par Ghost");
        }
        else
        {
            GhostPathSaved = MCScript.SceneGhostPaths[SceneIndex];
            Debug.Log("PB Ghost");
        }
    }
    void Update()
    {
        if (timer.TimerActive && !GhostHasStarted && GameObject.FindGameObjectWithTag("Music Controller") != null)
        {

            RunGhost = true;
            RunStartTime = Time.unscaledTime;
            CurrentIndex = 1;
            GhostHasStarted = true;
        }

        if (RunGhost && GhostPathSaved != null)
        {

            if (CurrentIndex > GhostPathSaved.Count - 3)
            {
                RunGhost = false;
            }
            float TimeSinceStart = Time.unscaledTime - RunStartTime;
            float positionx = Mathf.Lerp(GhostPathSaved[CurrentIndex - 1].Position.x, GhostPathSaved[CurrentIndex].Position.x,
                                        (TimeSinceStart - GhostPathSaved[CurrentIndex - 1].Times) / (GhostPathSaved[CurrentIndex].Times - GhostPathSaved[CurrentIndex - 1].Times));

            float positiony = Mathf.Lerp(GhostPathSaved[CurrentIndex - 1].Position.y, GhostPathSaved[CurrentIndex].Position.y,
                                        (TimeSinceStart - GhostPathSaved[CurrentIndex - 1].Times) / (GhostPathSaved[CurrentIndex].Times - GhostPathSaved[CurrentIndex - 1].Times));

            float rotation = Mathf.LerpAngle(GhostPathSaved[CurrentIndex - 1].Rotation.z, GhostPathSaved[CurrentIndex].Rotation.z,
                                        (TimeSinceStart - GhostPathSaved[CurrentIndex - 1].Times) / (GhostPathSaved[CurrentIndex].Times - GhostPathSaved[CurrentIndex - 1].Times));

            gameObject.transform.position = new Vector3(positionx, positiony, 0);
            gameObject.transform.rotation = Quaternion.Euler(0, 0, rotation);

            if (TimeSinceStart > GhostPathSaved[CurrentIndex].Times)
            {
                CurrentIndex++;
            }

            //flips gun by changing y-scale so it is always upright
            if (gameObject.transform.rotation.eulerAngles.z <= 90 || gameObject.transform.rotation.eulerAngles.z >= 270)
            { gameObject.transform.localScale = new Vector3(1, 1, 1); }
            else
            { gameObject.transform.localScale = new Vector3(1, -1, 1); }
        }

        if (timer.TimerActive && !PlayerScript.Paused && !PlayerScript.Dead)
        {
            TrackActive = true;
        }
        else
        {
            TrackActive = false;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            PrefabUtility.ApplyPrefabInstance(gameObject, InteractionMode.UserAction);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            //Debug.Log(ParRuns.ParGhostPaths.Count);
            //ParRuns.x = g;
        }

    }

    IEnumerator PointTracking()
    { 
        while (true)
        {
            while (TrackActive)
            {
                TrackedDeltaTime += Time.unscaledDeltaTime;
                GhostPoint point = new()
                {
                    Position = Player.transform.position,
                    Rotation = Player.transform.rotation.eulerAngles,
                    Times = TrackedDeltaTime
                };

                GhostPathNew.Add(point);

                yield return new WaitForSeconds(1 / PointsPerSecond);
            }

            yield return null;
        }
    }
}
