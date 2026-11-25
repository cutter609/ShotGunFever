using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private HashSet<Collider2D> OverLappingColliders = new HashSet<Collider2D>();
    [HideInInspector] public bool IsGrounded = true;
    [HideInInspector] public bool InAir = false;
    [HideInInspector] public bool Dead = false;
    [HideInInspector] public bool Paused;

    private bool runonce1 = true;
    private bool runonce2 = false;


    [Header("Movement Variables")]
    public float JumpHeight          = 95;
    public float RecoilPower         = 80;
    public float MaxSpeed            = 10;
    public float RotateForceAir      = 500;
    public float RotateForceGround   = 50;

    [HideInInspector] public bool SlowActive;

    public AudioMixer AudioMixer;


    [Space(15)]
    [Header("Shooting Variables")]
    public float FireDelay           = 0.2f;
    public float SlowEffectSpeed     = 20;
    public float TimeScale           = 0.25f;
    public int BulletCount           = 5;
    public float BulletSpread        = 2;
    public Volume EffectVolume;
    public float PitchMult           = 0.4f;


    [Space(15)]
    [Header("Audio")]
    public AudioSource GunShotAudio;

    public List<GameObject> GrindingObjects = new List<GameObject>();
    public float MinGrindingSpeed = 1.0f;
    public float MaxGrindingSpeed = 15.0f;
    public float MinGrindingVolume = 0.1f;
    public float MaxGrindingVolume = 1.0f;


    [Space(15)]
    [Header("Objects")]
    public GameObject Bullet;
    public GameObject Shell;

    public Transform FirePoint;
    public Transform ShellSpawnPoint;

    private GameObject DeathScreen;

    private Rigidbody2D rb;
    private GameObject MainCamera; private readonly int PixelsPerUnit = 8;
    private Animator animator;

    private LevelController levelController;
    private UI UIObject;

    public bool WaitForThreeFrames = false;

    public float FixedFPS = 50.0f;

    private void Awake()
    {
        levelController = GameObject.FindGameObjectWithTag("World").GetComponent<LevelController>();
        UIObject = GameObject.FindGameObjectWithTag("UI").GetComponent<UI>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        MainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        //GunShotAudio = gameObject.GetComponent<AudioSource>();
        animator = gameObject.GetComponent<Animator>();

        StartCoroutine(WaitForFrames());
        StartCoroutine(Shoot());
    }

    //keeps track of how many colliders its colliding with
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Jumpable"))
        {
            return;
        }
        OverLappingColliders.Add(collision);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Jumpable"))
        {
            return;
        }
        OverLappingColliders.Remove(collision);
    }
    private void FixedUpdate()
    {
        Time.fixedDeltaTime = (1.0f / FixedFPS) * Time.timeScale;

        //rotates gun to mouse
        if (!IsGrounded && !Dead && !Paused && WaitForThreeFrames)
        {
            Vector2 mousepositionworld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = new Vector2(mousepositionworld.x - gameObject.transform.position.x,
                                            mousepositionworld.y - gameObject.transform.position.y);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            
            if (InAir)
            {
                float t = 1.0f - Mathf.Exp(-RotateForceAir * Time.fixedDeltaTime);
                rb.MoveRotation(Mathf.LerpAngle(rb.rotation, angle, t));
            }
            else
            {
                float t = 1.0f - Mathf.Exp(-RotateForceGround * Time.fixedDeltaTime);
                rb.MoveRotation(Mathf.LerpAngle(rb.rotation, angle, t));
            }
        }
    }
    void Update()
    {
        IsGrounded = OverLappingColliders.Count > 0;

        //Moves camera to player (causes jittering if not first)
        Vector3 CameraPosition = Camera.main.transform.position;
        CameraPosition.x = (int)Mathf.Round(gameObject.transform.position.x * PixelsPerUnit) / PixelsPerUnit;
        CameraPosition.y = (int)Mathf.Round(gameObject.transform.position.y * PixelsPerUnit) / PixelsPerUnit;
        CameraPosition.z = -10;

        //MainCamera.transform.position = CameraPosition;
        MainCamera.transform.position = gameObject.transform.position + new Vector3(0, 0, -10);

        //adds force to jump
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded && !Dead && !Paused)
            { rb.AddForce(Vector2.up * JumpHeight); }

        /*
        //rotates gun to mouse
        if (!IsGrounded && !Dead && !Paused && WaitForThreeFrames)
        {
            Vector2 mousepositionworld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = new Vector2(mousepositionworld.x - gameObject.transform.position.x, 
                                            mousepositionworld.y - gameObject.transform.position.y);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            if (InAir)
            {
                rb.MoveRotation(Mathf.LerpAngle(rb.rotation, angle, RotateForceAir * Time.deltaTime));
            }
            else
            {
                rb.MoveRotation(Mathf.LerpAngle(rb.rotation, angle, RotateForceGround * Time.deltaTime));
            }
        }
        */

        //flips gun by changing y-scale so it is always upright
        if (gameObject.transform.rotation.eulerAngles.z <= 90 || gameObject.transform.rotation.eulerAngles.z >= 270)
        { gameObject.transform.localScale = new Vector3(1, 1, 1); }
        else
        { gameObject.transform.localScale = new Vector3(1, -1, 1); }

        //When slow motion is active, sets time scale, fixed delta time, negates recoil, pitches audio, 
        if (Input.GetKey(KeyCode.Mouse1) && !Dead && !Paused)
        {
            if (runonce1)
            {
                Time.timeScale = TimeScale;
                JumpHeight /= TimeScale;
                runonce1 = false;
                SlowActive = true;
                AudioMixer.SetFloat("SFX Pitch", PitchMult);
            }
            if (EffectVolume.weight < 1)
                { EffectVolume.weight += SlowEffectSpeed * Time.deltaTime; }
            runonce2 = true;
        }
        else
        {
            if (runonce2)
            {
                if (!Paused)
                {
                    Time.timeScale = 1;
                }
                JumpHeight *= TimeScale;
                SlowActive = false;
                runonce2 = false;
                AudioMixer.SetFloat("SFX Pitch", 1.0f);
            }
            if (EffectVolume.weight > 0)
                { EffectVolume.weight -= SlowEffectSpeed * Time.deltaTime; }
            runonce1 = true;
        }

        //Quick Restart
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Time.timeScale = 1;
        }

        //grinding sound effect
        if (IsGrounded && !Dead && Mathf.Abs(rb.linearVelocityX) > MinGrindingSpeed && Time.timeScale > 0)
        {
            for (int i = 0; i < GrindingObjects.Count; ++i)
            {
                GrindingObjects[i].SetActive(true);

                //sets color of grinding particles
                ParticleSystem par = GrindingObjects[i].GetComponent<ParticleSystem>();
                var main = par.main;
                main.startColor = levelController.TileMapColor;
            }
            if (Mathf.Abs(rb.linearVelocityX) >= MaxGrindingSpeed)
            {
                GrindingObjects[0].GetComponent<AudioSource>().volume = MaxGrindingVolume;
            }
            else
            {
                GrindingObjects[0].GetComponent<AudioSource>().volume =
                    (Mathf.Abs(rb.linearVelocityX) - MinGrindingSpeed) / (MaxGrindingSpeed - MinGrindingSpeed)
                    * (MaxGrindingVolume - MinGrindingVolume) + MinGrindingVolume;
            }
            for (int i = 0; i < GrindingObjects.Count; ++i)
            {
                int looking = (int)gameObject.transform.localScale.y;// -1 if looking left, 1 if looking right
                int moving = (int)(rb.linearVelocityX / Mathf.Abs(rb.linearVelocityX)); //-1 if moving left, 1 if moving right
                GrindingObjects[i].transform.localScale = new Vector3(looking * moving, looking, 1); //accounts for flipping and rotating so particles always spray in the right direction
            }
        }
        else
        {
            for (int i = 0; i < GrindingObjects.Count; ++i)
            {
                GrindingObjects[i].SetActive(false);
            }
        }
    }

    public void BulletHit()
    {
        if (!Dead)
        {
            Dead = true;
            UIObject.DeathScreen.SetActive(true);
            StartCoroutine(ResetMouseCursor());
        }
    }
    private void PlayAudioRandomPitch(AudioSource audio)
    {
        audio.pitch = Random.Range(0.95f, 1.05f);
        audio.Play();
    }
    //allows triggers to detect colliders so player doesn't immediatly rotate on load
    IEnumerator WaitForFrames()
    {
        for (int i = 0; i < 3;  i++)
        {
            yield return null;
        }
        WaitForThreeFrames = true;
    }
    //resets cursor to center of screen
    IEnumerator ResetMouseCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        yield return null;
        Cursor.lockState = CursorLockMode.None;
    }
    IEnumerator Shoot()
    {
        while (true)
        {
            //limits max velocity to MaxSpeed, adds force to move gun, instantiates bullets
            if (Input.GetKeyDown(KeyCode.Mouse0) && !Dead && !Paused)
            {
                if (rb.linearVelocity.x >= MaxSpeed && !SlowActive)
                { rb.linearVelocity = new Vector2(MaxSpeed, rb.linearVelocity.y); }

                else if (rb.linearVelocity.x <= -MaxSpeed && !SlowActive)
                { rb.linearVelocity = new Vector2(-MaxSpeed, rb.linearVelocity.y); }


                Vector2 ForceDirection = Quaternion.Euler(0, 0, transform.localEulerAngles.z) * Vector2.left * RecoilPower;
                if (!SlowActive && !Dead && !Paused)
                {
                    if (rb.linearVelocity.y < MaxSpeed && !SlowActive)
                    {
                        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                        rb.AddForce(ForceDirection);
                    }
                    else if (ForceDirection.y < 0)
                    { rb.AddForce(ForceDirection); }

                    else if (!SlowActive)
                    { rb.AddForce(new Vector2(ForceDirection.x, 0)); }
                }

                for (int i = 0; i < BulletCount; i++)
                {
                    GameObject BulletObject = Instantiate(Bullet, FirePoint.position, Quaternion.Euler(0, 0, FirePoint.rotation.eulerAngles.z
                                                         + (BulletCount - 1) / 2 * BulletSpread - i * BulletSpread));
                    Physics2D.IgnoreCollision(BulletObject.GetComponent<Collider2D>(), gameObject.GetComponent<Collider2D>());
                }
                PlayAudioRandomPitch(GunShotAudio);
                animator.SetTrigger("Shoot");

                StartCoroutine(SpawnShell(0.11f));
                yield return new WaitForSeconds(FireDelay);
            }
            else
            {
                yield return null;
            }
        }
    }
    IEnumerator SpawnShell(float TimeBeforeSpawn)
    {
        yield return new WaitForSeconds(TimeBeforeSpawn);
        Instantiate(Shell, ShellSpawnPoint.position, ShellSpawnPoint.rotation);
    }
}
