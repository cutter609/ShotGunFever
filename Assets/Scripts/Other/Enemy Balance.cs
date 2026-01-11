using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyBalance : MonoBehaviour
{
    public float TargetRotation;
    private Rigidbody2D rb;
    public float RotateForce = 150; //limbs and head 150, Torso 600

    private Agent Agent;

    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        Agent = gameObject.GetComponentInParent<Agent>();
    }

    void Update()
    {
        if (Agent.AgentGrounded)
        {
            rb.MoveRotation(Mathf.LerpAngle(rb.rotation, TargetRotation, RotateForce * Time.deltaTime));
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        Agent.AgentGrounded = false;
    }
    private void OnCollisionStay2D(Collision2D collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Agent.AgentGrounded = true;
        }
    }
    void OnMouseDown()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0 && Agent.Health > 0)
        {
            float RandomSideSpeed = Random.Range(-Agent.MaxSideSpeed, Agent.MaxSideSpeed);
            float RandomTorque = Random.Range(-Agent.MaxTorque, Agent.MaxTorque);
            gameObject.GetComponentInParent<Rigidbody2D>().AddForce(new Vector2(RandomSideSpeed, Agent.LaunchSpeed));
        }
    }
}
