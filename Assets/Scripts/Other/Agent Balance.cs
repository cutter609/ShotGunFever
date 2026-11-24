using UnityEngine;
using UnityEngine.SceneManagement;

public class AgentBalance : MonoBehaviour
{
    [HideInInspector] public float TargetRotation;
    private Rigidbody2D rb;
    //public float RotateForce = 150; //limbs and head 150, Torso 600

    private Agent Agent;

    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        Agent = gameObject.GetComponentInParent<Agent>();
    }

    void FixedUpdate()
    {
        if (Agent.AgentGrounded)
        {
            //rb.MoveRotation(Mathf.LerpAngle(rb.rotation, TargetRotation, Agent.BalaceRotateForce * Time.deltaTime));
            float t = 1.0f - Mathf.Exp(-Agent.BalaceRotateForce * Time.fixedDeltaTime);
            rb.MoveRotation(Mathf.LerpAngle(rb.rotation, TargetRotation, t));
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
            Rigidbody2D rb = gameObject.GetComponentInParent<Rigidbody2D>();
            rb.AddForce(new Vector2(RandomSideSpeed, Agent.LaunchSpeed));
            rb.AddTorque(RandomTorque);
        }
    }
}
