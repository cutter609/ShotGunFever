using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Agent : MonoBehaviour
{
    public bool StartFacingLeft = false;

    [Header("Agent")]
    public int Health                   = 50;
    public int DetectionDistance        = 30;
    public float TargetingDelayTime     = 1;
    public float ShootingDelay          = 1;

    private bool MovingToShoot = false;
    private LayerMask ExcludeAfterDeathLayers;
    [HideInInspector] public bool AgentGrounded;


    [Space(15)]
    [Header("Shooting")]
    public Rigidbody2D ShootingArmRb1;
    public Rigidbody2D ShootingArmRb2;
    public float ArmRotateForce         = 500;

    public float AngleAdjustment        = 23;
    public float LeftSideMulitplier     = 0.7f;

    public int AngleShootRange          = 15;

    private LayerMask PlayerBlockLineOfSightlayers;

    public GameObject Head;
    public GameObject Torso;

    public HingeJoint2D ForeLeft;
    public Rigidbody2D Hand;
    public float MoveSpeed              = 100;
    public Transform SecondHandPoint;
    public Transform GunPoint;
    public GameObject Gun;


    [Space(15)]
    [Header("Main Menu Settings")]
    public float ReTargetTime           = 3;
    public float LaunchSpeed            = 1000.0f;
    public float MaxSideSpeed           = 200.0f;
    public float MaxTorque              = 50.0f;

    private bool InMainMenu = true;


    private GameObject Player = null;

    public GameObject Bullet;
    public Transform FirePoint;


    private bool RunOnce = true;
    private LevelController levelController = null;

    private void Awake()
    {
        if (StartFacingLeft)
        {
            Torso.transform.localScale = new Vector3(-1, 1, 1);

            Head.GetComponent<SpriteRenderer>().enabled = false;
            Head.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = true;
            Torso.GetComponent<SpriteRenderer>().enabled = false;
            Torso.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = true;
        }

        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            InMainMenu = false;
            Player = GameObject.FindGameObjectWithTag("Player");
            levelController = GameObject.FindGameObjectWithTag("World").GetComponent<LevelController>();
        }


        //Must be called in Awake or Start
        PlayerBlockLineOfSightlayers    = (1 << LayerMask.NameToLayer("Ground"));
        ExcludeAfterDeathLayers         = (1 << LayerMask.NameToLayer("Enemy")) | 
                                        (1 << LayerMask.NameToLayer("Player")) | 
                                        (1 << LayerMask.NameToLayer("Bullet"));

        StartCoroutine(DelayedTargeting());
        StartCoroutine(Target());
        if (InMainMenu)
        {
            StartCoroutine(MainMenuTargeting());
        }
    }

    void Update()
    {
        if (Health > 0)
        {
            if (MovingToShoot && Player != null)
            {
                Vector2 direction = Player.transform.position - ShootingArmRb1.gameObject.transform.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;

                //puts angle through negative x squared function to get adjustment
                float x = Mathf.Abs(Mathf.DeltaAngle(0.0f, angle));
                float FinalAdjustment = -AngleAdjustment / 8100.0f * x * x + AngleAdjustment / 45 * x;

                //left side needs lower value for some reason
                if (Mathf.DeltaAngle(0.0f, angle) < 0)
                {
                    FinalAdjustment *= -LeftSideMulitplier;
                }
                if (Time.timeScale < 1)
                {
                    FinalAdjustment *= 0;
                }

                //Moves arm
                ShootingArmRb1.MoveRotation(Mathf.LerpAngle(ShootingArmRb1.rotation, angle + FinalAdjustment, ArmRotateForce * Time.deltaTime));
                ShootingArmRb2.MoveRotation(Mathf.LerpAngle(ShootingArmRb2.rotation, ShootingArmRb1.rotation, ArmRotateForce * 3 * Time.deltaTime));

                //Faces player
                if (direction.x < 0)
                {
                    Torso.transform.localScale = new Vector3(-1, 1, 1);

                    Head.GetComponent<SpriteRenderer>().enabled = false;
                    Head.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = true;
                    Torso.GetComponent<SpriteRenderer>().enabled = false;
                    Torso.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = true;
                }
                else
                {
                    Torso.transform.localScale = new Vector3(1, 1, 1);

                    Head.GetComponent<SpriteRenderer>().enabled = true;
                    Head.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    Torso.GetComponent<SpriteRenderer>().enabled = true;
                    Torso.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                }
            }
        }
        else
        {
            //Death
            if (RunOnce)
            {
                //makes body fall apart
                HingeJoint2D[] LimbJoints = gameObject.GetComponentsInChildren<HingeJoint2D>();
                foreach (HingeJoint2D joint in LimbJoints)
                {
                    joint.enabled = false;
                }

                //disables collision with body
                Rigidbody2D[] rbs = gameObject.GetComponentsInChildren<Rigidbody2D>();
                foreach (Rigidbody2D rb in rbs)
                {
                    rb.excludeLayers = ExcludeAfterDeathLayers;
                }

                //stops agent from trying to stay upright
                AgentBalance[] BalanceScript = gameObject.GetComponentsInChildren<AgentBalance>();
                foreach (AgentBalance Script in BalanceScript)
                {
                    Script.enabled = false;
                }

                if (!InMainMenu)
                {
                    levelController.EnemyCount -= 1;
                }

                RunOnce = false;
            }
        }
        if (InMainMenu && Torso.transform.position.y < -25)
        {
            Destroy(gameObject);
        }
    }
    public void BulletHit(int Damage)
    {
        Health -= Damage;
    }
    /*
    IEnumerator MoveRight(float Seconds)
    {
        LeftLegRb.AddForce(Vector2.right * Speed * Time.deltaTime);
        yield return new WaitForSeconds(Seconds);
        RightLegRb.AddForce(Vector2.right * Speed * Time.deltaTime);
    }
    IEnumerator MoveLeft(float Seconds)
    {
        RightLegRb.AddForce(Vector2.left * Speed * Time.deltaTime);
        yield return new WaitForSeconds(Seconds);
        LeftLegRb.AddForce(Vector2.left * Speed * Time.deltaTime);
    }
    */
    IEnumerator MainMenuTargeting()
    {
        while (Health > 0 && InMainMenu)
        {
            GameObject[] AgentsList = GameObject.FindGameObjectsWithTag("Agent");
            float MinDistance = DetectionDistance;
            int ClosestIndex = 0;
            for (int i = 0; i < AgentsList.Length; i++)
            {
                float Distance = Vector2.Distance(AgentsList[i].transform.GetChild(0).gameObject.transform.position, Head.transform.position);
                if (Distance < MinDistance && Distance > 2)
                {
                    MinDistance = Distance;
                    ClosestIndex = i;
                }
            }

            Player = AgentsList[ClosestIndex].transform.GetChild(0).gameObject;

            yield return new WaitForSeconds(ReTargetTime);
        }
    }
    IEnumerator DelayedTargeting()
    {
        while (Health > 0)
        {
            if (Player != null)
            {
                float DistanceToPlayer = Vector2.Distance(Player.transform.position, Head.transform.position);
                RaycastHit2D Hit = Physics2D.Raycast(Head.transform.position, Player.transform.position - Head.transform.position,
                                                    DistanceToPlayer, PlayerBlockLineOfSightlayers);
                if (Hit.collider == null && DistanceToPlayer < DetectionDistance)
                {
                    yield return new WaitForSeconds(TargetingDelayTime);
                    MovingToShoot = true;
                }
                else
                {
                    MovingToShoot = false;
                }
            }
            yield return null;
        }
    }
    IEnumerator Target()
    {
        while (Health > 0)
        {
            if (MovingToShoot & Player != null)
            {
                Vector2 direction = Player.transform.position - FirePoint.gameObject.transform.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                Vector3 LeftSideFix = Vector3.zero;
                if (direction.x < 0)
                {
                    LeftSideFix = new Vector3(0, 0, 180);
                }
                if (Mathf.Abs(Mathf.DeltaAngle(FirePoint.transform.rotation.eulerAngles.z, angle + LeftSideFix.z)) < AngleShootRange)
                {
                    GameObject BulletObject = Instantiate(Bullet, FirePoint.position, Quaternion.Euler(FirePoint.rotation.eulerAngles + LeftSideFix));
                    Collider2D[] LimbColliders = gameObject.GetComponentsInChildren<Collider2D>();
                    foreach (Collider2D collider in LimbColliders)
                    {
                        Physics2D.IgnoreCollision(BulletObject.GetComponent<Collider2D>(), collider);
                    }
                    //PlayAudioRandomPitch(GunShotAudio);
                    yield return new WaitForSeconds(ShootingDelay);
                }
                else
                {
                    yield return null;
                }
            }
            else
            {
                yield return null;
            }
        }
    }
    private void PlayAudioRandomPitch(AudioSource audio)
    {
        float CurrentPitch = audio.pitch;
        audio.pitch = CurrentPitch + Random.Range(-0.05f, 0.05f);
        audio.Play();
        audio.pitch = CurrentPitch;
    }
}
