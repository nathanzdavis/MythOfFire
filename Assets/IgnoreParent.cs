using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreParent : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
            return; // do nothing
        Debug.Log("child goes ouch!");
    }
}
