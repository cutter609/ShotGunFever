using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    public float ExplosionDistance = 12.0f;
    public float ExplosionForce = 42.0f;
    public float SlowModeForceMultiplier = 3.2f;

    public AudioSource Sound;


    private bool Exploded = false;
    private bool Playing = false;
    private float SlowModeForceMultiplierChanging;

    [HideInInspector] public bool DelayedExplode;
    private int WaitForFrames = 5;

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

        if (DelayedExplode)
        {
            WaitForFrames -= 1;
            if (WaitForFrames == 0)
            {
                Explode();
            }
        }

        if (Exploded)
        {
            Sound.Play();
            Playing = true;
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
            Exploded = false;
        }
        if (!Sound.isPlaying && Playing)
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

        float PlayerDistance = Vector2.Distance(Player.transform.position, gameObject.transform.position);
        if (PlayerDistance < ExplosionDistance)
        {
            Rigidbody2D Playerrb = Player.GetComponent<Rigidbody2D>();
            Vector2 ForceDirection = Player.transform.position - gameObject.transform.position;

            Playerrb.AddForce(ForceDirection.normalized * ExplosionForce * SlowModeForceMultiplierChanging
                                * ((ExplosionDistance - PlayerDistance) / ExplosionDistance));
        }

        foreach (GameObject Agent in Agents)
        {
            float Distance = Vector2.Distance(Agent.transform.position, gameObject.transform.position);

            if (Distance < ExplosionDistance)
            {
                Agent AgentScript = Agent.GetComponent<Agent>();
                AgentScript.Health = 0;
            }
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
            }
        }
        
        foreach (GameObject Barrel in Barrels)
        {
            float Distance = Vector2.Distance(Barrel.transform.position, gameObject.transform.position);
            if (Distance < ExplosionDistance)
            {
                Barrel.GetComponent<ExplosiveBarrel>().DelayedExplode = true;
            }
        }

        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        Exploded = true;
    }
}
