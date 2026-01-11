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


    [HideInInspector] public string SpawnSource;
    private Rigidbody2D rb;
    private Vector2 Velocity;


    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        Velocity = transform.right * Speed;
        rb.linearVelocity = Velocity;

        Physics2D.IgnoreLayerCollision(gameObject.layer, gameObject.layer, true); 

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
                break;
            case "Button":
                Hit.GetComponent<Button>().Activate();
                break;
            case "Barrel":
                Hit.GetComponent<ExplosiveBarrel>().Explode();
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
}
