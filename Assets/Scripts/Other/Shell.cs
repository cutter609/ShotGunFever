using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    public float EjectAngle      = 115;
    public float EjectSpeed      = 4;
    public float RotateSpeed     = 2;


    private void Awake()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player.transform.localScale.y == -1)
        {
            EjectAngle *= -1;
            RotateSpeed *= -1;
        }

        EjectAngle = (player.transform.eulerAngles.z + EjectAngle) * Mathf.Deg2Rad;


        rb.linearVelocity = new Vector2(Mathf.Cos(EjectAngle), Mathf.Sin(EjectAngle)) * EjectSpeed + player.GetComponent<Rigidbody2D>().linearVelocity * 0.75f;
        rb.angularVelocity = RotateSpeed * 360;
    }
}
