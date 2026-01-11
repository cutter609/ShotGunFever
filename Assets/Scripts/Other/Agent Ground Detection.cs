using Unity.VisualScripting;
using UnityEngine;

public class AgentGroundDetection : MonoBehaviour
{
    [HideInInspector] public bool AgentGrounded = false; 
    private void OnTriggerExit2D(Collider2D collision)
    {
        AgentGrounded = false; 
    }
    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            AgentGrounded = true;
        }
    }
}
