using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExplosiveBarrel : MonoBehaviour
{
    public float DamagePerLimb = 2.0f;
    public float ExplosionDistance = 12.0f;
    public float ExplosionForce = 42.0f;
    public float SlowModeForceMultiplier = 3.2f;
    public float BarrelPushMult = 5.0f;

    public ParticleSystem Particles1;
    public ParticleSystem Particles2;

    private float SlowModeForceMultiplierChanging;

    private bool InMainMenu = false;
    public bool Exploded = false;

    public float ExplodeDelayTimeMin = 0.1f;
    public float ExplodeDelayTimeMax = 0.5f;
    private float ExplodeDelayTime;

    private float ExplodeTime = 0.0f;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            InMainMenu = true;
        }
        ExplodeDelayTime = UnityEngine.Random.Range(ExplodeDelayTimeMin, ExplodeDelayTimeMax);
    }
    private void Update()
    {
        if (Time.timeScale == 1)
        {
            SlowModeForceMultiplierChanging = 1;
        }
        else
        {
            SlowModeForceMultiplierChanging = SlowModeForceMultiplier;
        }

        if (ExplodeTime != 0.0f && ExplodeTime + ExplodeDelayTime < Time.time)
        {
            Explode();
        }

        if (InMainMenu && transform.position.y < -25.0f)
        {
            Destroy(gameObject);
        }
    }

    public void Explode()
    {
        GameObject Player = GameObject.FindGameObjectWithTag("Player");
        GameObject[] Agents = GameObject.FindGameObjectsWithTag("Agent");
        GameObject[] Limbs = GameObject.FindGameObjectsWithTag("Agent Limb");
        GameObject[] Barrels = GameObject.FindGameObjectsWithTag("Barrel");

        float PlayerDistance = 0;
        if (Player != null)
        {
            PlayerDistance = Vector2.Distance(Player.transform.position, gameObject.transform.position);
        }
        if (PlayerDistance < ExplosionDistance && PlayerDistance != 0)
        {
            Rigidbody2D Playerrb = Player.GetComponent<Rigidbody2D>();
            Vector2 ForceDirection = Player.transform.position - gameObject.transform.position;

            Playerrb.AddForce(ForceDirection.normalized * ExplosionForce * SlowModeForceMultiplierChanging
                                * ((ExplosionDistance - PlayerDistance) / ExplosionDistance));
        }
        
        foreach (GameObject Limb in Limbs)
        {
            float Distance = Vector2.Distance(Limb.transform.position, gameObject.transform.position);
            if (Distance < ExplosionDistance)
            {
                Rigidbody2D Limbrb = Limb.GetComponent<Rigidbody2D>();

                Vector2 ForceDirection = Limb.transform.position - gameObject.transform.position;
                Limbrb.AddForce(ForceDirection.normalized * ExplosionForce * SlowModeForceMultiplierChanging
                                * ((ExplosionDistance - Distance) / ExplosionDistance));

                Limb.GetComponentInParent<Agent>().Health -= (ExplosionDistance - Distance) * DamagePerLimb;
            }
        }
        
        foreach (GameObject Barrel in Barrels)
        {
            float Distance = Vector2.Distance(Barrel.transform.position, gameObject.transform.position);
            if (Distance < ExplosionDistance && Distance > 0.1f)
            {
                Rigidbody2D Barrelrb = Barrel.GetComponent<Rigidbody2D>();

                Vector2 ForceDirection = Barrel.transform.position - gameObject.transform.position;
                Barrelrb.AddForce(ForceDirection.normalized * ExplosionForce * SlowModeForceMultiplierChanging * BarrelPushMult
                                * ((ExplosionDistance - Distance) / ExplosionDistance));

                ExplosiveBarrel BarrelScript = Barrel.GetComponent<ExplosiveBarrel>();
                if (BarrelScript.ExplodeTime == 0)
                {
                    BarrelScript.ExplodeTime = Time.time;
                }
            }
        }

        Instantiate(Particles1, gameObject.transform.position, gameObject.transform.rotation);
        Instantiate(Particles2, gameObject.transform.position, gameObject.transform.rotation);
        Destroy(gameObject);
        Exploded = true;
    }
    // explodes on click in main menu
    void OnMouseDown()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0 && !Exploded)
        {
            Explode();
        }
    }
}
