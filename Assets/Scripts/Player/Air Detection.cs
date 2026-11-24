using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirDetection : MonoBehaviour
{
    private Player player;

    private HashSet<Collider2D> OverLappingColliders = new HashSet<Collider2D>();

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }
    private void Update()
    {
        player.InAir = !(OverLappingColliders.Count > 0);
    }
    //keeps track of how many colliders its colliding with
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Ground"))
        {
            return;
        }
        OverLappingColliders.Add(collision);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Ground"))
        {
            return;
        }
        OverLappingColliders.Remove(collision);
    }
}
