using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    public GameObject ObjectsOn;
    public GameObject ObjectsOff;

    public void Activate() 
    {
        ObjectsOn.SetActive(false);
        ObjectsOff.SetActive(true);
    }
}
