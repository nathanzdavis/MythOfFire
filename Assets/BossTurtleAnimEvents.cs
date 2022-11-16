using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTurtleAnimEvents : MonoBehaviour
{
    public void GenerateStompEventL()
    {
        transform.root.GetComponent<TurtleBossController>().GenerateStompEffectsL();
    }

    public void GenerateStompEventR()
    {
        transform.root.GetComponent<TurtleBossController>().GenerateStompEffectsR();
    }
}
