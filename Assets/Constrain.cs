using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constrain : MonoBehaviour
{
    public bool constrainX;
    public bool constrainY;
    public bool constrainZ;
    private Vector3 origRotation;

    void Start()
    {
        origRotation = transform.eulerAngles;
    }

    void FixedUpdate()
    {
        if (constrainX)
            transform.eulerAngles = new Vector3(origRotation.x, transform.eulerAngles.y, transform.eulerAngles.z);
        if (constrainY)
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, origRotation.y, transform.eulerAngles.z);
        if (constrainZ)
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, origRotation.z);
    }
}
