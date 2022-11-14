using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineMovement : MonoBehaviour
{
    private float cycle; // This variable increases with time and allows the sine to produce numbers between -1 and 1.
    private Vector3 basePosition; // This variable maintains the location of the object without applying sine changes

    public Transform target;

    public float waveSpeed = 1f; // Higher make the wave faster
    public float bonusHeight = 1f; // Set higher if you want more wave intensity
    public float speed = 1f; // more value going faster to target

    public int healthToRestore;
    public AudioClip lanternPickup;
    public float activeTime;

    public void Start()
    {
        basePosition = transform.position;
        Destroy(gameObject, activeTime);
    }


    void Update()
    {
        cycle += Time.deltaTime * waveSpeed;

        transform.position = basePosition + (Vector3.up * bonusHeight) * Mathf.Sin(cycle);

        if (target) basePosition = Vector3.MoveTowards(basePosition, target.position, Time.deltaTime * speed);
    }
}