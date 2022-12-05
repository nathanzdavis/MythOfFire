using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffsetRotation : MonoBehaviour
{
    public float offsetRotation;
    private float offsetRotationNeg;

    public bool x, y, z;

    private void Start()
    {
        offsetRotationNeg = offsetRotation;
        offsetRotationNeg = (offsetRotationNeg > 180) ? offsetRotationNeg - 360 : offsetRotationNeg;
    }

    void LateUpdate()
    {
        if (z)
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z + offsetRotation);

        if (x && offsetRotation > 0)
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x + offsetRotation, transform.localEulerAngles.y, transform.localEulerAngles.z);
        else if (x)
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x + offsetRotation, transform.localEulerAngles.y, transform.localEulerAngles.z);
    }
}

