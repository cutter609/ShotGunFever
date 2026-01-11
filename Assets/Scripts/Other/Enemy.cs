using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    public LayerMask GroundLayer;
    public LayerMask PlayerLayer;

    private GameObject Player;
    private bool Activate = false;
    public float ActivateDistance;
    public float SightDistance;
    public GameObject Head;
    public GameObject Arm;
    private Rigidbody2D rb;
    private HingeJoint2D Joint;
    private JointMotor2D Motor;
    public float MotorSpeed;

    public HingeJoint2D HeadJoint;
    public HingeJoint2D ArmLeftJoint;
    public HingeJoint2D ArmRightJoint;
    public HingeJoint2D ThighLeftJoint;
    public HingeJoint2D ThighRightJoint;
    public HingeJoint2D CalfLeftJoint;
    public HingeJoint2D CalfRightJoint;

    public Transform FirePoint;
    public GameObject Bullet;

    public float FireDelay;
    private float LastFireTime = 0;

    public int Health;

    public float HoverDistance;
    public float Power;
    public float sinAdjustment;
    public float Speed;

    private float Distance;

    private bool MainMenu;
    public float MainMenuRandomTorque;
    public GameObject Torso;
    private Rigidbody2D TorsoRB;

    void Awake()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        Joint = Arm.GetComponent<HingeJoint2D>();
        Motor = Joint.motor;
        rb = Head.GetComponent<Rigidbody2D>();
        TorsoRB = Torso.GetComponent<Rigidbody2D>();

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            Activate = true;
            MainMenu = true;
            float RandomTorque = Random.Range(0, MainMenuRandomTorque);
            TorsoRB.AddTorque(RandomTorque);
        }
        else
        {
            MainMenu = false;
        }
    }

    void Update()
    {
        if (Health > 0)
        {
            if (!MainMenu)
            {
                Distance = Vector2.Distance(Head.transform.position, Player.transform.position);
            }

            if (!Activate)
            {
                rb.linearVelocity = Vector2.zero;
            }
            if (Distance < ActivateDistance && !MainMenu)
            {
                RaycastHit2D ActivateHit = Physics2D.Raycast(Head.transform.position, Player.transform.position - Head.transform.position, Distance, GroundLayer);
                if (ActivateHit.collider == null)
                {
                    Activate = true;
                }
            }

            if (Distance < SightDistance && !MainMenu)
            {
                RaycastHit2D Hit = Physics2D.Raycast(Head.transform.position, Player.transform.position - Head.transform.position, Distance, GroundLayer);
                if (Hit.collider == null)
                {
                    Vector2 direction = Player.transform.position - Arm.transform.position;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    float currentAngle = Arm.transform.eulerAngles.z;
                    if (currentAngle > 180)
                    {
                        currentAngle -= 360;
                    }

                    float AngleDifference = Mathf.DeltaAngle(currentAngle, angle);

                    Motor.motorSpeed = Mathf.Sign(AngleDifference) * MotorSpeed;
                    Joint.motor = Motor;

                    if (LastFireTime + FireDelay < Time.time)
                    {
                        Instantiate(Bullet, FirePoint.position, FirePoint.rotation);
                        LastFireTime = Time.time;
                    }
                }
            }

            RaycastHit2D HoverHit = Physics2D.Raycast(Head.transform.position, Vector2.down, 100, GroundLayer);
            if (HoverHit.distance < HoverDistance + Mathf.Sin(Time.time * Speed) * sinAdjustment && Health > 0)
            {
                rb.AddForce(Vector2.up * Power);
            }
        }
        else
        {
            Motor.motorSpeed = 0;
            Joint.motor = Motor;
            HeadJoint.enabled = false;
            ArmLeftJoint.enabled = false;
            ArmRightJoint.enabled = false;
            ThighLeftJoint.enabled = false;
            ThighRightJoint.enabled = false;
            CalfLeftJoint.enabled = false;
            CalfRightJoint.enabled = false;
        }

        if (MainMenu)
        {
            if (Head.transform.position.y < -30 || Head.transform.position.y > 50)
            {
                Destroy(gameObject);
            }
        }
    }
}