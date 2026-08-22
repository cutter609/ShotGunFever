using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bullet : MonoBehaviour
{
    [Header("Variables")]
    public float Speed           = 40;
    public float MoveStrength    = 30;
    public int Damage            = 10;
    public int DestroyDelay      = 10;

    [Space(15)]
    [Header("Particles")]
    public ParticleSystem BloodParticles;
    public ParticleSystem GroundHitParticles;


    [HideInInspector] public string SpawnSource;
    private Rigidbody2D rb;
    private Vector2 Velocity;

    private LevelController levelController;




    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        Velocity = transform.right * Speed;
        rb.linearVelocity = Velocity;

        //ignores collision with other bullets
        Physics2D.IgnoreLayerCollision(gameObject.layer, gameObject.layer, true);

        levelController = GameObject.FindGameObjectWithTag("World").GetComponent<LevelController>();

        StartCoroutine(DestroyAfterDelay());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject Hit = collision.gameObject;
        switch (Hit.tag)
        {
            case "Player":
                Hit.GetComponent<Player>().BulletHit();
                break;
            case "Agent Limb":
                Hit.GetComponentInParent<Agent>().BulletHit(Damage);
                MoveObject(Hit);
                ParticleSystem ParticleAgent = Instantiate(BloodParticles, transform.position, transform.rotation);
                PlayAudioRandomPitch(ParticleAgent.GetComponent<AudioSource>());
                break;
            case "Button":
                Hit.GetComponent<Button>().Activate();
                break;
            case "Barrel":
                Hit.GetComponent<ExplosiveBarrel>().Explode();
                break;
            case "Ground":
                float zRotation = Mathf.Atan2(collision.contacts[0].normal.y, collision.contacts[0].normal.x) * Mathf.Rad2Deg;
                ParticleSystem ParticleGround = Instantiate(GroundHitParticles, transform.position, Quaternion.Euler(0, 0, zRotation - 90));
                var main = ParticleGround.main;
                main.startColor = levelController.TileMapColor;
                PlayAudioRandomPitch(ParticleGround.GetComponent<AudioSource>());
                break;
        }
        Rigidbody2D Hitrb = Hit.GetComponent<Rigidbody2D>();
        if (Hitrb != null)
        {
            MoveObject(Hit);
        }
        Destroy(gameObject);
    }
    private void MoveObject(GameObject obj)
    {
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        rb.AddForceAtPosition(Velocity * MoveStrength / 100, transform.position, ForceMode2D.Force);
    }
    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(DestroyDelay);
        Destroy(gameObject);
    }
    private void PlayAudioRandomPitch(AudioSource audio)
    {
        float CurrentPitch = audio.pitch;
        audio.pitch = CurrentPitch + Random.Range(-0.05f, 0.05f);
        audio.Play();
        audio.pitch = CurrentPitch;
    }
}
