using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetection : MonoBehaviour
{
    private Player player;
    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        //player.IsGrounded = false;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Jumpable"))
        {
            //player.IsGrounded = true;
        }
    }
    private void Update()
    {
        if (!player.IsGrounded)
        {
            Debug.Log("1");
        }
    }

}
